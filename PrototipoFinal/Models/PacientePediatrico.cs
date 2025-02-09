using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PrototipoFinal.Models
{
    public class PacientePediatrico
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        // Información del Paciente
        [JsonProperty("nombrePaciente")]
        public string NombrePaciente { get; set; }        // Información del Paciente

        [JsonProperty("cedulaDelPaciente")]
        public string CedulaDelPaciente { get; set; }

        [JsonProperty("fechaNacimiento")]
        public DateTime FechaNacimiento { get; set; }

        // Información del Representante
        [JsonProperty("nombreRepresentante")]
        public string NombreRepresentante { get; set; }

        [JsonProperty("cedulaDelRepresentante")]
        public string CedulaDelRepresentante { get; set; }

        [JsonProperty("correoRepresentante")]
        public string CorreoRepresentante { get; set; }

        [JsonProperty("celularRepresentante")]
        public string CelularRepresentante { get; set; }

        // Control de Vacunas
        [JsonProperty("vacunas")]
        public List<Vacuna> Vacunas { get; set; } = new List<Vacuna>();

        // Historial de Consultas
        [JsonProperty("consultas")]
        public List<ConsultaPediatrica> Consultas { get; set; } = new List<ConsultaPediatrica>();

        [JsonProperty("fechaRegistro")]
        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }

    public class Vacuna
    {
        [JsonProperty("nombre")]
        public string Nombre { get; set; }

        [JsonProperty("edadRecomendada")]
        public string EdadRecomendada { get; set; }

        [JsonProperty("fechaAplicacion")]
        public DateTime? FechaAplicacion { get; set; }

        [JsonProperty("aplicada")]
        public bool Aplicada { get; set; }
    }

    public class ConsultaPediatrica
    {
        [JsonProperty("id")]
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [JsonProperty("fechaConsulta")]
        public DateTime FechaConsulta { get; set; }

        // Signos Vitales
        [JsonProperty("temperatura")]
        public decimal Temperatura { get; set; }

        [JsonProperty("frecuenciaCardiaca")]
        public int FrecuenciaCardiaca { get; set; }

        [JsonProperty("frecuenciaRespiratoria")]
        public int FrecuenciaRespiratoria { get; set; }

        [JsonProperty("saturacionOxigeno")]
        public int SaturacionOxigeno { get; set; }

        // Medidas Antropométricas
        [JsonProperty("peso")]
        public decimal Peso { get; set; }

        [JsonProperty("talla")]
        public decimal Talla { get; set; }

        [JsonProperty("perimetroCefalico")]
        public decimal PerimetroCefalico { get; set; }

        [JsonProperty("imc")]
        public decimal IMC { get; set; }

        // Examen Físico
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

        // Información General
        [JsonProperty("motivoConsulta")]
        public string MotivoConsulta { get; set; }

        [JsonProperty("observaciones")]
        public string Observaciones { get; set; }
    }
}