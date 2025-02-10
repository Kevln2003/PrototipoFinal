using System;
using Newtonsoft.Json;

namespace PrototipoFinal.Models
{
    public class PacientePediatrico
    {
        public class Vacuna
        {
            public string Nombre { get; set; }
            public string EdadRecomendada { get; set; }
        }

        // Identificación
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Datos básicos del paciente
        [JsonProperty("nombrePaciente")]
        public string NombrePaciente { get; set; }

        [JsonProperty("cedulaPaciente")]
        public string CedulaPaciente { get; set; }

        [JsonProperty("fechaNacimiento")]
        public DateTime FechaNacimiento { get; set; }

        // Datos del representante
        [JsonProperty("nombreRepresentante")]
        public string NombreRepresentante { get; set; }

        [JsonProperty("cedulaRepresentante")]
        public string CedulaRepresentante { get; set; }

        [JsonProperty("correoRepresentante")]
        public string CorreoRepresentante { get; set; }

        [JsonProperty("celularRepresentante")]
        public string CelularRepresentante { get; set; }

        // Vacunas
        [JsonProperty("nombreVacuna")]
        public string NombreVacuna { get; set; }

        [JsonProperty("edadRecomendada")]
        public string EdadRecomendada { get; set; }

        [JsonProperty("fechaAplicacion")]
        public DateTime? FechaAplicacion { get; set; }

        [JsonProperty("aplicada")]
        public bool Aplicada { get; set; }

        // Información de consulta médica
        [JsonProperty("fechaConsulta")]
        public DateTime FechaConsulta { get; set; }

        // Signos vitales
        [JsonProperty("temperatura")]
        public decimal Temperatura { get; set; }

        [JsonProperty("frecuenciaCardiaca")]
        public int FrecuenciaCardiaca { get; set; }

        [JsonProperty("frecuenciaRespiratoria")]
        public int FrecuenciaRespiratoria { get; set; }

        [JsonProperty("saturacionOxigeno")]
        public int SaturacionOxigeno { get; set; }

        // Medidas físicas
        [JsonProperty("peso")]
        public decimal Peso { get; set; }

        [JsonProperty("talla")]
        public decimal Talla { get; set; }

        [JsonProperty("perimetroCefalico")]
        public decimal PerimetroCefalico { get; set; }

        [JsonProperty("imc")]
        public decimal IMC { get; set; }

        // Examen físico
        [JsonProperty("pielFaneras")]
        public string PielFaneras { get; set; }

        [JsonProperty("cabezaCuello")]
        public string CabezaCuello { get; set; }

        [JsonProperty("toraxCardiopulmonar")]
        public string ToraxCardiopulmonar { get; set; }

        [JsonProperty("abdomen")]
        public string Abdomen { get; set; }

        [JsonProperty("extremidades")]
        public string Extremidades { get; set; }

        [JsonProperty("examenNeurologico")]
        public string ExamenNeurologico { get; set; }

        // Información general
        [JsonProperty("motivoConsulta")]
        public string MotivoConsulta { get; set; }

        [JsonProperty("observaciones")]
        public string Observaciones { get; set; }
    }
}