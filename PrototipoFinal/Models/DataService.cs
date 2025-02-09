using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using PrototipoFinal.Models;
using Windows.Storage;

namespace PrototipoFinal
{
    public static class DataService
    {
        private static ApplicationDataContainer usuarios = ApplicationData.Current.LocalSettings;
        private static ApplicationDataContainer localSettings = ApplicationData.Current.LocalSettings;
        private const string USUARIOS_KEY = "usuarios_lista";
        static DataService()
            {

            }
        public static List<Usuario> ObtenerUsuarios()
        {
            var jsonString = (string)localSettings.Values[USUARIOS_KEY];
            if (string.IsNullOrEmpty(jsonString))
            {
                // Crear usuarios iniciales si no existen
                var usuariosIniciales = new List<Usuario>
                {
                    new Usuario(
                        usuario: "Bella",
                        contrasena: "2022",
                        nombre: "Administrador Principal",
                        especialidad: "Administración"
                    ),
                    new Usuario(
                        usuario: "Luna",
                        contrasena: "2022",
                        nombre: "Medico Pediatra",
                        especialidad: "Pediatría"
                    ),
                    new Usuario(
                        usuario: "Gringo",
                        contrasena: "2023",
                        nombre: "Medico Deportivo",
                        especialidad: "Medicina Deportiva"
                    )
                };
                GuardarUsuarios(usuariosIniciales);
                return usuariosIniciales;
            }

            return JsonConvert.DeserializeObject<List<Usuario>>(jsonString);
        }

        public static void GuardarUsuarios(List<Usuario> usuarios)
        {
            string jsonString = JsonConvert.SerializeObject(usuarios);
            localSettings.Values[USUARIOS_KEY] = jsonString;
        }

        public static void AgregarUsuario(Usuario nuevoUsuario)
        {
            var usuarios = ObtenerUsuarios();
            usuarios.Add(nuevoUsuario);
            GuardarUsuarios(usuarios);
        }

        public static bool ModificarUsuario(string nombreUsuario, Usuario usuarioModificado)
        {
            var usuarios = ObtenerUsuarios();
            var usuarioExistente = usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

            if (usuarioExistente != null)
            {
                int index = usuarios.IndexOf(usuarioExistente);
                usuarios[index] = usuarioModificado;
                GuardarUsuarios(usuarios);
                return true;
            }
            return false;
        }

        public static bool EliminarUsuario(string nombreUsuario)
        {
            var usuarios = ObtenerUsuarios();
            var usuario = usuarios.FirstOrDefault(u => u.NombreUsuario == nombreUsuario);

            if (usuario != null)
            {
                usuarios.Remove(usuario);
                GuardarUsuarios(usuarios);
                return true;
            }
            return false;
        }
    }
    }