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
using System.IO;
using PrototipoFinal.Plantilla;
using System.Collections.ObjectModel;

namespace PrototipoFinal.Pediatria
{
    public sealed partial class FormularioDeSguimientoPediatrico : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private PacientePediatrico pacienteActual;
        private readonly string historiasClinicasFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "HistoriasClinicasPediatricas");
        private ObservableCollection<Vacuna> listaVacunas;

        public FormularioDeSguimientoPediatrico()
        {
            this.InitializeComponent();
            Directory.CreateDirectory(historiasClinicasFolder);
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
                MostrarError("No se encontró información del paciente.");
            }
        }

        private void InitializeVacunas()
        {
            // Initialize as ObservableCollection
            var vacunasList = pacienteActual?.Vacunas ?? new List<Vacuna>
        {
            new Vacuna { Nombre = "BCG", EdadRecomendada = "Al nacer" },
            new Vacuna { Nombre = "Hepatitis B", EdadRecomendada = "Al nacer" },
            new Vacuna { Nombre = "Pentavalente", EdadRecomendada = "2, 4, 6 meses" },
            new Vacuna { Nombre = "Polio", EdadRecomendada = "2, 4, 6 meses" },
            new Vacuna { Nombre = "Rotavirus", EdadRecomendada = "2, 4 meses" },
            new Vacuna { Nombre = "Neumococo", EdadRecomendada = "2, 4, 12 meses" },
            new Vacuna { Nombre = "Influenza", EdadRecomendada = "6 meses, anual" },
            new Vacuna { Nombre = "MMR", EdadRecomendada = "12 meses" },
            new Vacuna { Nombre = "Varicela", EdadRecomendada = "12 meses" }
        };

            // Convert to ObservableCollection and assign to class field
            listaVacunas = new ObservableCollection<Vacuna>(vacunasList);

            // Use the correct name of your ListView control as defined in XAML
            VacunasList.ItemsSource = listaVacunas;
        }

        private async void GuardarDatos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtCorreoRepresentante.Text) &&
                    string.IsNullOrWhiteSpace(txtCelularRepresentante.Text))
                {
                    await MostrarError("Debe proporcionar al menos un método de contacto del representante.");
                    return;
                }

                // Create new consultation
                var nuevaConsulta = new ConsultaPediatrica
                {
                    FechaConsulta = DateTime.Now,
                    Temperatura = decimal.Parse(txtTemperatura.Text),
                    FrecuenciaCardiaca = int.Parse(txtFrecuenciaCardiaca.Text),
                    FrecuenciaRespiratoria = int.Parse(txtFrecuenciaRespiratoria.Text),
                    SaturacionOxigeno = int.Parse(txtSaturacion.Text),
                    Peso = decimal.Parse(txtPeso.Text),
                    Talla = decimal.Parse(txtTalla.Text),
                    PerimetroCefalico = decimal.Parse(txtPerimetroCefalico.Text),
                    IMC = decimal.Parse(txtIMC.Text),
                    PielFaneras = txtPiel.Text,
                    CabezaCuello = txtCabeza.Text,
                    ToraxCardiopulmonar = txtTorax.Text,
                    Abdomen = txtAbdomen.Text,
                    Extremidades = txtExtremidades.Text,
                    ExamenNeurologico = txtNeurologico.Text,
                    MotivoConsulta = txtMotivo.Text,
                    Observaciones = txtObservaciones.Text
                };

                // Update patient data
                pacienteActual.CorreoRepresentante = txtCorreoRepresentante.Text?.Trim() ?? "";
                pacienteActual.CelularRepresentante = txtCelularRepresentante.Text?.Trim() ?? "";
               // pacienteActual.Vacunas = listaVacunas;
                pacienteActual.Consultas.Add(nuevaConsulta);

                // Save to LocalSettings
                var registros = ObtenerRegistros();
                var registroExistente = registros.FirstOrDefault(r => r.Id == pacienteActual.Id);

                if (registroExistente != null)
                {
                    registroExistente.CorreoRepresentante = pacienteActual.CorreoRepresentante;
                    registroExistente.CelularRepresentante = pacienteActual.CelularRepresentante;
                    registroExistente.Vacunas = pacienteActual.Vacunas;
                    registroExistente.Consultas.Add(nuevaConsulta); // Update consultations
                }

                string jsonDatos = JsonConvert.SerializeObject(registros, Formatting.Indented);
                localSettings.Values["RegistrosPediatricos"] = jsonDatos;

                // Generate/Update clinical history TXT file
                await GenerarHistoriaClinicaTXT(pacienteActual);
                await MostrarArchivoTXT(pacienteActual.Id);
                Frame.GoBack();
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al guardar los datos: {ex.Message}");
            }
        }

        private async Task GenerarHistoriaClinicaTXT(PacientePediatrico paciente)
        {
            string nombreArchivo = Path.Combine(historiasClinicasFolder, $"HC_PED_{paciente.Id}.txt");
            bool archivoExiste = File.Exists(nombreArchivo);

            using (StreamWriter writer = new StreamWriter(nombreArchivo, true)) // true for append
            {
                if (!archivoExiste)
                {
                    // Initial header if file doesn't exist
                    writer.WriteLine("HISTORIA CLÍNICA PEDIÁTRICA");
                    writer.WriteLine("===========================");
                    writer.WriteLine();

                    // Patient information (written only once)
                    writer.WriteLine("DATOS DEL PACIENTE");
                    writer.WriteLine("----------------");
                    writer.WriteLine($"Nombre: {paciente.NombrePaciente}");
                    writer.WriteLine($"Fecha de Nacimiento: {paciente.FechaNacimiento:dd/MM/yyyy}");
                    writer.WriteLine();
                }

                // Add new follow-up entry
                writer.WriteLine("ACTUALIZACIÓN DE SEGUIMIENTO");
                writer.WriteLine("---------------------------");
                writer.WriteLine($"Fecha: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine();

                var ultimaConsulta = paciente.Consultas.LastOrDefault();
                if (ultimaConsulta != null)
                {
                    writer.WriteLine("Signos Vitales:");
                    writer.WriteLine($"- Temperatura: {ultimaConsulta.Temperatura}°C");
                    writer.WriteLine($"- Frecuencia Cardíaca: {ultimaConsulta.FrecuenciaCardiaca} lpm");
                    writer.WriteLine($"- Frecuencia Respiratoria: {ultimaConsulta.FrecuenciaRespiratoria} rpm");
                    writer.WriteLine($"- Saturación O2: {ultimaConsulta.SaturacionOxigeno}%");
                    writer.WriteLine();

                    writer.WriteLine("Medidas Antropométricas:");
                    writer.WriteLine($"- Peso: {ultimaConsulta.Peso} kg");
                    writer.WriteLine($"- Talla: {ultimaConsulta.Talla} cm");
                    writer.WriteLine($"- Perímetro Cefálico: {ultimaConsulta.PerimetroCefalico} cm");
                    writer.WriteLine($"- IMC: {ultimaConsulta.IMC}");
                    writer.WriteLine();

                    writer.WriteLine("Examen Físico:");
                    writer.WriteLine($"- Piel y Faneras: {ultimaConsulta.PielFaneras}");
                    writer.WriteLine($"- Cabeza y Cuello: {ultimaConsulta.CabezaCuello}");
                    writer.WriteLine($"- Tórax y Cardiopulmonar: {ultimaConsulta.ToraxCardiopulmonar}");
                    writer.WriteLine($"- Abdomen: {ultimaConsulta.Abdomen}");
                    writer.WriteLine($"- Extremidades: {ultimaConsulta.Extremidades}");
                    writer.WriteLine($"- Examen Neurológico: {ultimaConsulta.ExamenNeurologico}");
                    writer.WriteLine();

                    writer.WriteLine("Motivo de Consulta:");
                    writer.WriteLine(ultimaConsulta.MotivoConsulta);
                    writer.WriteLine();

                    writer.WriteLine("Observaciones:");
                    writer.WriteLine(ultimaConsulta.Observaciones);
                    writer.WriteLine();
                }

                writer.WriteLine("Estado de Vacunación:");
                foreach (var vacuna in paciente.Vacunas)
                {
                    writer.WriteLine($"- {vacuna.Nombre} ({vacuna.EdadRecomendada}): " +
                        $"{(vacuna.Aplicada ? $"Aplicada el {vacuna.FechaAplicacion:dd/MM/yyyy}" : "Pendiente")}");
                }
                writer.WriteLine();

                writer.WriteLine("------------------------------------------------");
                writer.WriteLine();
            }
        }
        private void CalcularEdad(DateTime fechaNacimiento)
        {
            var edad = DateTime.Today - fechaNacimiento;
            var años = edad.Days / 365; // Calculo de años
            var meses = (edad.Days % 365) / 30; // Calculo de meses restantes

            // Asignar la edad al TextBox
            txtEdad.Text = $"{años} años" + (meses > 0 ? $" y {meses} meses" : "");
        }

private void CargarDatosPaciente(PacientePediatrico paciente)
{
    txtNombrePaciente.Text = paciente.NombrePaciente;
    txtNombreRepresentante.Text = paciente.NombreRepresentante;
    txtCorreoRepresentante.Text = paciente.CorreoRepresentante;
    txtCelularRepresentante.Text = paciente.CelularRepresentante;
    //dateFechaNacimiento.Date = paciente.FechaNacimiento;

    // Calcular y mostrar la edad
    CalcularEdad(paciente.FechaNacimiento);

    // Load last consultation data if exists
    var ultimaConsulta = paciente.Consultas.LastOrDefault();
    if (ultimaConsulta != null)
    {
        txtPeso.Text = ultimaConsulta.Peso.ToString();
        txtTalla.Text = ultimaConsulta.Talla.ToString();
        txtPerimetroCefalico.Text = ultimaConsulta.PerimetroCefalico.ToString();
        txtIMC.Text = ultimaConsulta.IMC.ToString();
    }

    // Update vaccines list
    //listaVacunas = paciente.Vacunas;
    VacunasList.ItemsSource = listaVacunas;
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

        private async Task MostrarArchivoTXT(string id)
        {
            try
            {
                string rutaArchivo = Path.Combine(historiasClinicasFolder, $"HC_PED_{id}.txt");

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

        private void GenerarReceta_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RecetaMedica));
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}