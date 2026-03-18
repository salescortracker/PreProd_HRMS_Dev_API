using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class CreatePlanDto
    {
        public string PlanName { get; set; }
        public int DurationInMonths { get; set; }
        public List<int> ModuleIds { get; set; }
    }
}
