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
using System.IO;

namespace PrototipoFinal.MedicinaDeportiva
{
    public sealed partial class FormularioDeSeguimientoDePacienteDepotivo : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private PacienteDeportivo pacienteActual;
        private readonly string historiasClinicasFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "HistoriasClinicas");

        public FormularioDeSeguimientoDePacienteDepotivo()
        {
            this.InitializeComponent();
            Directory.CreateDirectory(historiasClinicasFolder);

        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            // Check for both DatosMedicoDeportivos and PacienteDeportivo
            if (e.Parameter is PacienteDeportivo datosMedicos)
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
        private async Task GenerarHistoriaClinicaTXT(PacienteDeportivo paciente)
        {
            string nombreArchivo = Path.Combine(historiasClinicasFolder, $"HC_{paciente.Cedula}.txt");
            bool archivoExiste = File.Exists(nombreArchivo);

            using (StreamWriter writer = new StreamWriter(nombreArchivo, true)) // true para append
            {
                if (!archivoExiste)
                {
                    // Si el archivo no existe, escribir el encabezado inicial
                    writer.WriteLine("HISTORIA CLÍNICA DEPORTIVA");
                    writer.WriteLine("==========================");
                    writer.WriteLine();

                    // Datos personales (solo se escriben la primera vez)
                    writer.WriteLine("DATOS PERSONALES");
                    writer.WriteLine("----------------");
                    writer.WriteLine($"Nombres: {paciente.Nombres}");
                    writer.WriteLine($"Apellidos: {paciente.Apellidos}");
                    writer.WriteLine($"Cédula: {paciente.Cedula}");
                    writer.WriteLine();
                }

                // Agregar nueva entrada de seguimiento
                writer.WriteLine("ACTUALIZACIÓN DE SEGUIMIENTO");
                writer.WriteLine("---------------------------");
                writer.WriteLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine();

                // Datos de contacto actualizados
                writer.WriteLine("Datos de Contacto:");
                writer.WriteLine($"Correo: {paciente.Correo}");
                writer.WriteLine($"Celular: {paciente.Celular}");
                writer.WriteLine();

                // Datos médicos actualizados
                writer.WriteLine("Datos Médicos:");
                writer.WriteLine($"Peso: {paciente.Peso:F2} kg");
                writer.WriteLine($"Altura: {paciente.Altura:F2} m");
                writer.WriteLine($"IMC: {paciente.IMC:F2}");
                writer.WriteLine($"Clasificación IMC: {ObtenerClasificacionIMC(paciente.IMC)}");
                writer.WriteLine();

                // Línea separadora para futuras entradas
                writer.WriteLine("------------------------------------------------");
                writer.WriteLine();
            }
        }

        private string ObtenerClasificacionIMC(double imc)
        {
            if (imc < 18.5) return "Bajo peso";
            if (imc < 25) return "Peso normal";
            if (imc < 30) return "Sobrepeso";
            if (imc < 35) return "Obesidad grado I";
            if (imc < 40) return "Obesidad grado II";
            return "Obesidad grado III";
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

                // Actualizar datos del paciente
                pacienteActual.Correo = txtCorreo.Text?.Trim() ?? "";
                pacienteActual.Celular = txtCelular.Text?.Trim() ?? "";

                // Guardar en LocalSettings
                var registros = ObtenerRegistros();
                var registroExistente = registros.FirstOrDefault(r => r.Cedula == pacienteActual.Cedula);

                if (registroExistente != null)
                {
                    registroExistente.Correo = pacienteActual.Correo;
                    registroExistente.Celular = pacienteActual.Celular;
                }

                string jsonDatos = JsonConvert.SerializeObject(registros, Formatting.Indented);
                localSettings.Values["RegistrosMedicos"] = jsonDatos;

                // Generar/Actualizar archivo TXT de historia clínica
                await GenerarHistoriaClinicaTXT(pacienteActual);

                await MostrarArchivoTXT(pacienteActual.Cedula);
                Frame.GoBack();
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al guardar los datos: {ex.Message}");
            }
        }

        private async Task MostrarArchivoTXT(string cedula)
        {
            try
            {
                string rutaArchivo = Path.Combine(historiasClinicasFolder, $"HC_{cedula}.txt");

                if (File.Exists(rutaArchivo))
                {
                    string contenido = await File.ReadAllTextAsync(rutaArchivo);

                    var dialog = new ContentDialog
                    {
                        Title = "Historia Clínica Generada",
                        Content = new ScrollViewer
                        {
                            Content = new TextBlock
                            {
                                Text = contenido,
                                TextWrapping = TextWrapping.Wrap,
                                FontSize = 14
                            },
                            Height = 400
                        },
                        CloseButtonText = "Cerrar"
                    };

                    await dialog.ShowAsync();
                }
                else
                {
                    await MostrarError("No se encontró la historia clínica generada.");
                }
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al abrir el archivo: {ex.Message}");
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