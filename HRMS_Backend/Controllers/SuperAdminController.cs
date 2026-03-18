using BusinessLayer.DTOs;
using DataAccessLayer.DBContext;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HRMS_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuperAdminController : ControllerBase
    {
        private readonly HRMSContext _context;

        public SuperAdminController(HRMSContext context)
        {
            _context = context;
        }

        private bool IsSuperAdmin()
        {
            var designationId = User.FindFirst("RoleId")?.Value;
            return designationId == "0";
        }

        // 🔥 GET ALL COMPANIES
        [HttpGet("companies")]
        public async Task<IActionResult> GetCompanies()
        {
            //if (!IsSuperAdmin())
            //    return Forbid();

            var companies = await _context.Companies
                .Include(c => c.Plan)
                .ToListAsync();

            return Ok(companies);
        }
        [HttpGet("GetAllCompanies")]
        public async Task<IActionResult> GetAllCompanies()
        {
            //if (!IsSuperAdmin())
            //    return Forbid();
                    var companies = await _context.Companies
                .Include(c => c.Plan)
                .Select(c => new
                {
                    c.CompanyId,
                    c.CompanyName,
                    c.CompanyEmail,
                    c.PlanId,
                    c.ExpiryDate,
                    c.IsActive
                })
                .ToListAsync();

            return Ok(companies);
        }

        [HttpGet("company/{id}")]
        public async Task<IActionResult> GetCompany(int id)
        {
            //if (!IsSuperAdmin())
                //return Forbid();
            var company = await _context.Companies
                .Include(c => c.Plan)
                .FirstOrDefaultAsync(x => x.CompanyId == id);

            return Ok(company);
        }

        [HttpPut("update-company")]
        public async Task<IActionResult> UpdateCompany(UpdateCompanyDto dto)
        {
            //if (!IsSuperAdmin())
            //    return Forbid();
            var company = await _context.Companies.FindAsync(dto.CompanyId);

            company.CompanyName = dto.CompanyName;
            company.CompanyEmail = dto.CompanyEmail;
            company.PlanId = dto.PlanId;
            company.ExpiryDate = dto.ExpiryDate;

            await _context.SaveChangesAsync();

            return Ok();
        }

        //[HttpPost("create-company")]
        //public async Task<IActionResult> CreateCompany(CreateCompanyRequest request)
        //{
        //    if (!IsSuperAdmin())
        //        return Forbid();

        //    // 1️⃣ Validate Plan
        //    var plan = await _context.Plans.FindAsync(request.PlanId);
        //    if (plan == null)
        //        return BadRequest("Invalid Plan");

        //    // 2️⃣ Create Company
        //    var company = new Company
        //    {
        //        CompanyName = request.CompanyName,
        //        PlanId = request.PlanId,
        //        PlanStartDate = DateTime.UtcNow,
        //        ExpiryDate = DateTime.UtcNow.AddDays(plan.DurationInDays),
        //        IsActive = true
        //    };

        //    _context.Companies.Add(company);
        //    await _context.SaveChangesAsync();

        //    // 3️⃣ Assign Modules From Plan
        //    var planModules = await _context.PlanModules
        //        .Where(pm => pm.PlanId == request.PlanId)
        //        .ToListAsync();

        //    foreach (var pm in planModules)
        //    {
        //        _context.CompanyModules.Add(new CompanyModule
        //        {
        //            CompanyId = company.CompanyId,
        //            AppModuleId = pm.AppModuleId,   // ✅ aligned with your model
        //            IsEnabled = true,
        //            IsActive = true
        //        });
        //    }

        //    await _context.SaveChangesAsync();

        //    // 4️⃣ Create Admin Designation
        //    var designation = new Designation
        //    {
        //        Name = "Admin",
        //        CompanyId = company.CompanyId
        //    };

        //    _context.Designations.Add(designation);
        //    await _context.SaveChangesAsync();

        //    // 5️⃣ Create Default Admin User
        //    var adminUsername = request.CompanyName.Replace(" ", "").ToLower() + "_admin";

        //    var adminUser = new User
        //    {
        //        Username = adminUsername,
        //        PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
        //        CompanyId = company.CompanyId,
        //        DesignationId = designation.Id,
        //        IsActive = true
        //    };

        //    _context.Users.Add(adminUser);
        //    await _context.SaveChangesAsync();

        //    // 6️⃣ Give Full Permissions To Admin
        //    var companyModules = await _context.CompanyModules
        //        .Where(cm => cm.CompanyId == company.CompanyId)
        //        .ToListAsync();

        //    foreach (var cm in companyModules)
        //    {
        //        _context.DesignationModulePermissions.Add(new DesignationModulePermission
        //        {
        //            DesignationId = designation.Id,
        //            AppModuleId = cm.AppModuleId,   
        //            CanView = true,
        //            CanCreate = true,
        //            CanEdit = true,
        //            CanDelete = true
        //        });
        //    }

        //    await _context.SaveChangesAsync();


        //    return Ok(new
        //    {
        //        Message = "Company created successfully",
        //        Company = company.CompanyName,
        //        AdminUsername = adminUsername,
        //        DefaultPassword = "Admin@123"
        //    });
        //}

        [HttpPost("create-company")]
        public async Task<IActionResult> CreateCompany([FromBody] CreateCompanyRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // 1️⃣ Check duplicate email
                var emailExists = await _context.Users
                    .AnyAsync(u => u.Email == request.CompanyEmail);

                if (emailExists)
                    return BadRequest(new { message = "Email already exists" });

                // 2️⃣ Validate Plan
                var plan = await _context.Plans
                    .FirstOrDefaultAsync(x => x.PlanId == request.PlanId);

                if (plan == null)
                    return BadRequest(new { message = "Invalid plan selected" });

                // 3️⃣ Create Company
                var company = new Company
                {
                    CompanyName = request.CompanyName,
                    CompanyEmail = request.CompanyEmail,
                    PlanId = request.PlanId,
                    PlanStartDate = DateTime.UtcNow,
                    ExpiryDate = request.ExpiryDate,
                    IsActive = true,
                    CompanyCode = GenerateCompanyCode()
                };

                _context.Companies.Add(company);
                await _context.SaveChangesAsync();

                // 4️⃣ Create Default Region
                var region = new Region
                {
                    RegionName = "Head Office",
                    CompanyId = company.CompanyId,
                    IsActive = true,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Regions.Add(region);
                await _context.SaveChangesAsync();

                // 5️⃣ Copy Plan Modules → Company Modules
                var planModules = await _context.PlanModules
                    .Where(x => x.PlanId == request.PlanId)
                    .ToListAsync();

                var companyModules = planModules.Select(pm => new CompanyModule
                {
                    CompanyId = company.CompanyId,
                    AppModuleId = pm.AppModuleId,
                    IsEnabled = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                });

                await _context.CompanyModules.AddRangeAsync(companyModules);

                // 6️⃣ Generate Password
                var password = GeneratePassword();

                // 7️⃣ Create Admin User
                var adminUser = new User
                {
                    Email = request.CompanyEmail,
                    //PasswordHash = BCrypt.Net.BCrypt.HashPassword(password),
                    PasswordHash = password,
                    CompanyId = company.CompanyId,
                    RegionId = region.RegionId,   // ✅ FIX
                    RoleId = 1,
                    Status = "Active",
                    FullName = company.CompanyName,
                    CreatedDate = DateTime.UtcNow
                };

                _context.Users.Add(adminUser);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return Ok(new
                {
                    message = "Company created successfully",
                    company = company.CompanyName,
                    adminEmail = request.CompanyEmail,
                    password = password
                });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                return StatusCode(500, new
                {
                    message = "Company creation failed",
                    error = ex.Message
                });
            }
        }

        private string GenerateCompanyCode()
        {
            return "CMP-" + Guid.NewGuid().ToString().Substring(0, 8).ToUpper();
        }
        private string GeneratePassword()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789@#";
            var random = new Random();

            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        // 🔥 ACTIVATE / DEACTIVATE COMPANY
        [HttpPut("toggle-company/{companyId}")]
        public async Task<IActionResult> ToggleCompany(int companyId)
        {
            //if (!IsSuperAdmin())
            //    return Forbid();

            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
                return NotFound();

            company.IsActive = !company.IsActive;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                company.CompanyName,
                company.IsActive
            });
        }
        [HttpGet("GetUserMenus/{companyId}")]
        public async Task<IActionResult> GetUserMenus(int companyId)
        {
            // Get company plan
            var planId = await _context.Companies
                .Where(x => x.CompanyId == companyId)
                .Select(x => x.PlanId)
                .FirstOrDefaultAsync();

            if (planId == 0)
                return Ok(new List<object>());

            // Get allowed menu ids
            var allowedMenus = await _context.PlanModules
                .Where(x => x.PlanId == planId)
                .Select(x => x.AppModuleId)
                .ToListAsync();

            // Get menu details
            var menus = await _context.MenuMasters
                .Where(m => m.IsActive == true && allowedMenus.Contains(m.MenuId))
                .OrderBy(m => m.OrderNo)
                .Select(m => new
                {
                    menuId = m.MenuId,
                    menuName = m.MenuName,
                    parentMenuId = m.ParentMenuId,
                    url = m.Url,
                    icon = m.Icon
                })
                .ToListAsync();

            return Ok(menus);
        }



    }

    public class CreateCompanyRequest
    {
        public string CompanyName { get; set; }

        public string CompanyEmail { get; set; }  // ✅ NEW

        public int PlanId { get; set; }

        public int RegionId { get; set; }
        public DateTime ExpiryDate { get; set; }  // ✅ NEW
    }
}

