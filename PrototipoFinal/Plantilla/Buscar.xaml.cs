using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Newtonsoft.Json;
using PrototipoFinal.MedicinaDeportiva;
using PrototipoFinal.Models.PrototipoFinal.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using PdfSharp.Pdf;
using PdfSharp.Drawing;
using static PrototipoFinal.MedicinaDeportiva.FormularioDeMedicinaDeportiva;
using System.Text.RegularExpressions;

namespace PrototipoFinal.Plantilla
{
    public sealed partial class Buscar : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public Buscar()
        {
            this.InitializeComponent();
            cmbTipoBusqueda.SelectedIndex = 0;
        }

        private async void BtnBuscar_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string terminoBusqueda = txtBusqueda.Text.Trim();

                if (string.IsNullOrWhiteSpace(terminoBusqueda))
                {
                    await MostrarError("Por favor ingrese un término de búsqueda.");
                    return;
                }

                List<PacienteDeportivo> resultados;

                if (cmbTipoBusqueda.SelectedIndex == 0) // Búsqueda por cédula
                {
                    resultados = await FormularioDeMedicinaDeportiva.BuscarPorCedula(terminoBusqueda);
                }
                else // Búsqueda por nombre
                {
                    resultados = await FormularioDeMedicinaDeportiva.BuscarPorNombre(terminoBusqueda);
                }

                var resultadosVista = resultados.Select(r => new
                {
                    NombreCompleto = $"{r.Nombres} {r.Apellidos}",
                    r.Cedula,
                    FechaRegistro = r.FechaRegistro.ToString("dd/MM/yyyy"),
                    IMC = r.IMC.ToString("F2"),
                    DatosOriginales = r
                }).ToList();

                grdResultados.ItemsSource = resultadosVista;

