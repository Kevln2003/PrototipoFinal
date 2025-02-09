using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using PrototipoFinal.Adminitracion;
using PrototipoFinal.MedicinaDeportiva;
using PrototipoFinal.Models;
using PrototipoFinal.Pediatria;
using PrototipoFinal.Plantilla;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace PrototipoFinal
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private Usuario usuarioActual;
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string especialidadSeleccionada = (EspecialidadComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string usuario = UsuarioTextBox.Text;
            string contrasena = ContrasenaPasswordBox.Password;

            // Obtener la lista de usuarios desde DataService
            var usuarios = DataService.ObtenerUsuarios();
            var usuarioValidado = usuarios.FirstOrDefault(u => u.NombreUsuario == usuario && u.Contrasena == contrasena);

            // Validación de credenciales
            if (usuarioValidado != null)
            {
                // Verificar si la especialidad seleccionada coincide
                if (especialidadSeleccionada == usuarioValidado.Especialidad)
                {
                    // Navegar a la página correspondiente según la especialidad
                    switch (especialidadSeleccionada)
                    {
                        case "Medicina Deportiva":
                            Frame.Navigate(typeof(MedicinaDeportiva.MedicinaDeportiva));
                            break;
                        case "Pediatría":
                            Frame.Navigate(typeof(MedicinaPediatrica));
                            break;
                        case "Administración":
                            Frame.Navigate(typeof(Plantilla.Administracion));
                            break;
                        default:
                            MostrarMensajeError("Especialidad no reconocida.");
                            break;
                    }
                }
                else
                {
                    MostrarMensajeError($"El usuario {usuario} no pertenece a la especialidad seleccionada.");
                }
            }
            else
            {
                MostrarMensajeError("Usuario o contraseña incorrectos.");
            }
        }
        private void MostrarMensajeError(string mensaje)
        {
            var dialog = new ContentDialog
            {
                Title = "Error de inicio de sesión",
                Content = mensaje,
                CloseButtonText = "Aceptar"
            };
            dialog.ShowAsync();
        }

        private void Control_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            // Verifica si la tecla presionada es Enter
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                // Encuentra el siguiente elemento en el orden de tabulación
                var control = sender as Control;
                control?.Focus(FocusState.Keyboard);

                var next = FocusManager.FindNextFocusableElement(FocusNavigationDirection.Next) as Control;
                next?.Focus(FocusState.Keyboard);
            }
        }


    }
}
