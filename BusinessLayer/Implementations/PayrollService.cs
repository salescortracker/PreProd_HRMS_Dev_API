using BusinessLayer.DTOs;
using BusinessLayer.Interfaces;
using DataAccessLayer.DBContext;
using Microsoft.EntityFrameworkCore;

namespace BusinessLayer.Implementations
{
    public class PayrollService : IPayrollService
    {
        private readonly HRMSContext _context;

        public PayrollService(HRMSContext context)
        {
            _context = context;
        }

        /* ============================================================
           ATTENDANCE SUMMARY
        ============================================================ */

        private async Task<(int workingDays, int presentDays, int leaveDays, int halfDays)>
        GetEmployeeAttendanceSummary(int employeeId, int month, int year)
        {
            var employee = await _context.Users
                .Where(x => x.UserId == employeeId)
                .Select(x => new
                {
                    x.EmployeeCode,
                    x.CompanyId,
                    x.RegionId
                })
                .FirstOrDefaultAsync();

            if (employee == null)
                return (0, 0, 0, 0);

            DateOnly startDate = new DateOnly(year, month, 1);
            DateOnly endDate = new DateOnly(year, month, DateTime.DaysInMonth(year, month));

            var attendance = await _context.EmployeeAttendances
                .Where(a =>
                    a.EmployeeCode == employee.EmployeeCode &&
                    a.CompanyId == employee.CompanyId &&
                    a.RegionId == employee.RegionId &&
                    a.AttendanceDate >= startDate &&
                    a.AttendanceDate <= endDate)
                .ToListAsync();

            int present = attendance.Count(a => a.Status == "Present");

            int leave = attendance.Count(a =>
                a.Status == "SickLeave" ||
                a.Status == "CasualLeave" ||
                a.Status == "PaidLeave");

            int half = attendance.Count(a => a.Status == "HalfDay");

            int totalDays = DateTime.DaysInMonth(year, month);

            int weekendDays = Enumerable.Range(1, totalDays)
                .Select(d => new DateTime(year, month, d))
                .Count(d => d.DayOfWeek == DayOfWeek.Saturday ||
                            d.DayOfWeek == DayOfWeek.Sunday);

            int workingDays = totalDays - weekendDays;

            return (workingDays, present, leave, half);
        }

        /* ============================================================
           EXPENSE CALCULATION
        ============================================================ */

        private async Task<decimal> GetApprovedExpenses(int employeeId, int month, int year)
        {
            var expenses = await _context.Expenses
                .Where(e =>
                    e.UserId == employeeId &&
                    e.Status == "Approved" &&
                    e.ExpenseDate.HasValue &&
                    e.ExpenseDate.Value.Month == month &&
                    e.ExpenseDate.Value.Year == year)
                .SumAsync(e => (decimal?)e.Amount);

            return expenses ?? 0;
        }

        /* ============================================================
           COMMON PAYROLL CALCULATION
        ============================================================ */

