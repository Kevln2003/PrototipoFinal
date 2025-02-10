using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using Newtonsoft.Json;
using PrototipoFinal.Models;
using PrototipoFinal.Models.PrototipoFinal.Models;
using PrototipoFinal.Plantilla;

namespace PrototipoFinal.Pediatria
{
    public sealed partial class FormularioDeSguimientoPediatrico : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private PacientePediatrico pacienteActual;
        private readonly string historiasClinicasFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "HistoriasPediatricas");

        public FormularioDeSguimientoPediatrico()
        {
            this.InitializeComponent();
            Directory.CreateDirectory(historiasClinicasFolder);
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RecetaMedica));
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (e.Parameter is PacientePediatrico paciente)
            {
                pacienteActual = paciente;
                CargarDatosPaciente(paciente);
            }
            else
            {
                MostrarError("No se encontró información del paciente pediátrico.");
            }
        }

        private void CargarDatosPaciente(PacientePediatrico paciente)
        {
            // Datos del paciente
            txtNombrePaciente.Text = paciente.NombrePaciente;
            txtCedulaPaciente.Text = paciente.CedulaPaciente;
            if (paciente.FechaNacimiento != DateTime.MinValue)
            {
               // txtFechaNacimiento.Text = paciente.FechaNacimiento.ToString("yyyy-MM-dd");
            }

            // Datos del representante
            txtNombreRepresentante.Text = paciente.NombreRepresentante;
            txtCedulaRepresentante.Text = paciente.CedulaRepresentante;
            txtCorreoRepresentante.Text = paciente.CorreoRepresentante;
            txtCelularRepresentante.Text = paciente.CelularRepresentante;

            // Signos vitales
            txtTemperatura.Text = paciente.Temperatura.ToString("F1");
            txtFrecuenciaCardiaca.Text = paciente.FrecuenciaCardiaca.ToString();
            txtFrecuenciaRespiratoria.Text = paciente.FrecuenciaRespiratoria.ToString();
            txtSaturacion.Text = paciente.SaturacionOxigeno.ToString();

            // Medidas físicas
            txtPeso.Text = paciente.Peso.ToString("F2");
            txtTalla.Text = paciente.Talla.ToString("F2");
            txtPerimetroCefalico.Text = paciente.PerimetroCefalico.ToString("F1");

            // Examen físico y observaciones
            txtAbdomen.Text = paciente.Abdomen;
            txtExtremidades.Text = paciente.Extremidades;
            txtMotivo.Text = paciente.MotivoConsulta;
            txtObservaciones.Text = paciente.Observaciones;
        }

        private async Task GenerarHistoriaClinicaTXT(PacientePediatrico paciente)
        {
            string nombreArchivo = Path.Combine(historiasClinicasFolder, $"HC_Pediatrica_{paciente.CedulaPaciente}.txt");
            bool archivoExiste = File.Exists(nombreArchivo);

            using (StreamWriter writer = new StreamWriter(nombreArchivo, true))
            {
                if (!archivoExiste)
                {
                    writer.WriteLine("HISTORIA CLÍNICA PEDIÁTRICA");
                    writer.WriteLine("===========================");
                    writer.WriteLine();

                    writer.WriteLine("DATOS DEL PACIENTE");
                    writer.WriteLine("------------------");
                    writer.WriteLine($"Nombre del Paciente: {paciente.NombrePaciente}");
                    writer.WriteLine($"Cédula del Paciente: {paciente.CedulaPaciente}");
                    writer.WriteLine($"Fecha de Nacimiento: {paciente.FechaNacimiento:dd/MM/yyyy}");
                    writer.WriteLine();

                    writer.WriteLine("DATOS DEL REPRESENTANTE");
                    writer.WriteLine("----------------------");
                    writer.WriteLine($"Nombre: {paciente.NombreRepresentante}");
                    writer.WriteLine($"Cédula: {paciente.CedulaRepresentante}");
                    writer.WriteLine();
                }

                writer.WriteLine("REGISTRO DE CONSULTA");
                writer.WriteLine("-------------------");
                writer.WriteLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine();

                writer.WriteLine("Signos Vitales:");
                writer.WriteLine($"Temperatura: {paciente.Temperatura}°C");
                writer.WriteLine($"Frecuencia Cardíaca: {paciente.FrecuenciaCardiaca} lpm");
                writer.WriteLine($"Frecuencia Respiratoria: {paciente.FrecuenciaRespiratoria} rpm");
                writer.WriteLine($"Saturación O2: {paciente.SaturacionOxigeno}%");
                writer.WriteLine();

                writer.WriteLine("Medidas Antropométricas:");
                writer.WriteLine($"Peso: {paciente.Peso} kg");
                writer.WriteLine($"Talla: {paciente.Talla} cm");
                writer.WriteLine($"Perímetro Cefálico: {paciente.PerimetroCefalico} cm");
                writer.WriteLine($"IMC: {CalcularIMC(paciente.Peso, paciente.Talla):F2}");
                writer.WriteLine();

                writer.WriteLine("Examen Físico:");
                writer.WriteLine($"Abdomen: {paciente.Abdomen}");
                writer.WriteLine($"Extremidades: {paciente.Extremidades}");
                writer.WriteLine();

                writer.WriteLine("Observaciones:");
                writer.WriteLine(paciente.Observaciones);
                writer.WriteLine();

                writer.WriteLine("------------------------------------------------");
                writer.WriteLine();
            }
        }

        private double CalcularIMC(decimal peso, decimal talla)
        {
            if (talla == 0) return 0;
            double pesoDouble = (double)peso;
            double tallaMetros = (double)talla / 100;
            return pesoDouble / (tallaMetros * tallaMetros);
        }

        private async void GuardarDatos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ValidarDatos())
                {
                    ActualizarDatosPaciente();
                    await GuardarEnLocalSettings();
                    await GenerarHistoriaClinicaTXT(pacienteActual);
                    await MostrarArchivoTXT(pacienteActual.CedulaPaciente);
                    Frame.GoBack();
                }
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al guardar los datos: {ex.Message}");
            }
        }

        private bool ValidarDatos()
        {
            if (string.IsNullOrWhiteSpace(txtCorreoRepresentante.Text) &&
                string.IsNullOrWhiteSpace(txtCelularRepresentante.Text))
            {
                MostrarError("Debe proporcionar al menos un método de contacto del representante.");
                return false;
            }
            return true;
        }

        private void ActualizarDatosPaciente()
        {
            // Actualizar los datos del paciente con los valores de los TextBox
            pacienteActual.CorreoRepresentante = txtCorreoRepresentante.Text?.Trim();
            pacienteActual.CelularRepresentante = txtCelularRepresentante.Text?.Trim();
            // Actualizar otros campos según sea necesario
        }

        private async Task GuardarEnLocalSettings()
        {
            var registros = ObtenerRegistros();
            var registroExistente = registros.FirstOrDefault(r => r.CedulaPaciente == pacienteActual.CedulaPaciente);

            if (registroExistente != null)
            {
                // Actualizar registro existente
                registroExistente = pacienteActual;
            }
            else
            {
                // Agregar nuevo registro
                registros.Add(pacienteActual);
            }

            string jsonDatos = JsonConvert.SerializeObject(registros, Formatting.Indented);
            localSettings.Values["RegistrosPediatricos"] = jsonDatos;
        }

        private async Task MostrarArchivoTXT(string cedula)
        {
            try
            {
                string rutaArchivo = Path.Combine(historiasClinicasFolder, $"HC_Pediatrica_{cedula}.txt");
                if (File.Exists(rutaArchivo))
                {
                    string contenido = await File.ReadAllTextAsync(rutaArchivo);
                    var dialog = new ContentDialog
                    {
                        Title = "Historia Clínica Pediátrica",
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
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al mostrar el archivo: {ex.Message}");
            }
        }

        private List<PacientePediatrico> ObtenerRegistros()
        {
            if (localSettings.Values.TryGetValue("RegistrosPediatricos", out object value))
            {
                string json = value.ToString();
                return JsonConvert.DeserializeObject<List<PacientePediatrico>>(json) ??
                       new List<PacientePediatrico>();
            }
            return new List<PacientePediatrico>();
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

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }



        private void Recetar_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RecetaMedica), pacienteActual);
        }
    }
}