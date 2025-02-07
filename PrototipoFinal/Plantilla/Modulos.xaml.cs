using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class Modulos : Page
    {
        public event EventHandler<string> NavigationRequested;

        public Modulos()
        {
            this.InitializeComponent();
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        { //Al login
            var frame = Window.Current.Content as Frame;
            if (frame != null)
            {
                frame.Navigate(typeof(MainPage));
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            // Ingreso
            NavigationRequested?.Invoke(this, "Formulario");
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            //Historia Clinica
            NavigationRequested?.Invoke(this, "Historial");
        }
        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            //Diagnostico
            NavigationRequested?.Invoke(this, "Diagnostico");
        }
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            //Facturacion
            NavigationRequested?.Invoke(this, "Facturacion");
        }
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            //Agendamiento
            NavigationRequested?.Invoke(this, "Agendamiento");
        }
        private void Button_Click_6(object sender, RoutedEventArgs e)
        {
            //Aminitracion
            NavigationRequested?.Invoke(this, "Administracion");
        }
        private void Button_Click_7(object sender, RoutedEventArgs e)
        {
            
        }
    }
}
