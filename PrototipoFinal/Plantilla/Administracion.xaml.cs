using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using PrototipoFinal.Models;
using System.Linq;
using Windows.UI.Xaml.Media;
using TheArtOfDev.HtmlRenderer.Adapters;
using System.Collections.ObjectModel;

namespace PrototipoFinal.Plantilla
{
    public sealed partial class Administracion : Page
    {
        private ObservableCollection<AuditEvent> _auditEvents;

        public Administracion()
        {
            this.InitializeComponent();
            _auditEvents = new ObservableCollection<AuditEvent>();
            AuditListView.ItemsSource = _auditEvents;
            LoadAuditEvents();
        }
        private async void LoadAuditEvents()
        {
            try
            {
                var events = await AuditLogger.GetAuditEvents();
                _auditEvents.Clear();
                foreach (var auditEvent in events)
                {
                    _auditEvents.Add(auditEvent);
                }
            }
            catch (Exception ex)
            {
                MostrarMensajeError("No se pudieron cargar los eventos de auditoría.");
            }
        }
        private async void RegistrarEventoAuditoria(string usuario, string accion, string detalles)
        {
            await AuditLogger.LogEvent(usuario, accion, detalles);
            LoadAuditEvents(); // Recargar la lista después de agregar un nuevo evento
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
            // Registrar el evento de auditoría
            RegistrarEventoAuditoria("Admin", "Crear Usuario",
                $"Se creó el usuario {nuevoUsuario} con especialidad {nuevaEspecialidad}");

            DataService.AgregarUsuario(nuevoUsuarioObj);
            MostrarMensajeExito("Usuario creado exitosamente.");
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
                            new TextBox { Text = usuarioSeleccionado.Nombre, PlaceholderText = "Nuevo Nombre" },
                            // Eliminar la opción de modificar especialidad
                            new TextBlock { Text = "Especialidad: " + usuarioSeleccionado.Especialidad, Foreground = new SolidColorBrush(Windows.UI.Colors.Gray) }
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

                    // Aquí puedes implementar la lógica para modificar el usuario en DataService
                    usuarioSeleccionado.Contrasena = nuevaContrasena.Text;
                    usuarioSeleccionado.Nombre = nuevoNombre.Text;
                    // No se modifica la especialidad

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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void AuditListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }
    }
}