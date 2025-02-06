using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrototipoFinal.Models
{
    internal class CedulaValidator
    {
        public static bool ValidarCedula(string cedula)
        {
            // Verificar si la cédula tiene 10 dígitos

            // Verificar que la cédula tenga 10 dígitos
            if (cedula.Length != 10 || !long.TryParse(cedula, out _))
            {
                return false;
            }

            // Verificar que los dos primeros dígitos correspondan a una provincia válida
            int provincia = int.Parse(cedula.Substring(0, 2));
            if (provincia < 1 || (provincia > 24 && provincia != 30))
            {
                return false;
            }

            // Coeficientes de validación
            int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };

            // Dígito verificador (último dígito de la cédula)
            int digitoVerificador = int.Parse(cedula[9].ToString());

            // Calcular la suma ponderada
            int suma = 0;
            for (int i = 0; i < 9; i++)
            {
                int digito = int.Parse(cedula[i].ToString());
                int producto = digito * coeficientes[i];

                // Si el producto es mayor o igual a 10, sumar los dígitos
                if (producto >= 10)
                {
                    producto = producto - 9;
                }

                suma += producto;
            }

            // Calcular el dígito verificador esperado
            int residuo = suma % 10;
            int digitoEsperado = (residuo == 0) ? 0 : 10 - residuo;

            // Comparar con el dígito verificador proporcionado
            return digitoVerificador == digitoEsperado;
        }
    }
}
 