                if (!resultados.Any())
                {
                    await MostrarMensaje("Información", "No se encontraron resultados para la búsqueda.");
                }
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al realizar la búsqueda: {ex.Message}");
            }
        }
        private async Task ConvertirTXTaPDFConITextSharp(string txtPath, string pdfPath)
        {
            try
            {
                string contenido = await File.ReadAllTextAsync(txtPath);
                using (FileStream fs = new FileStream(pdfPath, FileMode.Create))
                {
                    Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                    PdfWriter writer = PdfWriter.GetInstance(document, fs);

                    document.Open();

                    // Agregar título
                    Font titleFont = new Font(Font.FontFamily.HELVETICA, 16, Font.BOLD);
                    Paragraph titulo = new Paragraph("HISTORIA CLÍNICA DEPORTIVA", titleFont);
                    titulo.Alignment = Element.ALIGN_CENTER;
                    titulo.SpacingAfter = 20f;
                    document.Add(titulo);

                    // Agregar contenido
                    Font contentFont = new Font(Font.FontFamily.HELVETICA, 12);
                    foreach (string linea in contenido.Split('\n'))
                    {
                        Paragraph p = new Paragraph(linea, contentFont);
                        p.SpacingAfter = 10f;
                        document.Add(p);
                    }

                    // Agregar pie de página
                    Font footerFont = new Font(Font.FontFamily.HELVETICA, 10, Font.ITALIC);
                    Paragraph footer = new Paragraph($"Documento generado el: {DateTime.Now:dd/MM/yyyy HH:mm:ss}", footerFont);
                    footer.Alignment = Element.ALIGN_RIGHT;
                    document.Add(footer);

                    document.Close();
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al convertir a PDF con iTextSharp: {ex.Message}");
            }
        }
        // Método para enviar por WhatsApp
        private async Task EnviarPorWhatsApp(PacienteDeportivo paciente, string pdfPath)
        {
            try
            {
                if (!ValidarCelularEcuatoriano(paciente.Celular))
                {
                    await MostrarError("El número de celular no tiene un formato válido ecuatoriano (debe empezar con 09 y tener 10 dígitos).");
                    return;
                }

                string numeroWhatsApp = FormatearNumeroWhatsApp(paciente.Celular);
                string whatsappUrl = $"https://wa.me/{numeroWhatsApp}";

                ContentDialog dialog = new ContentDialog
                {
                    Title = "Enviar Historia Clínica",
                    Content = $"Se abrirá WhatsApp Web para enviar el documento al número {paciente.Celular}.\n\nEl PDF se ha guardado en:\n{pdfPath}",
                    PrimaryButtonText = "Abrir WhatsApp",
                    SecondaryButtonText = "Cancelar"
                };

                var result = await dialog.ShowAsync();
                if (result == ContentDialogResult.Primary)
                {
                    await Launcher.LaunchUriAsync(new Uri(whatsappUrl));
                }
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al preparar el envío: {ex.Message}");
            }
        }
        private bool ValidarCelularEcuatoriano(string numero)
        {
            numero = Regex.Replace(numero, @"[\s\-\(\)]", "");
            return Regex.IsMatch(numero, @"^09\d{8}$");
        }

        private string FormatearNumeroWhatsApp(string numero)
        {
            numero = Regex.Replace(numero, @"[^\d]", "");
            return numero.StartsWith("0") ? "593" + numero.Substring(1) : numero;
        }
        private void CmbTipoBusqueda_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (txtBusqueda != null)
            {
                txtBusqueda.PlaceholderText = cmbTipoBusqueda.SelectedIndex == 0
                    ? "Ingrese el número de cédula"
                    : "Ingrese el nombre o apellido";
            }
        }

        private async void GrdResultados_ItemClick(object sender, ItemClickEventArgs e)
        {
            try
            {
                dynamic item = e.ClickedItem;
                PacienteDeportivo datos = item.DatosOriginales;

                if (datos == null)
                {
                    await MostrarError("No se pudo recuperar la información del paciente.");
                    return;
                }

                // Convertir DatosMedicoDeportivos a PacienteDeportivo
                PacienteDeportivo paciente = new PacienteDeportivo
                {
                    Id = Guid.NewGuid().ToString(), // Generar un ID si es necesario
                    Nombres = datos.Nombres ?? "No disponible",
                    Apellidos = datos.Apellidos ?? "No disponible",
                    Cedula = datos.Cedula ?? "No disponible",
                    Correo = datos.Correo ?? "No disponible",
                    Celular = datos.Celular ?? "No disponible",
                    Peso = datos.Peso,
                    Altura = datos.Altura,
                    IMC = datos.IMC,
                    FechaRegistro = datos.FechaRegistro
                };

                var savePicker = new FileSavePicker
                {
                    SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
                    SuggestedFileName = $"HistorialMedico_{paciente.Cedula}"
                };
                savePicker.FileTypeChoices.Add("PDF", new List<string>() { ".pdf" });

                StorageFile pdfFile = await savePicker.PickSaveFileAsync();
                if (pdfFile != null)
                {
                    string pdfContent = GenerarContenidoPDF(paciente);

                    await FileIO.WriteTextAsync(pdfFile, pdfContent);

                    var dialog = new ContentDialog
                    {
                        Title = "PDF Generado",
                        Content = "El informe médico se ha generado correctamente. ¿Desea abrirlo?",
                        PrimaryButtonText = "Abrir",
                        SecondaryButtonText = "Cancelar"
                    };

                    var result = await dialog.ShowAsync();
                    if (result == ContentDialogResult.Primary)
                    {
                        await Launcher.LaunchFileAsync(pdfFile);
                    }
                }
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al generar el PDF: {ex.Message}");
            }
        }

        private string GenerarContenidoPDF(PacienteDeportivo paciente)
        {
            return $@"HISTORIAL MÉDICO DEPORTIVO

INFORMACIÓN PERSONAL
--------------------
Nombre Completo: {paciente.Nombres} {paciente.Apellidos}
Cédula: {paciente.Cedula}
Correo Electrónico: {paciente.Correo}
Teléfono: {paciente.Celular}

DATOS FÍSICOS
-------------
Peso: {paciente.Peso} kg
Altura: {paciente.Altura} m
Índice de Masa Corporal (IMC): {paciente.IMC:F2}

INFORMACIÓN DE REGISTRO
----------------------
Fecha de Registro: {paciente.FechaRegistro:dd/MM/yyyy}

Generado el: {DateTime.Now:dd/MM/yyyy HH:mm}

NOTA: Este documento es un resumen informativo.
Consulte a su médico para un análisis detallado.";
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
    }
}
