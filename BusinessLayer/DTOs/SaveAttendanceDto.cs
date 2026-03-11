using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class SaveAttendanceDto
    {
        public int RegionId { get; set; }
        public int CompanyId { get; set; }

        public DateTime AttendanceDate { get; set; }

        public List<EmployeeAttendanceItemDto> Employees { get; set; }
    }

    public class EmployeeAttendanceItemDto
    {
        public string EmployeeCode { get; set; }
        public string EmployeeName { get; set; }

        public string Status { get; set; }

        // 👇 MAKE THESE NULLABLE
        public string? ClockIn { get; set; }

        public string? ClockOut { get; set; }

        public string? GrossTime { get; set; }
    }
}
