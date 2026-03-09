using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.DTOs
{
    public class CalendarEventDto
    {
        public DateOnly? date { get; set; }
        public string type { get; set; }
        public string title { get; set; }
    }
}
