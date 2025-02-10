using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace PrototipoFinal.Models
{
    public static class AuditLogger
    {
        private static readonly string auditFileName = "audit_log.txt";

        public static async Task LogEvent(string usuario, string accion, string detalles)
        {
            try
            {
                var auditEvent = new AuditEvent
                {
                    Timestamp = DateTime.Now,
                    Usuario = usuario,
                    Accion = accion,
                    Detalles = detalles
                };

                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await localFolder.CreateFileAsync(auditFileName,
                    CreationCollisionOption.OpenIfExists);

                string logEntry = $"{auditEvent.Timestamp:yyyy-MM-dd HH:mm:ss}|{auditEvent.Usuario}|{auditEvent.Accion}|{auditEvent.Detalles}\n";
                await FileIO.AppendTextAsync(file, logEntry);
            }
            catch (Exception ex)
            {
                // Manejar el error según tus necesidades
                Debug.WriteLine($"Error al registrar auditoría: {ex.Message}");
            }
        }

        public static async Task<List<AuditEvent>> GetAuditEvents()
        {
            try
            {
                StorageFolder localFolder = ApplicationData.Current.LocalFolder;
                StorageFile file = await localFolder.GetFileAsync(auditFileName);
                string content = await FileIO.ReadTextAsync(file);

                var events = new List<AuditEvent>();
                foreach (string line in content.Split('\n', StringSplitOptions.RemoveEmptyEntries))
                {
                    string[] parts = line.Split('|');
                    if (parts.Length == 4)
                    {
                        events.Add(new AuditEvent
                        {
                            Timestamp = DateTime.Parse(parts[0]),
                            Usuario = parts[1],
                            Accion = parts[2],
                            Detalles = parts[3]
                        });
                    }
                }

                return events.OrderByDescending(e => e.Timestamp).ToList();
            }
            catch (Exception)
            {
                return new List<AuditEvent>();
            }
        }
    }
}