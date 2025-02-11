using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using PrototipoFinal.Models.PrototipoFinal.Models;
using PrototipoFinal.Models;

namespace PrototipoFinal.Pediatria
{
    public sealed partial class FormularioDeMedicinaPediatrica : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private readonly string historiasPediatricasFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "HistoriasPediatricas1");
        private List<PacientePediatrico.Vacuna> listaVacunas;

        public FormularioDeMedicinaPediatrica()
        {
            InitializeComponent();
            Directory.CreateDirectory(historiasPediatricasFolder);
            InitializeVacunas();
        }

        private void InitializeVacunas()
        {
            listaVacunas = new List<PacientePediatrico.Vacuna>
            {
                new PacientePediatrico.Vacuna { Nombre = "BCG", EdadRecomendada = "Al nacer" },
                new PacientePediatrico.Vacuna { Nombre = "Hepatitis B", EdadRecomendada = "Al nacer" },
                new PacientePediatrico.Vacuna { Nombre = "Pentavalente", EdadRecomendada = "2, 4, 6 meses" }
            };
            VacunasList.ItemsSource = listaVacunas;
        }

        private async void GuardarDatos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validación básica de campos obligatorios del representante
                if (string.IsNullOrWhiteSpace(txtCorreoRepresentante.Text) &&
                    string.IsNullOrWhiteSpace(txtCelularRepresentante.Text))
                {
                    await MostrarError("Debe proporcionar al menos un método de contacto del representante.");
                    return;
                }

                // Crear nuevo paciente pediátrico con los datos del formulario
                var nuevoPaciente = new PacientePediatrico
                {
                    // Datos del paciente
                    NombrePaciente = txtNombrePaciente.Text?.Trim() ?? "",
                    CedulaPaciente = txtCedulaPaciente.Text?.Trim() ?? "",
//                    FechaNacimiento = DateTime.TryParse(txtFechaNacimiento.Text, out DateTime fechaNac) ? fechaNac : DateTime.MinValue,

                    // Datos del representante
                    NombreRepresentante = txtNombreRepresentante.Text?.Trim() ?? "",
                    CedulaRepresentante = txtCedulaRepresentante.Text?.Trim() ?? "",
                    CorreoRepresentante = txtCorreoRepresentante.Text?.Trim() ?? "",
                    CelularRepresentante = txtCelularRepresentante.Text?.Trim() ?? "",

                    // Información de consulta
                    FechaConsulta = DateTime.Now,

                    // Signos vitales y medidas
    

                    // Medidas físicas
                    Peso = decimal.TryParse(txtPeso.Text?.Replace(",", "."), out decimal peso) ? peso : 0,
                    Talla = decimal.TryParse(txtTalla.Text?.Replace(",", "."), out decimal talla) ? talla : 0,
                    PerimetroCefalico = decimal.TryParse(txtPerimetroCefalico.Text?.Replace(",", "."), out decimal perimetroCefalico) ? perimetroCefalico : 0,
                   // IMC = decimal.TryParse(txt.Text?.Replace(",", "."), out decimal imc) ? imc : 0,

                    // Examen físico
                   // PielFaneras = txtPielFaneras.Text?.Trim() ?? "",
                    //CabezaCuello = txtCabezaCuello.Text?.Trim() ?? "",
                    //oraxCardiopulmonar = txtToraxCardiopulmonar.Text?.Trim() ?? "",
                    Abdomen = txtAbdomen.Text?.Trim() ?? "",
                    Extremidades = txtExtremidades.Text?.Trim() ?? "",
                    //ExamenNeurologico = txtExamenNeurologico.Text?.Trim() ?? "",

                    // Información general
                    MotivoConsulta = txtMotivo.Text?.Trim() ?? "",
                    Observaciones = txtObservaciones.Text?.Trim() ?? "",

                    // Información de vacunas
                   // NombreVacuna = txtn.Text?.Trim() ?? "",
                   // EdadRecomendada = txtEdadRecomendada.Text?.Trim() ?? "",
                    //FechaAplicacion = DateTime.TryParse(txtFechaAplicacion.Text, out DateTime fechaAplicacion) ? fechaAplicacion : (DateTime?)null,
                    //Aplicada = chkAplicada.IsChecked ?? false
                };

                // Obtener registros existentes
                var registros = ObtenerRegistros();
                var registroExistente = registros.FirstOrDefault(r => r.CedulaPaciente == nuevoPaciente.CedulaPaciente);

                if (registroExistente != null)
                {
                    registroExistente.NombrePaciente = nuevoPaciente.NombrePaciente;
                    registroExistente.NombreRepresentante = nuevoPaciente.NombreRepresentante;
                    registroExistente.CorreoRepresentante = nuevoPaciente.CorreoRepresentante;
                    registroExistente.CedulaRepresentante = nuevoPaciente.CorreoRepresentante;
                    registroExistente.Peso = nuevoPaciente.Peso;

                }
                else
                {

                    // Agregar nuevo registro
                    registros.Add(nuevoPaciente);
                }
                  
              

                // Guardar en LocalSettings
                string jsonDatos = JsonConvert.SerializeObject(registros, Formatting.Indented);
                localSettings.Values["HistoriasPediatricas1"] = jsonDatos;

                // Generar/Actualizar archivo TXT de historia clínica
                await GenerarHistoriaClinicaTXT(nuevoPaciente);

                await MostrarArchivoTXT(nuevoPaciente.CedulaPaciente);
                LimpiarFormulario();
                Frame.GoBack();
                await AuditLogger.LogEvent(
                "Medicina Pediatrica ",
                "El doctor ingreso un nuevo paciente",
                nuevoPaciente.NombrePaciente

);
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al guardar los datos: {ex.Message}");
            }
        }
        private List<PacientePediatrico> ObtenerRegistros()
        {
            try
            {
                var jsonDatos = localSettings.Values["HistoriasPediatricas1"] as string;
                return string.IsNullOrEmpty(jsonDatos)
                    ? new List<PacientePediatrico>()
                    : JsonConvert.DeserializeObject<List<PacientePediatrico>>(jsonDatos);
            }
            catch
            {
                return new List<PacientePediatrico>();
            }
        }
        private void LimpiarFormulario()
        {
            // Datos del paciente
            txtNombrePaciente.Text = string.Empty;
            txtCedulaPaciente.Text = string.Empty;
          //  txtFechaNacimiento.Text = string.Empty;

            // Datos del representante
            txtNombreRepresentante.Text = string.Empty;
            txtCedulaRepresentante.Text = string.Empty;
            txtCorreoRepresentante.Text = string.Empty;
            txtCelularRepresentante.Text = string.Empty;

            // Signos vitales
            txtTemperatura.Text = string.Empty;
            txtFrecuenciaCardiaca.Text = string.Empty;
            txtFrecuenciaRespiratoria.Text = string.Empty;
            //txtSaturacionOxigeno.Text = string.Empty;

            // Medidas físicas
            txtPeso.Text = string.Empty;
            txtTalla.Text = string.Empty;
            txtPerimetroCefalico.Text = string.Empty;
            //txtIMC.Text = string.Empty;

            // Examen físico
            //txtPielFaneras.Text = string.Empty;
            //txtCabezaCuello.Text = string.Empty;
            //txtToraxCardiopulmonar.Text = string.Empty;
            txtAbdomen.Text = string.Empty;
            txtExtremidades.Text = string.Empty;
            //txtExamenNeurologico.Text = string.Empty;

            // Información general
            //txtMotivoConsulta.Text = string.Empty;
            txtObservaciones.Text = string.Empty;

            // Vacunas
            //txtNombreVacuna.Text = string.Empty;
            //txtEdadRecomendada.Text = string.Empty;
            //txtFechaAplicacion.Text = string.Empty;
            //chkAplicada.IsChecked = false;
        }
        private void ActualizarRegistros(List<PacientePediatrico> registros, PacientePediatrico nuevoPaciente)
        {
            var registroExistente = registros.FirstOrDefault(r => r.CedulaPaciente == nuevoPaciente.CedulaPaciente);
            if (registroExistente != null)
            {
                registros.Remove(registroExistente);
            }
            registros.Add(nuevoPaciente);

            string jsonDatos = JsonConvert.SerializeObject(registros, Formatting.Indented);
            localSettings.Values["HistoriasPediatricas1"] = jsonDatos;
        }

        private int CalcularEdad(DateTime fechaNacimiento)
        {
            var today = DateTime.Today;
            var age = today.Year - fechaNacimiento.Year;
            if (fechaNacimiento.Date > today.AddYears(-age)) age--;
            return age;
        }
        private async Task GenerarHistoriaClinicaTXT(PacientePediatrico paciente)
        {
            string nombreArchivo = Path.Combine(historiasPediatricasFolder, $"HC_Pediatrica_{paciente.CedulaPaciente}.txt");

            using (StreamWriter writer = new StreamWriter(nombreArchivo, false))
            {
                writer.WriteLine("HISTORIA CLÍNICA PEDIÁTRICA");
                writer.WriteLine("==========================");
                writer.WriteLine($"Fecha de Consulta: {paciente.FechaConsulta:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine();

                // Datos del Paciente
                writer.WriteLine("DATOS DEL PACIENTE");
                writer.WriteLine("-----------------");
                writer.WriteLine($"Nombre: {paciente.NombrePaciente}");
                writer.WriteLine($"Cédula: {paciente.CedulaPaciente}");
                writer.WriteLine($"Fecha de Nacimiento: {paciente.FechaNacimiento:dd/MM/yyyy}");
                writer.WriteLine($"Edad: {CalcularEdad(paciente.FechaNacimiento)} años");
                writer.WriteLine();

                // Datos del Representante
                writer.WriteLine("DATOS DEL REPRESENTANTE");
                writer.WriteLine("----------------------");
                writer.WriteLine($"Nombre: {paciente.NombreRepresentante}");
                writer.WriteLine($"Cédula: {paciente.CedulaRepresentante}");
                writer.WriteLine($"Correo: {paciente.CorreoRepresentante}");
                writer.WriteLine($"Celular: {paciente.CelularRepresentante}");
                writer.WriteLine();

                // Signos Vitales
                writer.WriteLine("SIGNOS VITALES");
                writer.WriteLine("--------------");
                writer.WriteLine($"Temperatura: {paciente.Temperatura:F1}°C");
                writer.WriteLine($"Frecuencia Cardíaca: {paciente.FrecuenciaCardiaca} bpm");
                writer.WriteLine($"Frecuencia Respiratoria: {paciente.FrecuenciaRespiratoria} rpm");
                writer.WriteLine($"Saturación de Oxígeno: {paciente.SaturacionOxigeno}%");
                writer.WriteLine();

                // Medidas Físicas
                writer.WriteLine("MEDIDAS FÍSICAS");
                writer.WriteLine("---------------");
                writer.WriteLine($"Peso: {paciente.Peso:F2} kg");
                writer.WriteLine($"Talla: {paciente.Talla:F2} cm");
                writer.WriteLine($"Perímetro Cefálico: {paciente.PerimetroCefalico:F2} cm");
                writer.WriteLine($"IMC: {paciente.IMC:F2}");
                writer.WriteLine();

                // Examen Físico
                writer.WriteLine("EXAMEN FÍSICO");
                writer.WriteLine("-------------");
                writer.WriteLine($"Piel y Faneras: {paciente.PielFaneras}");
                writer.WriteLine($"Cabeza y Cuello: {paciente.CabezaCuello}");
                writer.WriteLine($"Tórax Cardiopulmonar: {paciente.ToraxCardiopulmonar}");
                writer.WriteLine($"Abdomen: {paciente.Abdomen}");
                writer.WriteLine($"Extremidades: {paciente.Extremidades}");
                writer.WriteLine($"Examen Neurológico: {paciente.ExamenNeurologico}");
                writer.WriteLine();

                // Vacunas
                writer.WriteLine("INFORMACIÓN DE VACUNAS");
                writer.WriteLine("---------------------");
                writer.WriteLine($"Vacuna: {paciente.NombreVacuna}");
                writer.WriteLine($"Edad Recomendada: {paciente.EdadRecomendada}");
                writer.WriteLine($"Fecha de Aplicación: {(paciente.FechaAplicacion.HasValue ? paciente.FechaAplicacion.Value.ToString("dd/MM/yyyy") : "No aplicada")}");
                writer.WriteLine($"Estado: {(paciente.Aplicada ? "Aplicada" : "Pendiente")}");
                writer.WriteLine();

                // Información General
                writer.WriteLine("INFORMACIÓN GENERAL");
                writer.WriteLine("-------------------");
                writer.WriteLine($"Motivo de Consulta: {paciente.MotivoConsulta}");
                writer.WriteLine($"Observaciones: {paciente.Observaciones}");
            }
        }
        private List<PacientePediatrico> ObtenerRegistrosPediatricos()
        {
            if (localSettings.Values.TryGetValue("HistoriasPediatricas1", out object value))
            {
                string json = value.ToString();
                return JsonConvert.DeserializeObject<List<PacientePediatrico>>(json) ??
                       new List<PacientePediatrico>();
            }
            return new List<PacientePediatrico>();
        }



        private async Task MostrarArchivoTXT(string id)
        {
            try
            {
                string rutaArchivo = Path.Combine(historiasPediatricasFolder, $"HC_Pediatrica_{id}.txt");

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

        private async Task MostrarError(string mensaje)
        {
            await new ContentDialog { Title = "Error", Content = mensaje, CloseButtonText = "OK" }.ShowAsync();
        }
        public static async Task<List<PacientePediatrico>> BuscarPorCedula1(string cedula)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("HistoriasPediatricas1", out object value))
            {
                string json = value.ToString();
                var todos = JsonConvert.DeserializeObject<List<PacientePediatrico>>(json);
                return todos?.Where(x => x.CedulaPaciente.Contains(cedula)).ToList() ??
                       new List<PacientePediatrico>();
            }
            return new List<PacientePediatrico>();
        }

        public static async Task<List<PacientePediatrico>> BuscarPorNombre1(string nombre)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("HistoriasPediatricas1", out object value))
            {
                string json = value.ToString();
                var todos = JsonConvert.DeserializeObject<List<PacientePediatrico>>(json);
                return todos?.Where(x =>
                    x.NombrePaciente.ToLower().Contains(nombre.ToLower()) 
                ).ToList() ?? new List<PacientePediatrico>();
            }
            return new List<PacientePediatrico>();
        }
        private void Cancelar_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void GenerarReceta_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Plantilla.RecetaMedica));
        }
    }
}