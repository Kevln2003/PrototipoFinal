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
using Windows.Graphics.Printing;
using static PrototipoFinal.MedicinaDeportiva.FormularioDeMedicinaDeportiva;
using PrototipoFinal.MedicinaDeportiva;
using PrototipoFinal.Models.PrototipoFinal.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PrototipoFinal.Plantilla
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class SeguimientoDePacienteBusqueda : Page
    {
        private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;

        public SeguimientoDePacienteBusqueda()
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

                // Convertir resultados al formato de visualización
                var resultadosVista = resultados.Select(r => new {
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

            dynamic item = e.ClickedItem;
            PacienteDeportivo datosCompletos = item.DatosOriginales;

            // Navigate to FormularioSeguimiento and pass the patient data
            Frame.Navigate(typeof(FormularioDeSeguimientoDePacienteDepotivo), datosCompletos);

           
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
