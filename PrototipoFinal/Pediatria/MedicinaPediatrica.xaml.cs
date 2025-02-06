using System;
using System.Text.RegularExpressions;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using System.Collections.Generic;
using PrototipoFinal.Models;
namespace PrototipoFinal.Pediatria
{
    public sealed partial class MedicinaPediatrica : Page
    {
        public MedicinaPediatrica()
        {
            this.InitializeComponent();
            dpFechaNacimiento.DateChanged += DpFechaNacimiento_DateChanged;
        }

        private void DpFechaNacimiento_DateChanged(object sender, DatePickerValueChangedEventArgs args)
        {
            DateTime fechaNacimiento = args.NewDate.DateTime;
            int edad = DateTime.Today.Year - fechaNacimiento.Year;

            // Mostrar tutor legal si es menor de 18 años
            pnlTutorLegal.Visibility = edad < 18 ? Visibility.Visible : Visibility.Collapsed;
        }

        private async void BtnGuardar_Click(object sender, RoutedEventArgs e)
        {
            // Obtener valores de los campos
            string nombre = txtNombre.Text.Trim();
            string apellido = txtApellido.Text.Trim();
            string cedula = txtCedula.Text.Trim();
            DateTime fechaNacimiento = dpFechaNacimiento.Date.DateTime;
            string tipoSangre = (cbTipoSangre.SelectedItem as ComboBoxItem)?.Content.ToString();
            bool discapacidad = cbDiscapacidad.IsChecked ?? false;
            string contacto = txtContacto.Text.Trim();
            string email = txtEmail.Text.Trim();
            string tutorNombre = txtTutorNombre.Text.Trim();
            string tutorContacto = txtTutorContacto.Text.Trim();

            // Lista para almacenar los mensajes de error
            List<string> mensajesError = new List<string>();

            // Limpiar resaltados y mensajes previos
            LimpiarResaltados();

            // Validaciones
            if (string.IsNullOrWhiteSpace(nombre))
            {
                mensajesError.Add("Nombre obligatorio");
                ResaltarCampo(txtNombre, "Este campo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(apellido))
            {
                mensajesError.Add("Apellido obligatorio");
                ResaltarCampo(txtApellido, "Este campo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(cedula))
            {
                mensajesError.Add("Cédula obligatoria");
                ResaltarCampo(txtCedula, "Este campo es obligatorio.");
            }
            //Validacion de cedula
            else if(!CedulaValidator.ValidarCedula(cedula))
            {
                mensajesError.Add("Cédula Inválida");
                ResaltarCampo(txtCedula, "La cédula no es válida");
            }
            

            if (fechaNacimiento == null)
            {
                mensajesError.Add("Fecha de Nacimiento obligatoria");
                ResaltarCampo(dpFechaNacimiento, "Este campo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(tipoSangre))
            {
                mensajesError.Add("Tipo de sangre obligatorio");
                ResaltarCampo(cbTipoSangre, "Este campo es obligatorio.");
            }

            if (string.IsNullOrWhiteSpace(contacto))
            {
                mensajesError.Add("Número de contacto obligatorio");
                ResaltarCampo(txtContacto, "Este campo es obligatorio.");
            }

            if (mensajesError.Count > 0)
            {
                return; // No continuar si hay errores
            }

            // Validaciones de formatos
            if (!Regex.IsMatch(nombre, @"^[a-zA-Z\s]+$"))
            {
                await MostrarMensaje("El nombre solo puede contener letras.");
                return;
            }

            if (!Regex.IsMatch(apellido, @"^[a-zA-Z\s]+$"))
            {
                await MostrarMensaje("El apellido solo puede contener letras.");
                return;
            }

            if (!Regex.IsMatch(cedula, @"^\d+$"))
            {
                await MostrarMensaje("La cédula solo debe contener números.");
                return;
            }

            if (!Regex.IsMatch(contacto, @"^\d+$"))
            {
                await MostrarMensaje("El número de contacto solo debe contener números.");
                return;
            }

            // Validaciones del tutor legal
            if (pnlTutorLegal.Visibility == Visibility.Visible)
            {
                if (string.IsNullOrWhiteSpace(tutorNombre) || string.IsNullOrWhiteSpace(tutorContacto))
                {
                    await MostrarMensaje("Los datos del tutor son obligatorios para menores de edad.");
                    return;
                }

                if (!Regex.IsMatch(tutorNombre, @"^[a-zA-Z\s]+$"))
                {
                    await MostrarMensaje("El nombre del tutor solo puede contener letras.");
                    return;
                }

                if (!Regex.IsMatch(tutorContacto, @"^\d+$"))
                {
                    await MostrarMensaje("El número de contacto del tutor solo debe contener números.");
                    return;
                }
            }

            // Mostrar mensaje con los datos guardados
            string mensaje = $"Paciente guardado:\nNombre: {nombre} {apellido}\nCédula: {cedula}\nFecha de Nacimiento: {fechaNacimiento.Date.ToShortDateString()}\nTipo de Sangre: {tipoSangre}\nDiscapacidad: {discapacidad}\nContacto: {contacto}\nEmail: {email}";

            if (pnlTutorLegal.Visibility == Visibility.Visible)
            {
                mensaje += $"\nTutor Legal: {tutorNombre}\nContacto del Tutor: {tutorContacto}";
            }

            var dialog = new ContentDialog
            {
                Title = "Paciente Guardado",
                Content = mensaje,
                CloseButtonText = "Aceptar"
            };

            await dialog.ShowAsync();
        }

        private async System.Threading.Tasks.Task MostrarMensaje(string mensaje)
        {
            var dialog = new MessageDialog(mensaje);
            await dialog.ShowAsync();
        }

        private void ResaltarCampo(Control campo, string mensajeError)
        {
            // Resaltar el campo y mostrar el mensaje de error
            if (campo is TextBox textBox)
            {
                textBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                textBox.BorderThickness = new Windows.UI.Xaml.Thickness(2);

                MostrarMensajeError(textBox, mensajeError);
            }
            else if (campo is ComboBox comboBox)
            {
                comboBox.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                comboBox.BorderThickness = new Windows.UI.Xaml.Thickness(2);

                MostrarMensajeError(comboBox, mensajeError);
            }
            else if (campo is DatePicker datePicker)
            {
                datePicker.BorderBrush = new SolidColorBrush(Windows.UI.Colors.Red);
                datePicker.BorderThickness = new Windows.UI.Xaml.Thickness(2);

                MostrarMensajeError(datePicker, mensajeError);
            }
        }

        private void MostrarMensajeError(Control campo, string mensajeError)
        {
            // Obtener el contenedor del campo (debe ser un StackPanel o Grid)
            var contenedor = campo.Parent as Panel;
            if (contenedor != null)
            {
                // Crear un texto de error
                TextBlock errorMessage = new TextBlock
                {
                    Name = "errorMessage", // Asignamos un nombre para poder eliminarlo luego
                    Text = mensajeError,
                    Foreground = new SolidColorBrush(Windows.UI.Colors.Red),
                    Margin = new Windows.UI.Xaml.Thickness(0, 5, 0, 0)
                };

                // Añadir el mensaje de error al contenedor del campo
                contenedor.Children.Add(errorMessage);
            }
        }

        private void LimpiarResaltados()
        {
            // Limpiar solo los mensajes de error, pero no los bordes rojos
            EliminarMensajeError(txtNombre);
            EliminarMensajeError(txtApellido);
            EliminarMensajeError(txtCedula);
            EliminarMensajeError(dpFechaNacimiento);
            EliminarMensajeError(cbTipoSangre);
            EliminarMensajeError(txtContacto);
        }

        private void EliminarMensajeError(Control campo)
        {
            // Obtener el contenedor del campo (debe ser un StackPanel o Grid)
            var contenedor = campo.Parent as Panel;
            if (contenedor != null)
            {
                // Buscar el TextBlock de error dentro del contenedor
                foreach (var child in contenedor.Children)
                {
                    if (child is TextBlock errorMessage && errorMessage.Name == "errorMessage")
                    {
                        // Eliminar el TextBlock de error
                        contenedor.Children.Remove(errorMessage);
                        break;
                    }
                }
            }
        }
    }
}

