using System;
using System.Collections.Generic;

namespace Tugas_Besar_Ayam_Icikiwir.Models
{
    public interface ILoggable
    {
        List<LogEntry> Logs { get; set; }
    }

    public class LogEntry
    {
        public DateTime Waktu { get; set; }
        public string Aksi { get; set; } = "";
        public string OlehSiapa { get; set; } = "";
        public string Keterangan { get; set; } = "";
    }
}
