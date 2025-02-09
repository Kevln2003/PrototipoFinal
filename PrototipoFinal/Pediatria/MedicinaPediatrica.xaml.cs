using System;
using PrototipoFinal.MedicinaDeportiva;
using PrototipoFinal.Plantilla;
using Windows.UI.Xaml.Controls;
using PrototipoFinal.Plantilla;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace PrototipoFinal.Pediatria
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MedicinaPediatrica : Page
    {
        public MedicinaPediatrica()
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
                    ContentFrame.Navigate(typeof(OpcionesPediatria));
                }
                else if (pageType == "Agendamiento")
                {
                    ContentFrame.Navigate(typeof(Agendamiento));
                }
                else if (pageType == "Diagnostico")
                {
                    ContentFrame.Navigate(typeof(Plantilla.RecetaMedica));
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
