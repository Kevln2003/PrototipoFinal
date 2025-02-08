using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace PrototipoFinal.Models
{
    namespace PrototipoFinal.Models
    {
        public class PacienteDeportivo
        {

            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("nombres")]
            public string Nombres { get; set; }

            [JsonProperty("apellidos")]
            public string Apellidos { get; set; }

            [JsonProperty("correo")]
            public string Correo { get; set; }

            [JsonProperty("celular")]
            public string Celular { get; set; }

            [JsonProperty("cedula")]
            public string Cedula { get; set; }

            [JsonProperty("antecedentesFamiliares")]
            public string AntecedentesFamiliares { get; set; }

            [JsonProperty("haPracticadoDeportes")]
            public bool HaPracticadoDeportes { get; set; }

            [JsonProperty("deportesPracticados")]
            public string DeportesPracticados { get; set; }

            [JsonProperty("peso")]
            public double Peso { get; set; }

            [JsonProperty("altura")]
            public double Altura { get; set; }

            [JsonProperty("imc")]
            public double IMC { get; set; }

            [JsonProperty("fechaRegistro")]
            public DateTime FechaRegistro { get; set; }
        }

    }
}
