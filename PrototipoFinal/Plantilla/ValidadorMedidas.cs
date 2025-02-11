using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototipoFinal.Models
{
    public class ValidadorMedidas
    {
        // Valida el peso
        public static bool ValidarPeso(double peso)
        {
            return peso >= 30 && peso <= 250; // Rango típico de peso
        }

        // Valida la altura
        public static bool ValidarAltura(double altura)
        {
            return altura >= 100 && altura <= 250; // Rango típico de altura
        }
    }
}