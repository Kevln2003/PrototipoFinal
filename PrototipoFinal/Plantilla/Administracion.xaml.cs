using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PrototipoFinal.Models;
using System.Linq;

namespace PrototipoFinal.Plantilla
{
    public sealed partial class Administracion : Page
    {
        public Administracion()
        {
            this.InitializeComponent();
        }

        private void CrearUsuarioButton_Click(object sender, RoutedEventArgs e)
        {
            // Lógica para crear usuario...
        }

        private void BuscarUsuarioButton_Click(object sender, RoutedEventArgs e)
        {
            string usuarioModificar = UsuarioModificarTextBox.Text;
            string especialidadFiltrada = (EspecialidadFiltroComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            var resultados = DataService.ObtenerUsuarios()
                .Where(u => (string.IsNullOrWhiteSpace(usuarioModificar) || u.NombreUsuario.Contains(usuarioModificar)) &&
                             (especialidadFiltrada == "Todas" || u.Especialidad == especialidadFiltrada))
                .ToList();

            if (resultados.Any())
            {
                grdResultados.ItemsSource = resultados;
            }
            else
            {
                MostrarMensajeError("No se encontraron resultados.");
            }
        }

        private async void GrdResultados_ItemClick(object sender, ItemClickEventArgs e)
        {
            Usuario usuarioSeleccionado = e.ClickedItem as Usuario;

            if (usuarioSeleccionado != null)
            {
                // Abrir un diálogo para modificar los datos del usuario
                ContentDialog dialog = new ContentDialog
                {
                    Title = "Modificar Usuario",
                    Content = new StackPanel
                    {
                        Children =
                        {
                            new TextBlock { Text = "Nombre de Usuario: " + usuarioSeleccionado.NombreUsuario },
                            new TextBox { Text = usuarioSeleccionado.Contrasena, PlaceholderText = "Nueva Contraseña" },
                            new TextBox { Text = usuarioSeleccionado.Nombre, PlaceholderText = "Nuevo Nombre Completo" },
                            new ComboBox
                            {
                                PlaceholderText = "Nueva Especialidad",
                                SelectedItem = new ComboBoxItem { Content = usuarioSeleccionado.Especialidad }
                            }
                        }
                    },
                    PrimaryButtonText = "Guardar",
                    SecondaryButtonText = "Cancelar"
                };

                var result = await dialog.ShowAsync();

                if (result == ContentDialogResult.Primary)
                {
                    // Lógica para guardar las modificaciones
                    var nuevaContrasena = (dialog.Content as StackPanel).Children[1] as TextBox;
                    var nuevoNombre = (dialog.Content as StackPanel).Children[2] as TextBox;
                    var nuevaEspecialidad = (dialog.Content as StackPanel).Children[3] as ComboBox;

                    // Aquí puedes implementar la lógica para modificar el usuario en DataService
                    usuarioSeleccionado.Contrasena = nuevaContrasena.Text;
                    usuarioSeleccionado.Nombre = nuevoNombre.Text;
                    usuarioSeleccionado.Especialidad = (nuevaEspecialidad.SelectedItem as ComboBoxItem)?.Content.ToString();

                    DataService.ModificarUsuario(usuarioSeleccionado.NombreUsuario, usuarioSeleccionado);
                    MostrarMensajeExito("Usuario modificado exitosamente.");
                }
            }
        }

        private void MostrarMensajeError(string mensaje)
        {
            var dialog = new ContentDialog
            {
                Title = "Error",
                Content = mensaje,
                CloseButtonText = "Aceptar"
            };
            dialog.ShowAsync();
        }

        private void MostrarMensajeExito(string mensaje)
        {
            var dialog = new ContentDialog
            {
                Title = "Éxito",
                Content = mensaje,
                CloseButtonText = "Aceptar"
            };
            dialog.ShowAsync();
        }
    }
}