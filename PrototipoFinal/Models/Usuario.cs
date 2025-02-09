using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Windows.Storage;
using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace PrototipoFinal.Models
{

        public class Usuario
        {
            public string NombreUsuario { get; set; }
            public string Contrasena { get; set; }
            public string Nombre { get; set; }
            public string Especialidad { get; set; }


            public Usuario(string usuario, string contrasena, string nombre, string especialidad)
            {
                NombreUsuario = usuario;
                Contrasena = contrasena;
                Nombre = nombre;
                Especialidad = especialidad;
            }
        }

    
}
