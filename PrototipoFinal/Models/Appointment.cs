using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototipoFinal.Models
{
    public class Appointment
    {
        public int Id { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public string Area { get; set; }
        public string PatientName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Reason { get; set; }
        public string Status { get; set; } = "Scheduled"; // Scheduled, Cancelled
    }
}
