using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238
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
            // Crear usuario
            string nuevoUsuario = NuevoUsuarioTextBox.Text;
            string nuevaContrasena = NuevaContrasenaPasswordBox.Password;
            string nuevoNombre = NuevoNombreTextBox.Text;
            string nuevaEspecialidad = (NuevaEspecialidadComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrWhiteSpace(nuevoUsuario) || string.IsNullOrWhiteSpace(nuevaContrasena) || string.IsNullOrWhiteSpace(nuevoNombre) || string.IsNullOrWhiteSpace(nuevaEspecialidad))
            {
                MostrarMensajeError("Por favor complete todos los campos.");
                return;
            }

            var nuevoUsuarioObj = new Usuario(nuevoUsuario, nuevaContrasena, nuevoNombre, nuevaEspecialidad);
            DataService.AgregarUsuario(nuevoUsuarioObj);
            MostrarMensajeExito("Usuario creado exitosamente.");
        }

        private void BuscarUsuarioButton_Click(object sender, RoutedEventArgs e)
        {
            // Buscar usuario
            string usuarioModificar = UsuarioModificarTextBox.Text;

            if (string.IsNullOrWhiteSpace(usuarioModificar))
            {
                MostrarMensajeError("Por favor ingrese el nombre de usuario a modificar.");
                return;
            }

            var usuario = DataService.ObtenerUsuarios().FirstOrDefault(u => u.NombreUsuario == usuarioModificar);
            if (usuario != null)
            {
                // Rellenar campos con datos del usuario
                NuevaContrasenaModificarTextBox.Text = usuario.Contrasena;
                NuevoNombreModificarTextBox.Text = usuario.Nombre;
                NuevaEspecialidadModificarComboBox.SelectedItem = NuevaEspecialidadModificarComboBox.Items
                    .OfType<ComboBoxItem>()
                    .FirstOrDefault(item => item.Content.ToString() == usuario.Especialidad);
            }
            else
            {
                MostrarMensajeError("Usuario no encontrado.");
            }
        }

        private void ModificarUsuarioButton_Click(object sender, RoutedEventArgs e)
        {
            // Modificar usuario
            string usuarioModificar = UsuarioModificarTextBox.Text;
            string nuevaContrasena = NuevaContrasenaModificarTextBox.Text;
            string nuevoNombre = NuevoNombreModificarTextBox.Text;
            string nuevaEspecialidad = (NuevaEspecialidadModificarComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (string.IsNullOrWhiteSpace(usuarioModificar) || string.IsNullOrWhiteSpace(nuevaContrasena) || string.IsNullOrWhiteSpace(nuevoNombre) || string.IsNullOrWhiteSpace(nuevaEspecialidad))
            {
                MostrarMensajeError("Por favor complete todos los campos.");
                return;
            }

            var usuarioModificado = new Usuario(usuarioModificar, nuevaContrasena, nuevoNombre, nuevaEspecialidad);
            if (DataService.ModificarUsuario(usuarioModificar, usuarioModificado))
            {
                MostrarMensajeExito("Usuario modificado exitosamente.");
            }
            else
            {
                MostrarMensajeError("Usuario no encontrado.");
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }
    }
}