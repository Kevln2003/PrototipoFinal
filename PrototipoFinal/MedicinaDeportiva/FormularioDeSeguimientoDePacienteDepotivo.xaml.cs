using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using PrototipoFinal.Models;
using PrototipoFinal.Models.PrototipoFinal.Models;
using static PrototipoFinal.MedicinaDeportiva.FormularioDeMedicinaDeportiva;
using PrototipoFinal.Plantilla;

namespace PrototipoFinal.MedicinaDeportiva
{
    public sealed partial class FormularioDeSeguimientoDePacienteDepotivo : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private PacienteDeportivo pacienteActual;

        public FormularioDeSeguimientoDePacienteDepotivo()
        {
            this.InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Check for both DatosMedicoDeportivos and PacienteDeportivo
            if (e.Parameter is DatosMedicoDeportivos datosMedicos)
            {
                // Convert DatosMedicoDeportivos to PacienteDeportivo
                pacienteActual = new PacienteDeportivo
                {
                    Id = datosMedicos.Id,
                    Nombres = datosMedicos.Nombres,
                    Apellidos = datosMedicos.Apellidos,
                    Correo = datosMedicos.Correo,
                    Celular = datosMedicos.Celular,
                    Cedula = datosMedicos.Cedula,
                    Peso = datosMedicos.Peso,
                    Altura = datosMedicos.Altura,
                    IMC = datosMedicos.IMC,
                    FechaRegistro = datosMedicos.FechaRegistro
                };

                CargarDatosPaciente(pacienteActual);
            }
            else if (e.Parameter is PacienteDeportivo paciente)
            {
                pacienteActual = paciente;
                CargarDatosPaciente(paciente);
            }
            else
            {
                MostrarError("No se encontró información del paciente.");
            }
        }

        private void CargarDatosPaciente(PacienteDeportivo paciente)
        {
            txtNombres.Text = paciente.Nombres;
            txtApellidos.Text = paciente.Apellidos;
            txtCedula.Text = paciente.Cedula;
            txtPeso.Text = paciente.Peso.ToString("F2");
            txtAltura.Text = paciente.Altura.ToString("F2");
            txtIMC.Text = paciente.IMC.ToString("F2");

            txtCorreo.Text = paciente.Correo;
            txtCelular.Text = paciente.Celular;
        }

        private async void GuardarDatos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCorreo.Text) &&
                    string.IsNullOrWhiteSpace(txtCelular.Text))
                {
                    await MostrarError("Debe proporcionar al menos un método de contacto.");
                    return;
                }

                pacienteActual.Correo = txtCorreo.Text?.Trim() ?? "";
                pacienteActual.Celular = txtCelular.Text?.Trim() ?? "";

                var registros = ObtenerRegistros();
                var registroExistente = registros.FirstOrDefault(r => r.Cedula == pacienteActual.Cedula);

                if (registroExistente != null)
                {
                    registroExistente.Correo = pacienteActual.Correo;
                    registroExistente.Celular = pacienteActual.Celular;
                }

                string jsonDatos = JsonConvert.SerializeObject(registros, Formatting.Indented);
                localSettings.Values["RegistrosMedicos"] = jsonDatos;

                await MostrarMensaje("Éxito", "Datos de contacto actualizados correctamente.");
                Frame.GoBack();
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al guardar los datos: {ex.Message}");
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private List<PacienteDeportivo> ObtenerRegistros()
        {
            if (localSettings.Values.TryGetValue("RegistrosMedicos", out object value))
            {
                string json = value.ToString();
                return JsonConvert.DeserializeObject<List<PacienteDeportivo>>(json) ??
                       new List<PacienteDeportivo>();
            }
            return new List<PacienteDeportivo>();
        }

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

        private void Recetar_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RecetaMedica));
        }
    }
}