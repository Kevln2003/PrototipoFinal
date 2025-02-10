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

namespace PrototipoFinal.Pediatria
{
    public sealed partial class FormularioDeMedicinaPediatrica : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private readonly string historiasClinicasFolder = Path.Combine(ApplicationData.Current.LocalFolder.Path, "HistoriasClinicasPediatricas");
        private List<PacientePediatrico.Vacuna> listaVacunas;

        public FormularioDeMedicinaPediatrica()
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
                // Validación básica de campos obligatorios
                if (string.IsNullOrWhiteSpace(txtCorreoRepresentante.Text) &&
                    string.IsNullOrWhiteSpace(txtCelularRepresentante.Text))
                {
                    await MostrarError("Debe proporcionar al menos un método de contacto.");
                    return;
                }

                // Crear nuevo paciente con los datos del formulario (versión simplificada)
                var nuevoPaciente = new PacientePediatrico
                {
                    NombrePaciente = txtNombrePaciente.Text?.Trim() ?? "",
                    CedulaPaciente = txtCedulaPaciente.Text?.Trim() ?? "",
                    NombreRepresentante = txtNombreRepresentante.Text?.Trim() ?? "",
                    CorreoRepresentante = txtCorreoRepresentante.Text?.Trim() ?? "",
                    CelularRepresentante = txtCelularRepresentante.Text?.Trim() ?? "",
                    FechaNacimiento = DateTime.Now,
                    
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
                    // Mantenemos las consultas y vacunas existentes
                    nuevoPaciente = registroExistente;
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
  

        private decimal CalcularIMC(decimal peso, decimal talla)
        {
            if (talla == 0) return 0;
            var tallaMetros = talla / 100;
            return peso / (tallaMetros * tallaMetros);
        }

        
        

        private string CalcularEdad(DateTime fechaNacimiento)
        {
            var edad = DateTime.Now - fechaNacimiento;
            int meses = (int)(edad.TotalDays / 30.4368);

            if (meses >= 24) return $"{meses / 12} años";
            if (meses >= 12) return $"{meses / 12} año y {meses % 12} meses";
            return $"{meses} meses";
        }

        private void LimpiarFormulario()
        {
            // Limpiar todos los campos
            txtNombrePaciente.Text = "";
            txtCedulaPaciente.Text = "";
            txtCedulaRepresentante.Text = "";
            dateFechaNacimiento.Date = DateTime.Now;
            txtNombreRepresentante.Text = "";
            txtCorreoRepresentante.Text = "";
            txtCelularRepresentante.Text = "";

            // Limpiar consulta
            txtTemperatura.Text = "";
            txtFrecuenciaCardiaca.Text = "";
            txtFrecuenciaRespiratoria.Text = "";
            txtSaturacion.Text = "";
            txtPeso.Text = "";
            txtTalla.Text = "";
            txtPerimetroCefalico.Text = "";
            txtPiel.Text = "";
            txtCabeza.Text = "";
            txtTorax.Text = "";
            txtAbdomen.Text = "";
            txtExtremidades.Text = "";
            txtNeurologico.Text = "";
            txtMotivo.Text = "";
            txtObservaciones.Text = "";

            // Reiniciar vacunas
            foreach (var vacuna in listaVacunas)
            {
                vacuna.Aplicada = false;
                vacuna.FechaAplicacion = null;
            }
            VacunasList.ItemsSource = null;
            VacunasList.ItemsSource = listaVacunas;
        }

        private async Task MostrarArchivoTXT(string id)
        {
            try
            {
                // Obtener la carpeta local de la aplicación
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;

                // Asegurarnos que existe la carpeta de historias clínicas
                StorageFolder historiasFolder = await localFolder.CreateFolderAsync(
                    "HistoriasClinicas",
                    CreationCollisionOption.OpenIfExists);

                // Obtener el archivo
                string nombreArchivo = $"HC_PED_{id}.txt";
                StorageFile file = await historiasFolder.GetFileAsync(nombreArchivo);

                if (file != null)
                {
                    // Leer el contenido del archivo
                    string contenido = await FileIO.ReadTextAsync(file);

                    // Crear y mostrar el diálogo
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
                            Height = 400,
                            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
                        },
                        CloseButtonText = "Cerrar",
                        DefaultButton = ContentDialogButton.Close
                    };

                    await dialog.ShowAsync();
                }
                else
                {
                    await MostrarError("No se encontró el archivo de historia clínica.");
                }
            }
            catch (FileNotFoundException)
            {
                await MostrarError("No se encontró el archivo de historia clínica.");
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al mostrar archivo: {ex.Message}");
            }
        }

        // Método para obtener la carpeta de historias clínicas
        private async Task<StorageFolder> ObtenerCarpetaHistoriasClinicas()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            return await localFolder.CreateFolderAsync(
                "HistoriasClinicas",
                CreationCollisionOption.OpenIfExists);
        }

        // Método para escribir la historia clínica
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
                    // ... (resto del contenido igual que antes)

                    // Datos personales
                    writer.WriteLine("DATOS DEL PACIENTE");
                    writer.WriteLine("----------------");
                    writer.WriteLine($"Nombre: {paciente.NombrePaciente}");
                    writer.WriteLine($"Cédula: {paciente.CedulaPaciente}");
                    writer.WriteLine($"Fecha de Nacimiento: {paciente.FechaNacimiento:dd/MM/yyyy}");
                    writer.WriteLine();

                    // Datos del representante
                    writer.WriteLine("DATOS DEL REPRESENTANTE");
                    writer.WriteLine("----------------------");
                    writer.WriteLine($"Nombre: {paciente.NombreRepresentante}");
                    writer.WriteLine($"Correo: {paciente.CorreoRepresentante}");
                    writer.WriteLine($"Celular: {paciente.CelularRepresentante}");
                    writer.WriteLine();

                    // El resto del contenido sigue igual...
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

        private void Cancelar_Click(object sender, RoutedEventArgs e) => Frame.GoBack();

        private void GenerarReceta_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Plantilla.RecetaMedica));
        }
    }
}