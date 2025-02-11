using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using PrototipoFinal.MedicinaDeportiva;
using PrototipoFinal.Models.PrototipoFinal.Models;
using PrototipoFinal.Models;
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
         
                    else if (tipoBusqueda == "Pediatría")
                    {
                        await RealizarBusquedaPediatrica(terminoBusqueda);
                    }
                }
                catch (Exception ex)
                {
                    await MostrarError($"Error al realizar la búsqueda: {ex.Message}");
                }
            }

            private async Task RealizarBusquedaDeportiva(string terminoBusqueda)
            {
                List<PacienteDeportivo> resultadosDeportivos = cmbTipoBusqueda.SelectedIndex == 0
                    ? await FormularioDeMedicinaDeportiva.BuscarPorCedula(terminoBusqueda)
                    : await FormularioDeMedicinaDeportiva.BuscarPorNombre(terminoBusqueda);

                var resultadosVista = resultadosDeportivos.Select(r => new
                {
                    NombreCompleto = $"{r.Nombres} {r.Apellidos}",
                    r.Cedula,
                    FechaRegistro = r.FechaRegistro.ToString("dd/MM/yyyy"),
                    IMC = r.IMC.ToString("F2"),
                    DatosOriginales = r
                }).ToList();

                grdResultados.ItemsSource = resultadosVista;
                await MostrarMensajeSiNoResultados(resultadosVista);
            }

            private async Task RealizarBusquedaPediatrica(string terminoBusqueda)
            {
                List<PacientePediatrico> resultadosPediatricos = cmbTipoBusqueda.SelectedIndex == 0
                    ? await FormularioDeMedicinaPediatrica.BuscarPorCedula1(terminoBusqueda)
                    : await FormularioDeMedicinaPediatrica.BuscarPorNombre1(terminoBusqueda);

                var resultadosVista = resultadosPediatricos.Select(r => new
                {
                    NombreCompleto = $"{r.NombrePaciente} {r.NombrePaciente}",
                    r.CedulaPaciente,
                    DatosOriginales = r
                }).ToList();

                grdResultados.ItemsSource = resultadosVista;
                await MostrarMensajeSiNoResultados(resultadosVista);
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
                if (tipoBusqueda == "Deportiva")
                {
                    PacienteDeportivo datosCompletos = item.DatosOriginales;
                    Frame.Navigate(typeof(FormularioDeSeguimientoDePacienteDepotivo), datosCompletos);
                }
                else if (tipoBusqueda == "Pediatría")
                {
                    PacientePediatrico datosCompletos = item.DatosOriginales;
                    Frame.Navigate(typeof(FormularioDeSeguimientoDePacienteDepotivo), datosCompletos);
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
        }
    }

