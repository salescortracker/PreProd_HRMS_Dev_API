using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class ModeOfStudyDto
    {
        public int ModeOfStudyId { get; set; }
        public string ModeName { get; set; } = string.Empty;

        public int CompanyId { get; set; }

        public int RegionId { get; set; }

        public int UserId { get; set; }

        public bool IsActive { get; set; }

        public bool IsDeleted { get; set; }

        public int CreatedBy { get; set; }

        public DateTime CreatedAt { get; set; }

        public int? ModifiedBy { get; set; }

        public DateTime? ModifiedAt { get; set; }
        public string? CompanyName { get; set; }
        public string? RegionName { get; set; }
    }
}
