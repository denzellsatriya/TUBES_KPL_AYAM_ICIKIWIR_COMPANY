using System;
using System.Collections.Generic;
using System.Linq;

namespace Tugas_Besar_Ayam_Icikiwir.Models
{
    public class UserAccount : ITanggalDibuat, ILoggable
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string NomorIdentitas { get; set; } = "";
        public string Nama { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "Pengunjung";

        public DateTime TanggalDibuat { get; set; }
        public List<LogEntry> Logs { get; set; } = new List<LogEntry>();

        public void ValidasiDataPengunjung()
        {
            if (string.IsNullOrWhiteSpace(Nama))
                throw new ArgumentException("Nama harus diisi!");

            if (NomorIdentitas.Length < 5 || !NomorIdentitas.All(char.IsDigit))
                throw new ArgumentException("Nomor Identitas harus berupa angka minimal 5 digit!");

            if (!Email.Contains("@"))
                throw new ArgumentException("Format email tidak valid!");
        }
    }
}
