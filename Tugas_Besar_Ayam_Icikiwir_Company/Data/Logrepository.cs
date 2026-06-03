using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Tugas_Besar_Ayam_Icikiwir.Models;

namespace Tugas_Besar_Ayam_Icikiwir.Data
{
    public class LogRepository<T> where T : ILoggable
    {
        private readonly string _filePath;

        public LogRepository(string filePath)
        {
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
        }

        public void TambahLog(T entitas, string aksi, string olehSiapa, string keterangan = "")
        {
            entitas.Logs ??= new List<LogEntry>();
            entitas.Logs.Add(new LogEntry
            {
                Waktu = DateTime.Now,
                Aksi = aksi,
                OlehSiapa = olehSiapa,
                Keterangan = keterangan
            });
            SimpanLog(entitas.Logs);
        }

        private void SimpanLog(List<LogEntry> logs)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_filePath, JsonSerializer.Serialize(logs, options));
        }

        public void MuatLog(T entitas)
        {
            entitas.Logs = new List<LogEntry>();
            if (!File.Exists(_filePath)) return;
            try
            {
                string json = File.ReadAllText(_filePath);
                entitas.Logs = JsonSerializer.Deserialize<List<LogEntry>>(json,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<LogEntry>();
            }
            catch { entitas.Logs = new List<LogEntry>(); }
        }

        public static void TampilkanLog(T entitas, string judulLog)
        {
            Console.WriteLine($"\n--- {judulLog} ---");
            if (entitas.Logs == null || entitas.Logs.Count == 0)
            {
                Console.WriteLine("  (Belum ada riwayat)");
                return;
            }
            foreach (var log in entitas.Logs.OrderByDescending(l => l.Waktu))
            {
                string baris = $"  [{log.Waktu:dd/MM/yyyy HH:mm}] {log.Aksi,-15} oleh: {log.OlehSiapa}";
                if (!string.IsNullOrWhiteSpace(log.Keterangan))
                    baris += $" | {log.Keterangan}";
                Console.WriteLine(baris);
            }
        }
    }
}
