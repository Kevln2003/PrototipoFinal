using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototipoFinal.Models
{
    public class AuditEvent
    {
        public DateTime Timestamp { get; set; }
        public string Usuario { get; set; }
        public string Accion { get; set; }
        public string Detalles { get; set; }
    }
}
