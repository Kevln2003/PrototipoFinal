using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PrototipoFinal.Plantilla
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Factura : Page
    {
        public Factura()
        {
            this.InitializeComponent();
        }

        private async void MostrarFactura_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Validación básica de campos
                if (string.IsNullOrWhiteSpace(txtNombreCliente.Text) ||
                    string.IsNullOrWhiteSpace(txtProducto.Text) ||
                    string.IsNullOrWhiteSpace(txtPrecio.Text) ||
                    string.IsNullOrWhiteSpace(txtCantidad.Text))
                {
                    await MostrarError("Debe completar todos los campos.");
                    return;
                }

                // Crear una nueva factura con los datos del formulario
                var nuevaFactura = new Factura
                {
                    NombreCliente = txtNombreCliente.Text?.Trim() ?? "",
                    Producto = txtProducto.Text?.Trim() ?? "",
                    Precio = double.TryParse(txtPrecio.Text?.Replace(",", "."), out double precio) ? precio : 0,
                    Cantidad = int.TryParse(txtCantidad.Text?.Trim(), out int cantidad) ? cantidad : 0,
                    Total = double.TryParse(txtPrecio.Text?.Replace(",", "."), out double precioFactura) ? precioFactura * cantidad : 0
                };

                // Mostrar los datos de la factura
                string facturaDetalles = $"Nombre Cliente: {nuevaFactura.NombreCliente}\n" +
                                         $"Producto: {nuevaFactura.Producto}\n" +
                                         $"Precio Unitario: {nuevaFactura.Precio} €\n" +
                                         $"Cantidad: {nuevaFactura.Cantidad}\n" +
                                         $"Total: {nuevaFactura.Total} €";

                // Mostrar los datos de la factura en un cuadro de mensaje
                await MostrarMensaje("Factura Generada", facturaDetalles);
            }
            catch (Exception ex)
            {
                await MostrarError($"Error al mostrar la factura: {ex.Message}");
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