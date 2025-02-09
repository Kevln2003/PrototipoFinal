using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using PrototipoFinal.Plantilla;
using PrototipoFinal.Models.PrototipoFinal.Models;
using System.IO;


// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PrototipoFinal.MedicinaDeportiva
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class FormularioDeMedicinaDeportiva : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private readonly string historiasClinicasFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "HistoriasClinicas");

        public FormularioDeMedicinaDeportiva()
        {
            this.InitializeComponent();
            CalcularIMCAutomaticamente();
            Directory.CreateDirectory(historiasClinicasFolder);
        }

        private async void GuardarDatos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validación básica de campos obligatorios
                if (string.IsNullOrWhiteSpace(txtCorreo.Text) &&
                    string.IsNullOrWhiteSpace(txtCelular.Text))
                {
                    await MostrarError("Debe proporcionar al menos un método de contacto.");
                    return;
                }

                // Crear nuevo paciente con los datos del formulario
                var nuevoPaciente = new PacienteDeportivo
                {
                    Nombres = txtNombres.Text?.Trim() ?? "",
                    Apellidos = txtApellidos.Text?.Trim() ?? "",
                    Cedula = txtCedula.Text?.Trim() ?? "",
                    Correo = txtCorreo.Text?.Trim() ?? "",
                    Celular = txtCelular.Text?.Trim() ?? "",
                    Peso = double.TryParse(txtPeso.Text?.Replace(",", "."), out double peso) ? peso : 0,
                    Altura = double.TryParse(txtAltura.Text?.Replace(",", "."), out double altura) ? altura : 0,
                    IMC = double.TryParse(txtIMC.Text?.Replace(",", "."), out double imc) ? imc : 0,
                    FechaRegistro = DateTime.Now,
                    AntecedentesFamiliares = txtAntecedentesFamiliares.Text?.Trim() ?? "",
                    DeportesPracticados = txtDeportesPracticados.Text?.Trim() ?? ""
                };

                // Obtener registros existentes
                var registros = ObtenerRegistros();
                var registroExistente = registros.FirstOrDefault(r => r.Cedula == nuevoPaciente.Cedula);

                if (registroExistente != null)
                {
                    // Actualizar registro existente
                    registroExistente.Nombres = nuevoPaciente.Nombres;
                    registroExistente.Apellidos = nuevoPaciente.Apellidos;
                    registroExistente.Correo = nuevoPaciente.Correo;
                    registroExistente.Celular = nuevoPaciente.Celular;
                    registroExistente.Peso = nuevoPaciente.Peso;
                    registroExistente.Altura = nuevoPaciente.Altura;
                    registroExistente.IMC = nuevoPaciente.IMC;
                    registroExistente.AntecedentesFamiliares = nuevoPaciente.AntecedentesFamiliares;
                    registroExistente.DeportesPracticados = nuevoPaciente.DeportesPracticados;
                }
                else
                {
                    // Agregar nuevo registro
                    registros.Add(nuevoPaciente);
                }

                // Guardar en LocalSettings
                string jsonDatos = JsonConvert.SerializeObject(registros, Formatting.Indented);
                localSettings.Values["RegistrosMedicos"] = jsonDatos;

                // Generar/Actualizar archivo TXT de historia clínica
                await GenerarHistoriaClinicaTXT(registroExistente ?? nuevoPaciente);


                await MostrarArchivoTXT(nuevoPaciente.Cedula);
                LimpiarFormulario();
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

        private async Task GenerarHistoriaClinicaTXT(PacienteDeportivo paciente)
        {
            string nombreArchivo = Path.Combine(historiasClinicasFolder, $"HC_{paciente.Cedula}.txt");

            using (StreamWriter writer = new StreamWriter(nombreArchivo, false))
            {
                writer.WriteLine("HISTORIA CLÍNICA DEPORTIVA");
                writer.WriteLine("==========================");
                writer.WriteLine($"Fecha de Actualización: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
                writer.WriteLine();

                // Datos personales
                writer.WriteLine("DATOS PERSONALES");
                writer.WriteLine("----------------");
                writer.WriteLine($"Nombres: {paciente.Nombres}");
                writer.WriteLine($"Apellidos: {paciente.Apellidos}");
                writer.WriteLine($"Cédula: {paciente.Cedula}");
                writer.WriteLine($"Correo: {paciente.Correo}");
                writer.WriteLine($"Celular: {paciente.Celular}");
                writer.WriteLine();

                // Datos médicos y antropométricos
                writer.WriteLine("DATOS MÉDICOS Y ANTROPOMÉTRICOS");
                writer.WriteLine("-------------------------------");
                writer.WriteLine($"Peso: {paciente.Peso:F2} kg");
                writer.WriteLine($"Altura: {paciente.Altura:F2} m");
                writer.WriteLine($"IMC: {paciente.IMC:F2}");
                writer.WriteLine($"Clasificación IMC: {ObtenerClasificacionIMC(paciente.IMC)}");
                writer.WriteLine();

                // Antecedentes y deportes
                writer.WriteLine("ANTECEDENTES Y ACTIVIDAD DEPORTIVA");
                writer.WriteLine("---------------------------------");
                writer.WriteLine($"Antecedentes Familiares: {paciente.AntecedentesFamiliares}");
                writer.WriteLine($"Deportes Practicados: {paciente.DeportesPracticados}");
                writer.WriteLine();

                // Información de registro
                writer.WriteLine("INFORMACIÓN DE REGISTRO");
                writer.WriteLine("----------------------");
                writer.WriteLine($"Fecha de Registro Inicial: {paciente.FechaRegistro:dd/MM/yyyy}");
                writer.WriteLine($"Última Actualización: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
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

        public static async Task<List<PacienteDeportivo>> BuscarPorCedula(string cedula)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("RegistrosMedicos", out object value))
            {
                string json = value.ToString();
                var todos = JsonConvert.DeserializeObject<List<PacienteDeportivo>>(json);
                return todos?.Where(x => x.Cedula.Contains(cedula)).ToList() ??
                       new List<PacienteDeportivo>();
            }
            return new List<PacienteDeportivo>();
        }

        public static async Task<List<PacienteDeportivo>> BuscarPorNombre(string nombre)
        {
            var localSettings = ApplicationData.Current.LocalSettings;
            if (localSettings.Values.TryGetValue("RegistrosMedicos", out object value))
            {
                string json = value.ToString();
                var todos = JsonConvert.DeserializeObject<List<PacienteDeportivo>>(json);
                return todos?.Where(x =>
                    x.Nombres.ToLower().Contains(nombre.ToLower()) ||
                    x.Apellidos.ToLower().Contains(nombre.ToLower())
                ).ToList() ?? new List<PacienteDeportivo>();
            }
            return new List<PacienteDeportivo>();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Buscar));
        }        
        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(RecetaMedica));
        }
    }
}
