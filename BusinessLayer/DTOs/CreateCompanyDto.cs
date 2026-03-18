using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class CreateCompanyDto
    {
        public string CompanyName { get; set; }
        public int PlanId { get; set; }
        public int RegionId { get; set; }
        public string CompanyEmail { get; set; }
        public DateTime ExpiryDate { get; set; }

    }
}
