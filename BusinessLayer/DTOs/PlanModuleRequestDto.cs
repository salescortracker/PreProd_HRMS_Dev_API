using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class PlanModuleRequestDto
    {
        public int PlanId { get; set; }

        public List<int> ModuleIds { get; set; }
    }
}
