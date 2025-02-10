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
using System.Collections.ObjectModel;

namespace PrototipoFinal.Pediatria
{
    public sealed partial class FormularioDeSguimientoPediatrico : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private PacientePediatrico pacienteActual;
        private readonly string historiasClinicasFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "HistoriasClinicasPediatricas");
        private ObservableCollection<PacientePediatrico.Vacuna> listaVacunas;

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
                InitializeVacunas();
            }
            else
            {
                _ = MostrarError("No se encontró información del paciente.");
                Frame.GoBack();
            }
        }

        private void InitializeVacunas()
        {
            listaVacunas = new ObservableCollection<PacientePediatrico.Vacuna>(
                pacienteActual.Vacunas ?? new List<PacientePediatrico.Vacuna>
                {
                    new PacientePediatrico.Vacuna { Nombre = "BCG", EdadRecomendada = "Al nacer" },
                    new PacientePediatrico.Vacuna { Nombre = "Hepatitis B", EdadRecomendada = "Al nacer" },
                    new PacientePediatrico.Vacuna { Nombre = "Pentavalente", EdadRecomendada = "2, 4, 6 meses" },
                    new PacientePediatrico.Vacuna { Nombre = "Polio", EdadRecomendada = "2, 4, 6 meses" },
                    new PacientePediatrico.Vacuna { Nombre = "Rotavirus", EdadRecomendada = "2, 4 meses" },
                    new PacientePediatrico.Vacuna { Nombre = "Neumococo", EdadRecomendada = "2, 4, 12 meses" },
                    new PacientePediatrico.Vacuna { Nombre = "Influenza", EdadRecomendada = "6 meses, anual" },
                    new PacientePediatrico.Vacuna { Nombre = "MMR", EdadRecomendada = "12 meses" },
                    new PacientePediatrico.Vacuna { Nombre = "Varicela", EdadRecomendada = "12 meses" }
                });

            VacunasList.ItemsSource = listaVacunas;
        }

        private async void GuardarDatos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidarCamposObligatorios())
                    return;

                var nuevaConsulta = CrearConsultaDesdeFormulario();

                ActualizarDatosPaciente(nuevaConsulta);

                await GuardarDatosCompletos();
                await GenerarHistoriaClinicaTXT();
                await MostrarArchivoTXT();

                Frame.GoBack();
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al guardar: {ex.Message}");
            }
        }

        private bool ValidarCamposObligatorios()
        {
            if (string.IsNullOrWhiteSpace(txtCorreoRepresentante.Text) &&
                string.IsNullOrWhiteSpace(txtCelularRepresentante.Text))
            {
                _ = MostrarError("Debe proporcionar al menos un método de contacto del representante.");
                return false;
            }
            return true;
        }

        private PacientePediatrico.Consulta CrearConsultaDesdeFormulario()
        {
            return new PacientePediatrico.Consulta
            {
                FechaConsulta = DateTime.Now,
                Temperatura = decimal.Parse(txtTemperatura.Text),
                FrecuenciaCardiaca = int.Parse(txtFrecuenciaCardiaca.Text),
                FrecuenciaRespiratoria = int.Parse(txtFrecuenciaRespiratoria.Text),
                SaturacionOxigeno = int.Parse(txtSaturacion.Text),
                Peso = decimal.Parse(txtPeso.Text),
                Talla = decimal.Parse(txtTalla.Text),
                PerimetroCefalico = decimal.Parse(txtPerimetroCefalico.Text),
                IMC = CalcularIMC(),
                PielFaneras = txtPiel.Text,
                CabezaCuello = txtCabeza.Text,
                ToraxCardiopulmonar = txtTorax.Text,
                Abdomen = txtAbdomen.Text,
                Extremidades = txtExtremidades.Text,
                ExamenNeurologico = txtNeurologico.Text,
                MotivoConsulta = txtMotivo.Text,
                Observaciones = txtObservaciones.Text
            };
        }

        private decimal CalcularIMC()
        {
            var peso = decimal.Parse(txtPeso.Text);
            var talla = decimal.Parse(txtTalla.Text) / 100;
            return peso / (talla * talla);
        }

        private void ActualizarDatosPaciente(PacientePediatrico.Consulta consulta)
        {
            pacienteActual.CorreoRepresentante = txtCorreoRepresentante.Text.Trim();
            pacienteActual.CelularRepresentante = txtCelularRepresentante.Text.Trim();
            pacienteActual.Vacunas = listaVacunas.ToList();
            pacienteActual.Consultas.Add(consulta);
        }

        private async Task GuardarDatosCompletos()
        {
            var registros = ObtenerRegistros();
            var registroExistente = registros.FirstOrDefault(r => r.Id == pacienteActual.Id);

            if (registroExistente != null)
            {
                registros.Remove(registroExistente);
            }
            registros.Add(pacienteActual);

            string jsonDatos = JsonConvert.SerializeObject(registros, Formatting.Indented);
            localSettings.Values["RegistrosPediatricos"] = jsonDatos;
            await Task.CompletedTask;
        }

        private async Task GenerarHistoriaClinicaTXT()
        {
            string nombreArchivo = Path.Combine(historiasClinicasFolder, $"HC_PED_{pacienteActual.Id}.txt");

            using (StreamWriter writer = new StreamWriter(nombreArchivo, false)) // Sobreescribir completo
            {
                writer.WriteLine("HISTORIA CLÍNICA PEDIÁTRICA ACTUALIZADA");
                writer.WriteLine("========================================");
                writer.WriteLine($"Fecha de actualización: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine();

                // Datos básicos actualizados
                writer.WriteLine("[DATOS ACTUALIZADOS DEL PACIENTE]");
                writer.WriteLine($"Nombre: {pacienteActual.NombrePaciente}");
                writer.WriteLine($"Fecha Nacimiento: {pacienteActual.FechaNacimiento:dd/MM/yyyy}");
                writer.WriteLine($"Edad: {CalcularEdad(pacienteActual.FechaNacimiento)}");
                writer.WriteLine($"Contacto: {pacienteActual.CelularRepresentante} | {pacienteActual.CorreoRepresentante}");
                writer.WriteLine();

                // Historial completo de consultas
                writer.WriteLine("[HISTORIAL COMPLETO DE CONSULTAS]");
                foreach (var consulta in pacienteActual.Consultas.OrderBy(c => c.FechaConsulta))
                {
                    writer.WriteLine($"Fecha: {consulta.FechaConsulta:dd/MM/yyyy HH:mm}");
                    writer.WriteLine($"Motivo: {consulta.MotivoConsulta}");
                    writer.WriteLine($"Peso: {consulta.Peso} kg | Talla: {consulta.Talla} cm | IMC: {consulta.IMC:F2}");
                    writer.WriteLine($"Observaciones: {consulta.Observaciones}");
                    writer.WriteLine(new string('-', 50));
                }
                writer.WriteLine();

                // Estado actual de vacunación
                writer.WriteLine("[VACUNAS APLICADAS]");
                foreach (var vacuna in pacienteActual.Vacunas.Where(v => v.Aplicada))
                {
                    writer.WriteLine($"- {vacuna.Nombre} ({vacuna.EdadRecomendada}) " +
                        $"Aplicada el: {vacuna.FechaAplicacion:dd/MM/yyyy}");
                }
            }
        }

        private string CalcularEdad(DateTime fechaNacimiento)
        {
            var edad = DateTime.Today - fechaNacimiento;
            int meses = (int)(edad.TotalDays / 30.4368);

            if (meses >= 24) return $"{meses / 12} años";
            if (meses >= 12) return $"{meses / 12} año y {meses % 12} meses";
            return $"{meses} meses";
        }

        private void CargarDatosPaciente(PacientePediatrico paciente)
        {
            txtNombrePaciente.Text = paciente.NombrePaciente;
            txtNombreRepresentante.Text = paciente.NombreRepresentante;
            txtCorreoRepresentante.Text = paciente.CorreoRepresentante;
            txtCelularRepresentante.Text = paciente.CelularRepresentante;
             

            var ultimaConsulta = paciente.Consultas.LastOrDefault();
            if (ultimaConsulta != null)
            {
                txtPeso.Text = ultimaConsulta.Peso.ToString();
                txtTalla.Text = ultimaConsulta.Talla.ToString();
                txtPerimetroCefalico.Text = ultimaConsulta.PerimetroCefalico.ToString();
                txtIMC.Text = ultimaConsulta.IMC.ToString("F2");
            }
        }

        private List<PacientePediatrico> ObtenerRegistros()
        {
            if (localSettings.Values.TryGetValue("RegistrosPediatricos", out object value))
            {
                return JsonConvert.DeserializeObject<List<PacientePediatrico>>(value.ToString())
                    ?? new List<PacientePediatrico>();
            }
            return new List<PacientePediatrico>();
        }

        private async Task MostrarError(string mensaje)
        {
            await new ContentDialog
            {
                Title = "Error",
                Content = mensaje,
                CloseButtonText = "OK"
            }.ShowAsync();
        }

        private async Task MostrarArchivoTXT()
        {
            try
            {
                string rutaArchivo = Path.Combine(historiasClinicasFolder, $"HC_PED_{pacienteActual.Id}.txt");
                string contenido = await File.ReadAllTextAsync(rutaArchivo);

                var dialog = new ContentDialog
                {
                    Title = "Historia Clínica Actualizada",
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
            catch (Exception ex)
            {
                await MostrarError($"Error al mostrar archivo: {ex.Message}");
            }
        }

        private void Cancelar_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void txtEdad_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void GenerarReceta_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Plantilla.RecetaMedica));
        }
    }
}