using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using PrototipoFinal.MedicinaDeportiva;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using static PrototipoFinal.MedicinaDeportiva.FormularioDeMedicinaDeportiva;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

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

                List<DatosMedicoDeportivos> resultados;

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
            // Aquí puedes implementar la navegación al detalle del paciente
            // o mostrar más información en un diálogo
            dynamic item = e.ClickedItem;
            await MostrarMensaje("Detalle del Paciente",
                $"Nombre: {item.NombreCompleto}\n" +
                $"Cédula: {item.Cedula}\n" +
                $"IMC: {item.IMC}");
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
