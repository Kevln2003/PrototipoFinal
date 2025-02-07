using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using PrototipoFinal.Adminitracion;
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

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PrototipoFinal.MedicinaDeportiva
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MedicinaDeportiva : Page
    {
        public MedicinaDeportiva()
        {
            this.InitializeComponent();
            Modulos.NavigationRequested += OnNavigationRequested;
        }
        private void OnNavigationRequested(object sender, string pageType)
        {
            try
            {
                if (pageType == "Formulario")
                {
                    ContentFrame.Navigate(typeof(OpcionesMedicinaDeportiva));
                }
                else if (pageType == "Agendamiento")
                {
                    ContentFrame.Navigate(typeof(Agendamiento));
                }
                else if (pageType == "Administracion")
                {
                    ContentFrame.Navigate(typeof(Administracion));
                }
                else if (pageType == "Historial")
                {
                    ContentFrame.Navigate(typeof(Buscar));
                }

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error durante la navegación: {ex.Message}");
            }


        }
    }
}
