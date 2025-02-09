using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using PrototipoFinal.Models;
using System.IO;
using PrototipoFinal.Plantilla;
using PrototipoFinal.Models.PrototipoFinal.Models;

namespace PrototipoFinal.Pediatria
{
    public sealed partial class FormularioDeMedicinaPediatrica : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private readonly string historiasClinicasFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "HistoriasClinicasPediatricas");
        private List<Vacuna> listaVacunas;

        public FormularioDeMedicinaPediatrica()
        {
            this.InitializeComponent();
            Directory.CreateDirectory(historiasClinicasFolder);
            InitializeVacunas();
        }

        private void InitializeVacunas()
        {
            listaVacunas = new List<Vacuna>
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
            VacunasList.ItemsSource = listaVacunas;
        }

        private async void GuardarDatos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validación básica de contacto
                if (string.IsNullOrWhiteSpace(txtCorreoRepresentante.Text) &&
                    string.IsNullOrWhiteSpace(txtCelularRepresentante.Text))
                {
                    await MostrarError("Debe proporcionar al menos un método de contacto del representante.");
                    return;
                }

                if (!decimal.TryParse(txtPerimetroCefalico.Text.Replace(",", "."),
                    System.Globalization.NumberStyles.Any,
                    System.Globalization.CultureInfo.InvariantCulture,
                    out decimal perimetroCefalico))
                {
                    await MostrarError("El formato del perímetro cefálico no es válido.");
                    return;
                }




                // Crear o actualizar paciente
                var nuevoPaciente = new PacientePediatrico
                {
                    NombrePaciente = txtNombrePaciente.Text?.Trim() ?? "",
                    CedulaDelPaciente = txtCedulaPaciente.Text?.Trim() ?? "",
                    FechaNacimiento = dateFechaNacimiento.Date.DateTime,
                    NombreRepresentante = txtNombreRepresentante.Text?.Trim() ?? "",
                    CedulaDelRepresentante = txtCedulaRepresentante.Text?.Trim() ?? "",
                    CorreoRepresentante = txtCorreoRepresentante.Text?.Trim() ?? "",
                    CelularRepresentante = txtCelularRepresentante.Text?.Trim() ?? "",
                    Vacunas = listaVacunas,
                    FechaRegistro = DateTime.Now
                };

                // Obtener registros existentes
                var registros = ObtenerRegistros();
                var registroExistente = registros.FirstOrDefault(r =>
                    r.NombrePaciente == nuevoPaciente.NombrePaciente &&
                    r.FechaNacimiento == nuevoPaciente.FechaNacimiento);

                if (registroExistente != null)
                {
                    // Actualizar registro existente
                    registroExistente.NombreRepresentante = nuevoPaciente.NombreRepresentante;
                    registroExistente.CorreoRepresentante = nuevoPaciente.CorreoRepresentante;
                    registroExistente.CelularRepresentante = nuevoPaciente.CelularRepresentante;
                    registroExistente.Vacunas = nuevoPaciente.Vacunas;
                }
                else
                {
                    registros.Add(nuevoPaciente);
                }

                // Guardar en LocalSettings
                string jsonDatos = JsonConvert.SerializeObject(registros, Formatting.Indented);
                localSettings.Values["RegistrosPediatricos"] = jsonDatos;

                // Generar historia clínica
               // await GuardarPacienteEnArchivo(nuevoPaciente);
                await GenerarHistoriaClinicaTXT(nuevoPaciente);
                await MostrarArchivoTXT(nuevoPaciente.Id);
                LimpiarFormulario();
                Frame.GoBack();
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al guardar los datos: {ex.Message}");
            }
        }
        private async Task GuardarPacienteEnArchivo(PacienteDeportivo paciente)
        {
            string nombreArchivo = Path.Combine(historiasClinicasFolder, $"PAC_{paciente.Cedula}.json");
            string jsonDatos = JsonConvert.SerializeObject(paciente, Formatting.Indented);
            await File.WriteAllTextAsync(nombreArchivo, jsonDatos);
        }

        private async Task GenerarHistoriaClinicaTXT(PacientePediatrico paciente)
        {
            string nombreArchivo = Path.Combine(historiasClinicasFolder, $"HC_PED_{paciente.NombrePaciente.Replace(" ", "_")}_{paciente.FechaNacimiento:yyyyMMdd}.txt");

            using (StreamWriter writer = new StreamWriter(nombreArchivo, false))
            {
                writer.WriteLine("HISTORIA CLÍNICA PEDIÁTRICA");
                writer.WriteLine("===========================");
                writer.WriteLine($"Fecha de Actualización: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine();

                // Datos del paciente
                writer.WriteLine("DATOS DEL PACIENTE");
                writer.WriteLine("-----------------");
                writer.WriteLine($"Nombre: {paciente.NombrePaciente}");
                writer.WriteLine($"Fecha de Nacimiento: {paciente.FechaNacimiento:dd/MM/yyyy}");
                writer.WriteLine($"Edad: {CalcularEdad(paciente.FechaNacimiento)}");
                writer.WriteLine();

                // Datos del representante
                writer.WriteLine("DATOS DEL REPRESENTANTE");
                writer.WriteLine("----------------------");
                writer.WriteLine($"Nombre: {paciente.NombreRepresentante}");
                writer.WriteLine($"Correo: {paciente.CorreoRepresentante}");
                writer.WriteLine($"Celular: {paciente.CelularRepresentante}");
                writer.WriteLine();

                // Control de vacunas
                writer.WriteLine("CONTROL DE VACUNAS");
                writer.WriteLine("-----------------");
                foreach (var vacuna in paciente.Vacunas)
                {
                    writer.WriteLine($"{vacuna.Nombre} ({vacuna.EdadRecomendada}): " +
                        $"{(vacuna.Aplicada ? $"Aplicada el {vacuna.FechaAplicacion:dd/MM/yyyy}" : "Pendiente")}");
                }
                writer.WriteLine();

                // Última consulta
                var ultimaConsulta = paciente.Consultas.LastOrDefault();
                if (ultimaConsulta != null)
                {
                    writer.WriteLine("ÚLTIMA CONSULTA");
                    writer.WriteLine("--------------");
                    writer.WriteLine($"Fecha: {ultimaConsulta.FechaConsulta:dd/MM/yyyy}");
                    writer.WriteLine($"Motivo: {ultimaConsulta.MotivoConsulta}");
                    writer.WriteLine();

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

                    writer.WriteLine("Observaciones:");
                    writer.WriteLine(ultimaConsulta.Observaciones);
                }
            }
        }
        private string CalcularEdad(DateTime fechaNacimiento)
        {
            var edad = DateTime.Now - fechaNacimiento;
            var años = Math.Floor(edad.TotalDays / 365.25);
            var meses = Math.Floor((edad.TotalDays % 365.25) / 30.44);

            if (años >= 2)
                return $"{años} años";
            else if (años == 1)
                return $"1 año y {meses} meses";
            else
                return $"{meses} meses";
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

        private void LimpiarFormulario()
        {
            txtNombrePaciente.Text = string.Empty;
            dateFechaNacimiento.Date = DateTime.Now;
            txtNombreRepresentante.Text = string.Empty;
            txtCorreoRepresentante.Text = string.Empty;
            txtCelularRepresentante.Text = string.Empty;
            txtTemperatura.Text = string.Empty;
            txtFrecuenciaCardiaca.Text = string.Empty;
            txtFrecuenciaRespiratoria.Text = string.Empty;
            txtSaturacion.Text = string.Empty;
            txtPeso.Text = string.Empty;
            txtTalla.Text = string.Empty;
            txtPerimetroCefalico.Text = string.Empty;

            txtPiel.Text = string.Empty;
            txtCabeza.Text = string.Empty;
            txtTorax.Text = string.Empty;
            txtAbdomen.Text = string.Empty;
            txtExtremidades.Text = string.Empty;
            txtNeurologico.Text = string.Empty;
            txtMotivo.Text = string.Empty;
            txtObservaciones.Text = string.Empty;
            InitializeVacunas();
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
                        Title = "Historia Clínica Pediátrica Generada",
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