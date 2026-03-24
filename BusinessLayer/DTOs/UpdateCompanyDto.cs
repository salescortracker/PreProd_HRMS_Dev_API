using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class UpdateCompanyDto
    {
        public int CompanyId { get; set; }

        public string CompanyName { get; set; }

        public string CompanyEmail { get; set; }

        public int PlanId { get; set; }

        public DateTime ExpiryDate { get; set; }
    }
}
