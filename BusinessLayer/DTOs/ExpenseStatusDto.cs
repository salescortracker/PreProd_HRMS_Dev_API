using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class ExpenseStatusDto
    {

        public int ExpenseStatusID { get; set; }

        public string ExpenseStatusName { get; set; }

        public bool IsActive { get; set; }

        public int CompanyID { get; set; }

        public int RegionID { get; set; }

        public int UserId { get; set; }

        public int CreatedBy { get; set; }
    }
}
