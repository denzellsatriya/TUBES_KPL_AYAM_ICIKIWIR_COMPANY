using System;
using System.Linq;

namespace Tugas_Besar_Ayam_Icikiwir.Models
{
    //runtime config
    public class UserAccount
    {
        //staff
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";

        //pengunjung
        public string NomorIdentitas { get; set; } = ""; 
        public string Nama { get; set; } = "";
        public string Email { get; set; } = "";
        public string Role { get; set; } = "Pengunjung";

        public void ValidasiDataPengunjung()
        {
            if (string.IsNullOrWhiteSpace(Nama))
                throw new ArgumentException("Nama harus diisi!");

            if (NomorIdentitas.Length < 5 || !NomorIdentitas.All(char.IsDigit))
                throw new ArgumentException("Nomor Identitas (NIM/NIK) harus berupa angka minimal 5 digit!");

            if (!Email.Contains("@"))
                throw new ArgumentException("Format email tidak valid!");
        }
    }
}