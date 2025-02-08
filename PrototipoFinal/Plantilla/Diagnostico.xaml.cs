using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.Storage;
using Windows.System;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;

namespace PrototipoFinal.Plantilla
{
    public sealed partial class RecetaMedica : Page
    {
        public RecetaMedica()
        {
            InitializeComponent();
            InicializarPagina();
        }

        private void InicializarPagina()
        {
            txtFecha.Text = $"Fecha: {DateTime.Now.ToShortDateString()}";
        }

        private void AgregarRP_Click(object sender, RoutedEventArgs e)
        {
            // Crear nuevo grupo de RP e Indicaciones
            var rpGrid = new Grid
            {
                Margin = new Thickness(0, 10, 0, 20)
            };

            rpGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });
            rpGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Auto) });

            var txtNuevoRP = new TextBox
            {
                Header = "RP:",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 100,
                Margin = new Thickness(0, 0, 0, 10)
            };

            var txtNuevasIndicaciones = new TextBox
            {
                Header = "Indicaciones:",
                TextWrapping = TextWrapping.Wrap,
                AcceptsReturn = true,
                Height = 100
            };

            Grid.SetRow(txtNuevoRP, 0);
            Grid.SetRow(txtNuevasIndicaciones, 1);

            rpGrid.Children.Add(txtNuevoRP);
            rpGrid.Children.Add(txtNuevasIndicaciones);

            // Insertar antes del botón de agregar
            int lastIndex = rpContainer.Children.Count - 1;
            rpContainer.Children.Insert(lastIndex, rpGrid);
        }

        private async void GenerarPDF_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Generar el nombre del archivo
                string fileName = $"Receta_{DateTime.Now:yyyyMMddHHmmss}.pdf";
                
                // Obtener la carpeta de documentos
                StorageFolder documentsFolder = 
                    await ApplicationData.Current.LocalFolder.CreateFolderAsync("Recetas", 
                    CreationCollisionOption.OpenIfExists);

                // Crear el archivo
                StorageFile pdfFile = 
                    await documentsFolder.CreateFileAsync(fileName, 
                    CreationCollisionOption.ReplaceExisting);

                // Generar el PDF
                using (Stream stream = await pdfFile.OpenStreamForWriteAsync())
                {
                    Document document = new Document(PageSize.A4, 50, 50, 50, 50);
                    PdfWriter writer = PdfWriter.GetInstance(document, stream);

                    document.Open();

                    // Agregar contenido al PDF
                    // ... (código para agregar el contenido al PDF)

                    document.Close();
                }

                ContentDialog dialog = new ContentDialog
                {
                    Title = "PDF Generado",
                    Content = "El PDF se ha generado correctamente.",
                    CloseButtonText = "Ok"
                };

                await dialog.ShowAsync();
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Error al generar el PDF: {ex.Message}",
                    CloseButtonText = "Ok"
                };

                await dialog.ShowAsync();
            }
        }

        private async void EnviarWhatsApp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string numeroWhatsApp = $"593{txtCelular.Text.Trim()}";
                string mensaje = Uri.EscapeDataString("Adjunto su receta médica.");
                
                // Construir la URL de WhatsApp
                string whatsappUrl = $"https://wa.me/{numeroWhatsApp}?text={mensaje}";
                
                // Abrir WhatsApp Web
                await Launcher.LaunchUriAsync(new Uri(whatsappUrl));
            }
            catch (Exception ex)
            {
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Error",
                    Content = $"Error al abrir WhatsApp: {ex.Message}",
                    CloseButtonText = "Ok"
                };

                await dialog.ShowAsync();
            }
        }
    }
}