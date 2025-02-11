using System;
using System.Collections.Generic;
using System.Linq;
using PrototipoFinal.MedicinaDeportiva;
using PrototipoFinal.Models;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PrototipoFinal.Models.PrototipoFinal.Models;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PrototipoFinal.Pediatria
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class BusquedaDePacientePediatrico : Page
    {
            private ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
            private string tipoBusqueda;



            public BusquedaDePacientePediatrico()
            {
                this.InitializeComponent();
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
                else
                {
                    List<PacientePediatrico> resultadosPediatricos;
                    if (cmbTipoBusqueda.SelectedIndex == 0) // Búsqueda por cédula
                    {
                        resultadosPediatricos = await FormularioDeMedicinaPediatrica.BuscarPorCedula1(terminoBusqueda);
                    }
                    else // Búsqueda por nombre
                    {
                        resultadosPediatricos = await FormularioDeMedicinaPediatrica.BuscarPorNombre1(terminoBusqueda);
                    }

                    // Convertir resultados para la vista
                    var resultadosVista = resultadosPediatricos.Select(r => new {
                        NombreCompleto = $"{r.NombrePaciente} {r.NombrePaciente}",
                        r.CedulaPaciente,
                        FechaRegistro = r.FechaConsulta.ToString("dd/MM/yyyy"),
                        IMC = r.IMC.ToString("F2"),
                        DatosOriginales = r
                    }).ToList();

                    grdResultados.ItemsSource = resultadosVista;

                    if (!resultadosVista.Any())
                    {
                        await MostrarMensaje("Información", "No se encontraron resultados para la búsqueda.");
                    }
                }
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al realizar la búsqueda: {ex.Message}");
            }
        }





            private async Task MostrarMensajeSiNoResultados<T>(List<T> resultadosVista)
            {
                if (!resultadosVista.Any())
                {
                    await MostrarMensaje("Información", "No se encontraron resultados para la búsqueda.");
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
            PacientePediatrico datosCompletos = item.DatosOriginales;

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

