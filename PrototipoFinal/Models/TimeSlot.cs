using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototipoFinal.Models
{
    public class TimeSlot
    {
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public bool IsAvailable { get; set; }
        public string Area { get; set; }
    }
}
