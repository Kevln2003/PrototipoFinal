using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Newtonsoft.Json;
using PrototipoFinal.Models.PrototipoFinal.Models;

namespace PrototipoFinal.Pediatria
{
    public sealed partial class FormularioDeMedicinaPediatricaL : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private readonly string historiasClinicasFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "HistoriasClinicasPediatricas");
        private List<PacientePediatrico.Vacuna> listaVacunas;

        public FormularioDeMedicinaPediatricaL()
        {
            this.InitializeComponent();
            Directory.CreateDirectory(historiasClinicasFolder);
            InitializeVacunas();
        }

        private void InitializeVacunas()
        {
            listaVacunas = new List<PacientePediatrico.Vacuna>
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
            };
            VacunasList.ItemsSource = listaVacunas;
        }

        private async void GuardarDatos_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Lista para almacenar los mensajes de error
                List<string> mensajesError = new List<string>();

                // Limpiar resaltados y mensajes previos
                LimpiarResaltados();

                // Validaciones de campos obligatorios
                if (string.IsNullOrWhiteSpace(txtNombrePaciente.Text))
                {
                    mensajesError.Add("Nombre del paciente es obligatorio.");
                    ResaltarCampo(txtNombrePaciente, "Este campo es obligatorio.");
                }

                if (string.IsNullOrWhiteSpace(txtCedulaPaciente.Text))
                {
                    mensajesError.Add("Cédula del paciente es obligatoria.");
                    ResaltarCampo(txtCedulaPaciente, "Este campo es obligatorio.");
                }
                else if (!CedulaValidator.ValidarCedula(txtCedulaPaciente.Text))
                {
                    mensajesError.Add("La cédula no es válida.");
                    ResaltarCampo(txtCedulaPaciente, "Cédula inválida.");
                }

                if (string.IsNullOrWhiteSpace(txtNombreRepresentante.Text))
                {
                    mensajesError.Add("Nombre del representante es obligatorio.");
                    ResaltarCampo(txtNombreRepresentante, "Este campo es obligatorio.");
                }

                if (string.IsNullOrWhiteSpace(txtCorreoRepresentante.Text) &&
                    string.IsNullOrWhiteSpace(txtCelularRepresentante.Text))
                {
                    mensajesError.Add("Debe proporcionar al menos un método de contacto (correo o celular).");
                    ResaltarCampo(txtCorreoRepresentante, "Este campo es obligatorio.");
                    ResaltarCampo(txtCelularRepresentante, "Este campo es obligatorio.");
                }

                // Validaciones de formatos
                if (!string.IsNullOrWhiteSpace(txtCorreoRepresentante.Text) &&
                    !Regex.IsMatch(txtCorreoRepresentante.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    mensajesError.Add("El correo electrónico no es válido.");
                    ResaltarCampo(txtCorreoRepresentante, "Correo inválido.");
                }

                if (!string.IsNullOrWhiteSpace(txtCelularRepresentante.Text) &&
                    !Regex.IsMatch(txtCelularRepresentante.Text, @"^\d+$"))
                {
                    mensajesError.Add("El número de contacto solo debe contener números.");
                    ResaltarCampo(txtCelularRepresentante, "Número inválido.");
                }

                // Validaciones del tutor legal
                if (pnlTutorLegal.Visibility == Visibility.Visible)
                {
                    if (string.IsNullOrWhiteSpace(txtTutorNombre.Text))
                    {
                        mensajesError.Add("El nombre del tutor es obligatorio.");
                        ResaltarCampo(txtTutorNombre, "Este campo es obligatorio.");
                    }

                    if (string.IsNullOrWhiteSpace(txtTutorContacto.Text))
                    {
                        mensajesError.Add("El contacto del tutor es obligatorio.");
                        ResaltarCampo(txtTutorContacto, "Este campo es obligatorio.");
                    }
                    else if (!Regex.IsMatch(txtTutorContacto.Text, @"^\d+$"))
                    {
                        mensajesError.Add("El contacto del tutor solo debe contener números.");
                        ResaltarCampo(txtTutorContacto, "Número inválido.");
                    }
                }

                // Si hay errores, mostrarlos y no continuar
                if (mensajesError.Count > 0)
                {
                    await MostrarError(string.Join("\n", mensajesError));
                    return;
                }

                // Crear nuevo paciente con los datos del formulario
                var nuevoPaciente = new PacientePediatrico
                {
                    NombrePaciente = txtNombrePaciente.Text?.Trim() ?? "",
                    CedulaPaciente = txtCedulaPaciente.Text?.Trim() ?? "",
                    NombreRepresentante = txtNombreRepresentante.Text?.Trim() ?? "",
                    CorreoRepresentante = txtCorreoRepresentante.Text?.Trim() ?? "",
                    CelularRepresentante = txtCelularRepresentante.Text?.Trim() ?? "",
                    FechaNacimiento = dpFechaNacimiento.Date.DateTime,
                    Vacunas = listaVacunas.Where(v => v.Aplicada).ToList()
                };

                // Obtener registros existentes
                var registros = ObtenerRegistrosPediatricos();
                var registroExistente = registros.FirstOrDefault(r => r.CedulaPaciente == nuevoPaciente.CedulaPaciente);

                if (registroExistente != null)
                {
                    // Actualizar registro existente
                    registroExistente.NombrePaciente = nuevoPaciente.NombrePaciente;
                    registroExistente.NombreRepresentante = nuevoPaciente.NombreRepresentante;
                    registroExistente.CorreoRepresentante = nuevoPaciente.CorreoRepresentante;
                    registroExistente.CelularRepresentante = nuevoPaciente.CelularRepresentante;
                    registroExistente.Vacunas = nuevoPaciente.Vacunas;
                }
                else
                {
                    // Agregar nuevo registro
                    registros.Add(nuevoPaciente);
                }

                // Guardar en LocalSettings
                string jsonDatos = JsonConvert.SerializeObject(registros, Formatting.Indented);
                localSettings.Values["RegistrosPediatricos"] = jsonDatos;

                // Generar/Actualizar archivo TXT de historia clínica
                await GenerarHistoriaClinicaPediatricaTXT(nuevoPaciente);

                await MostrarArchivoTXT(nuevoPaciente.CedulaPaciente);
                LimpiarFormulario();
                Frame.GoBack();
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al guardar los datos: {ex.Message}");
            }
        }

        private List<PacientePediatrico> ObtenerRegistrosPediatricos()
        {
            try
            {
                var jsonDatos = localSettings.Values["RegistrosPediatricos"] as string;
                if (string.IsNullOrEmpty(jsonDatos))
                    return new List<PacientePediatrico>();

                var registros = JsonConvert.DeserializeObject<List<PacientePediatrico>>(jsonDatos);
                return registros ?? new List<PacientePediatrico>();
            }
            catch
            {
                return new List<PacientePediatrico>();
            }
        }

        private async Task GenerarHistoriaClinicaPediatricaTXT(PacientePediatrico paciente)
        {
            try
            {
                // Obtener la carpeta de historias clínicas
                StorageFolder historiasFolder = await ObtenerCarpetaHistoriasClinicas();

                // Crear o sobrescribir el archivo
                StorageFile file = await historiasFolder.CreateFileAsync(
                    $"HC_PED_{paciente.CedulaPaciente}.txt",
                    CreationCollisionOption.ReplaceExisting);

                // Escribir el contenido
                using (StreamWriter writer = new StreamWriter(await file.OpenStreamForWriteAsync()))
                {
                    writer.WriteLine("HISTORIA CLÍNICA PEDIÁTRICA");
                    writer.WriteLine("===========================");
                    writer.WriteLine($"Nombre: {paciente.NombrePaciente}");
                    writer.WriteLine($"Cédula: {paciente.CedulaPaciente}");
                    writer.WriteLine($"Fecha de Nacimiento: {paciente.FechaNacimiento:dd/MM/yyyy}");
                    writer.WriteLine();
                    // ... más detalles
                }
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al generar el archivo: {ex.Message}");
            }
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

        private void ResaltarCampo(Control campo, string mensajeError)
        {
            if (campo is TextBox textBox)
            {
                textBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                textBox.BorderThickness = new Thickness(2);
                MostrarMensajeError(textBox, mensajeError);
            }
            else if (campo is ComboBox comboBox)
            {
                comboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                comboBox.BorderThickness = new Thickness(2);
                MostrarMensajeError(comboBox, mensajeError);
            }
            else if (campo is DatePicker datePicker)
            {
                datePicker.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                datePicker.BorderThickness = new Thickness(2);
                MostrarMensajeError(datePicker, mensajeError);
            }
        }

        private void MostrarMensajeError(Control campo, string mensajeError)
        {
            var contenedor = campo.Parent as Panel;
            if (contenedor != null)
            {
                TextBlock errorMessage = new TextBlock
                {
                    Name = "errorMessage",
                    Text = mensajeError,
                    Foreground = new SolidColorBrush(Windows.UI.Colors.Red),
                    Margin = new Thickness(0, 5, 0, 0)
                };
                contenedor.Children.Add(errorMessage);
            }
        }

        private void LimpiarResaltados()
        {
            EliminarMensajeError(txtNombrePaciente);
            EliminarMensajeError(txtCedulaPaciente);
            EliminarMensajeError(txtNombreRepresentante);
            EliminarMensajeError(txtCorreoRepresentante);
            EliminarMensajeError(txtCelularRepresentante);
            EliminarMensajeError(txtTutorNombre);
            EliminarMensajeError(txtTutorContacto);
        }

        private void EliminarMensajeError(Control campo)
        {
            var contenedor = campo.Parent as Panel;
            if (contenedor != null)
            {
                foreach (var child in contenedor.Children)
                {
                    if (child is TextBlock errorMessage && errorMessage.Name == "errorMessage")
                    {
                        contenedor.Children.Remove(errorMessage);
                        break;
                    }
                }
            }
        }
    }
}