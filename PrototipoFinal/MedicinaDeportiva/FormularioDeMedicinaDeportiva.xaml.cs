using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Windows.Storage;
using PrototipoFinal.Plantilla;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PrototipoFinal.MedicinaDeportiva
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FormularioDeMedicinaDeportiva : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public FormularioDeMedicinaDeportiva()
        {
            this.InitializeComponent();
            CalcularIMCAutomaticamente();
        }
        private async void GuardarDatos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validar campos requeridos
                if (string.IsNullOrWhiteSpace(txtNombres.Text) ||
                    string.IsNullOrWhiteSpace(txtApellidos.Text) ||
                    string.IsNullOrWhiteSpace(txtCedula.Text))
                {
                    await MostrarError("Los campos Nombres, Apellidos y Cédula son obligatorios.");
                    return;
                }

                // Validar formato de números
                double peso = 0;
                double altura = 0;
                double imc = 0;

                if (!string.IsNullOrWhiteSpace(txtPeso.Text))
                {
                    if (!double.TryParse(txtPeso.Text.Replace(",", "."), out peso))
                    {
                        await MostrarError("El formato del peso no es válido. Use solo números (ejemplo: 70.5)");
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtAltura.Text))
                {
                    if (!double.TryParse(txtAltura.Text.Replace(",", "."), out altura))
                    {
                        await MostrarError("El formato de la altura no es válido. Use solo números (ejemplo: 170.5)");
                        return;
                    }
                }

                if (!string.IsNullOrWhiteSpace(txtIMC.Text))
                {
                    if (!double.TryParse(txtIMC.Text.Replace(",", "."), out imc))
                    {
                        await MostrarError("Error al calcular el IMC");
                        return;
                    }
                }

                // Crear objeto con los datos validados
                var datos = new DatosMedicoDeportivos
                {
                    Id = Guid.NewGuid().ToString(),
                    Nombres = txtNombres.Text.Trim(),
                    Apellidos = txtApellidos.Text.Trim(),
                    Correo = txtCorreo.Text?.Trim() ?? "",
                    Celular = txtCelular.Text?.Trim() ?? "",
                    Cedula = txtCedula.Text.Trim(),
                    AntecedentesFamiliares = txtAntecedentesFamiliares.Text?.Trim() ?? "",
                    HaPracticadoDeportes = false, // Obtener del CheckBox
                    DeportesPracticados = txtDeportesPracticados.Text?.Trim() ?? "",
                    Peso = peso,
                    Altura = altura,
                    IMC = imc,
                    FechaRegistro = DateTime.Now
                };

                // Obtener registros existentes
                var registrosExistentes = ObtenerRegistros();
                registrosExistentes.Add(datos);

                // Guardar en almacenamiento local
                string jsonDatos = JsonConvert.SerializeObject(registrosExistentes, Formatting.Indented);
                localSettings.Values["RegistrosMedicos"] = jsonDatos;

                await MostrarMensaje("Éxito", "Los datos han sido guardados correctamente.");
                LimpiarFormulario();
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al guardar los datos: {ex.Message}");
            }
        }

        // Métodos auxiliares para mostrar mensajes
        private async Task MostrarError(string mensaje)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = mensaje,
                CloseButtonText = "Ok"
            };
            await dialog.ShowAsync();
        }

        private async Task MostrarMensaje(string titulo, string mensaje)
        {
            var dialog = new ContentDialog
            {
                Title = titulo,
                Content = mensaje,
                CloseButtonText = "Ok"
            };
            await dialog.ShowAsync();
        }

        // Modelo de datos para el formulario
        public class DatosMedicoDeportivos
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("nombres")]
            public string Nombres { get; set; }

            [JsonProperty("apellidos")]
            public string Apellidos { get; set; }

            [JsonProperty("correo")]
            public string Correo { get; set; }

            [JsonProperty("celular")]
            public string Celular { get; set; }

            [JsonProperty("cedula")]
            public string Cedula { get; set; }

            [JsonProperty("antecedentesFamiliares")]
            public string AntecedentesFamiliares { get; set; }

            [JsonProperty("haPracticadoDeportes")]
            public bool HaPracticadoDeportes { get; set; }

            [JsonProperty("deportesPracticados")]
            public string DeportesPracticados { get; set; }

            [JsonProperty("peso")]
            public double Peso { get; set; }

            [JsonProperty("altura")]
            public double Altura { get; set; }

            [JsonProperty("imc")]
            public double IMC { get; set; }

            [JsonProperty("fechaRegistro")]
            public DateTime FechaRegistro { get; set; }
        }



        private void CalcularIMCAutomaticamente()
        {
            txtPeso.TextChanged += (s, e) => CalcularIMC();
            txtAltura.TextChanged += (s, e) => CalcularIMC();
        }

        private void CalcularIMC()
        {
            try
            {
                if (double.TryParse(txtPeso.Text?.Replace(",", "."), out double peso) &&
                    double.TryParse(txtAltura.Text?.Replace(",", "."), out double altura))
                {
                    altura = altura / 100; // Convertir cm a metros
                    double imc = peso / (altura * altura);
                    txtIMC.Text = imc.ToString("F2").Replace(",", ".");
                }
                else
                {
                    txtIMC.Text = "";
                }
            }
            catch
            {
                txtIMC.Text = "";
            }
        }


        private List<DatosMedicoDeportivos> ObtenerRegistros()
        {
            if (localSettings.Values.TryGetValue("RegistrosMedicos", out object value))
            {
                string json = value.ToString();
                return JsonConvert.DeserializeObject<List<DatosMedicoDeportivos>>(json) ??
                       new List<DatosMedicoDeportivos>();
            }
            return new List<DatosMedicoDeportivos>();
        }


        private void LimpiarFormulario()
        {
            txtNombres.Text = string.Empty;
            txtApellidos.Text = string.Empty;
            txtCorreo.Text = string.Empty;
            txtCelular.Text = string.Empty;
            txtCedula.Text = string.Empty;
            txtAntecedentesFamiliares.Text = string.Empty;
            txtDeportesPracticados.Text = string.Empty;
            txtPeso.Text = string.Empty;
            txtAltura.Text = string.Empty;
            txtIMC.Text = string.Empty;
        }

        public static async Task<List<DatosMedicoDeportivos>> BuscarPorCedula(string cedula)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("RegistrosMedicos", out object value))
            {
                string json = value.ToString();
                var todos = JsonConvert.DeserializeObject<List<DatosMedicoDeportivos>>(json);
                return todos?.Where(x => x.Cedula.Contains(cedula)).ToList() ??
                       new List<DatosMedicoDeportivos>();
            }
            return new List<DatosMedicoDeportivos>();
        }

        public static async Task<List<DatosMedicoDeportivos>> BuscarPorNombre(string nombre)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("RegistrosMedicos", out object value))
            {
                string json = value.ToString();
                var todos = JsonConvert.DeserializeObject<List<DatosMedicoDeportivos>>(json);
                return todos?.Where(x =>
                    x.Nombres.ToLower().Contains(nombre.ToLower()) ||
                    x.Apellidos.ToLower().Contains(nombre.ToLower())
                ).ToList() ?? new List<DatosMedicoDeportivos>();
            }
            return new List<DatosMedicoDeportivos>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Buscar));
        }
    }
}
