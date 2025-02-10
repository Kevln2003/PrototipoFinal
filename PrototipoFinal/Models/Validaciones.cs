using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace PrototipoFinal.Models
{
    internal class Validaciones
    {
        // Validación de cédula ecuatoriana
        public static bool ValidarCedula(string cedula)
        {
            try
            {
                if (string.IsNullOrEmpty(cedula) || cedula.Length != 10 || !cedula.All(char.IsDigit))
                    return false;

                int[] coeficientes = { 2, 1, 2, 1, 2, 1, 2, 1, 2 };
                int suma = 0;
                int verificador = int.Parse(cedula[9].ToString());

                for (int i = 0; i < 9; i++)
                {
                    int valor = int.Parse(cedula[i].ToString()) * coeficientes[i];
                    suma += valor > 9 ? valor - 9 : valor;
                }

                int digitoVerificador = 10 - (suma % 10);
                if (digitoVerificador == 10) digitoVerificador = 0;

                return digitoVerificador == verificador;
            }
            catch
            {
                return false;
            }
        }

        // Validación de celular ecuatoriano
        public static bool ValidarCelular(string celular)
        {
            try
            {
                if (string.IsNullOrEmpty(celular))
                    return false;

                // Eliminar espacios y guiones si existen
                celular = celular.Replace(" ", "").Replace("-", "");

                // Verificar longitud y que comience con 09
                return celular.Length == 10 &&
                       celular.StartsWith("09") &&
                       celular.All(char.IsDigit);
            }
            catch
            {
                return false;
            }
        }

        // Validación de peso en un rango específico
        public static bool ValidarPeso(string peso, double minimo = 0, double maximo = 500)
        {
            try
            {
                if (string.IsNullOrEmpty(peso))
                    return false;

                if (double.TryParse(peso, out double pesoNumerico))
                {
                    return pesoNumerico >= minimo && pesoNumerico <= maximo;
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        // Método para validación en tiempo real de TextBox
        public static void ValidarTextBoxNumerico(TextBox textBox)
        {
            textBox.TextChanged += (sender, e) =>
            {
                string texto = textBox.Text;
                if (!texto.All(char.IsDigit))
                {
                    int pos = textBox.SelectionStart - 1;
                    textBox.Text = new string(texto.Where(char.IsDigit).ToArray());
                    textBox.SelectionStart = pos;
                }
            };
        }
    }
}