        private async Task<(decimal gross, decimal totalDeduction, decimal attendanceDeduction, decimal expenses, List<PayrollDetail> details)>
        CalculatePayroll(
            EmployeeSalary empSalary,
            List<SalaryStructureComponent> structureComponents,
            int userId,
            int month,
            int year)
        {
            decimal basic = 0;
            decimal gross = 0;
            decimal totalDeduction = 0;

            var payrollDetails = new List<PayrollDetail>();

            /* ================= BASIC ================= */

            var basicComponent = structureComponents
                .FirstOrDefault(x => x.Component.ComponentName.ToLower() == "basic");

            if (basicComponent != null)
            {
                if (basicComponent.CalculationType?.ToLower() == "fixed")
                    basic = basicComponent.Value;
                else if (basicComponent.CalculationType?.ToLower() == "percentage")
                    basic = empSalary.Ctc * basicComponent.Value / 100;

                basic = Math.Round(basic, 2);
                gross += basic;

                payrollDetails.Add(CreatePayrollDetail(basicComponent.ComponentId, basic, userId));
            }

            /* ================= EARNINGS ================= */

            var earnings = structureComponents
                .Where(x => x.Component.Type == "Earning" &&
                            x.Component.ComponentName.ToLower() != "basic");

            foreach (var item in earnings)
            {
                decimal amount = 0;

                if (item.CalculationType?.ToLower() == "fixed")
                    amount = item.Value;

                else if (item.CalculationType?.ToLower() == "percentage")
                {
                    if (item.Component.PercentageOf?.ToLower() == "basic")
                        amount = basic * item.Value / 100;
                    else
                        amount = empSalary.Ctc * item.Value / 100;
                }

                amount = Math.Round(amount, 2);
                gross += amount;

                payrollDetails.Add(CreatePayrollDetail(item.ComponentId, amount, userId));
            }

            /* ================= DEDUCTIONS ================= */

            var deductions = structureComponents
                .Where(x => x.Component.Type == "Deduction");

            foreach (var item in deductions)
            {
                decimal amount = 0;

                if (item.CalculationType?.ToLower() == "fixed")
                    amount = item.Value;

                else if (item.CalculationType?.ToLower() == "percentage")
                {
                    if (item.Component.PercentageOf?.ToLower() == "basic")
                        amount = basic * item.Value / 100;
                    else
                        amount = empSalary.Ctc * item.Value / 100;
                }

                amount = Math.Round(amount, 2);
                totalDeduction += amount;

                payrollDetails.Add(CreatePayrollDetail(item.ComponentId, amount, userId));
            }

            /* ================= ATTENDANCE ================= */

            var attendance = await GetEmployeeAttendanceSummary(
                empSalary.EmployeeId, month, year);

            int allowedLeaves = 1;
            int allowedHalfDays = 2;

            int extraLeaves = Math.Max(0, attendance.leaveDays - allowedLeaves);
            int extraHalfDays = Math.Max(0, attendance.halfDays - allowedHalfDays);

            decimal perDaySalary = attendance.workingDays == 0
                ? 0
                : gross / attendance.workingDays;

            decimal attendanceDeduction =
                (extraLeaves * perDaySalary) +
                (extraHalfDays * (perDaySalary / 2));

            attendanceDeduction = Math.Round(attendanceDeduction, 2);

            if (attendanceDeduction > 0)
            {
                totalDeduction += attendanceDeduction;

                payrollDetails.Add(new PayrollDetail
                {
                    ComponentId = 0,
                    Amount = attendanceDeduction,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            /* ================= EXPENSES ================= */

            var expenses = await GetApprovedExpenses(
                empSalary.EmployeeId, month, year);

            if (expenses > 0)
            {
                gross += expenses;

                payrollDetails.Add(new PayrollDetail
                {
                    ComponentId = 0,
                    Amount = expenses,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                });
            }

            return (
                Math.Round(gross, 2),
                Math.Round(totalDeduction, 2),
                attendanceDeduction,
                expenses,
                payrollDetails
            );
        }

        /* ============================================================
           PREVIEW PAYROLL
        ============================================================ */

        public async Task<List<PayrollTransactionDto>> PreviewPayrollAsync(ProcessPayrollRequestDto dto, int userId)
        {
            var resultList = new List<PayrollTransactionDto>();

            var activeSalaries = await _context.EmployeeSalaries
                .Where(x => x.IsActive && x.UserId == userId)
                .ToListAsync();

            foreach (var empSalary in activeSalaries)
            {
                var structureComponents = await _context.SalaryStructureComponents
                    .Include(x => x.Component)
                    .Where(x => x.StructureId == empSalary.StructureId)
                    .ToListAsync();

                var (gross, totalDeduction, attendanceDeduction, expenses, details) =
                    await CalculatePayroll(empSalary, structureComponents, userId, dto.Month, dto.Year);

                var attendance = await GetEmployeeAttendanceSummary(
                    empSalary.EmployeeId, dto.Month, dto.Year);

                var detailList = details.Select(d => new PayrollDetailDto
                {
                    ComponentId = d.ComponentId,
                    Amount = d.Amount,
                    Type = structureComponents
                        .FirstOrDefault(x => x.ComponentId == d.ComponentId)?.Component.Type ?? "",
                    ComponentName = structureComponents
                        .FirstOrDefault(x => x.ComponentId == d.ComponentId)?.Component.ComponentName ?? "Other"
                }).ToList();

                resultList.Add(new PayrollTransactionDto
                {
                    EmployeeId = empSalary.EmployeeId,
                    Month = dto.Month,
                    Year = dto.Year,
                    GrossSalary = gross,
                    TotalDeductions = totalDeduction,
                    NetSalary = gross - totalDeduction,
                    AttendanceDeduction = attendanceDeduction,
                    Expenses = expenses,
                    Status = "Preview",
                    WorkingDays = attendance.workingDays,
                    PresentDays = attendance.presentDays,
                    LeaveDays = attendance.leaveDays,
                    HalfDays = attendance.halfDays,
                    Details = detailList
                });
            }

            return resultList;
        }

        /* ============================================================
           PROCESS PAYROLL
        ============================================================ */

        public async Task<string> ProcessPayrollAsync(ProcessPayrollRequestDto dto, int userId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var activeSalaries = await _context.EmployeeSalaries
                    .Where(x => x.IsActive && x.UserId == userId)
                    .ToListAsync();

                foreach (var empSalary in activeSalaries)
                {
                    var alreadyProcessed = await _context.PayrollTransactions
                        .AnyAsync(x =>
                            x.EmployeeId == empSalary.EmployeeId &&
                            x.Month == dto.Month &&
                            x.Year == dto.Year &&
                            x.UserId == userId);

                    if (alreadyProcessed)
                        continue;

                    var structureComponents = await _context.SalaryStructureComponents
                        .Include(x => x.Component)
                        .Where(x => x.StructureId == empSalary.StructureId)
                        .ToListAsync();

                    var (gross, totalDeduction, attendanceDeduction, expenses, payrollDetails) =
                        await CalculatePayroll(empSalary, structureComponents, userId, dto.Month, dto.Year);

                    var payrollTransaction = new PayrollTransaction
                    {
                        EmployeeId = empSalary.EmployeeId,
                        Month = dto.Month,
                        Year = dto.Year,
                        GrossSalary = gross,
                        TotalDeductions = totalDeduction,
                        NetSalary = gross - totalDeduction,
                        Status = "Processed",
                        UserId = userId,
                        CompanyId = empSalary.CompanyId,
                        RegionId = empSalary.RegionId,
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.PayrollTransactions.Add(payrollTransaction);
                    await _context.SaveChangesAsync();

                    foreach (var detail in payrollDetails)
                    {
                        detail.PayrollId = payrollTransaction.PayrollId;
                        _context.PayrollDetails.Add(detail);
                    }

                    await _context.SaveChangesAsync();
                }

                await transaction.CommitAsync();
                return "Payroll Processed Successfully";
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        /* ============================================================
           GET PAYROLL BY MONTH
        ============================================================ */

        public async Task<List<PayrollTransactionDto>> GetPayrollByMonthAsync(int month, int year, int userId)
        {
            return await _context.PayrollTransactions
                .Where(x => x.Month == month && x.Year == year && x.UserId == userId)
                .Select(x => new PayrollTransactionDto
                {
                    PayrollId = x.PayrollId,
                    EmployeeId = x.EmployeeId,
                    Month = x.Month,
                    Year = x.Year,
                    GrossSalary = x.GrossSalary,
                    TotalDeductions = x.TotalDeductions,
                    NetSalary = x.NetSalary,
                    Status = x.Status
                })
                .ToListAsync();
        }

        /* ============================================================
           HELPER
        ============================================================ */

        private PayrollDetail CreatePayrollDetail(int componentId, decimal amount, int userId)
        {
            return new PayrollDetail
            {
                ComponentId = componentId,
                Amount = Math.Round(amount, 2),
                UserId = userId,
                CreatedAt = DateTime.UtcNow
            };
        }
    }
}