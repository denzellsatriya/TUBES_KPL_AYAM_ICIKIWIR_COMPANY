namespace Tugas_Besar_Ayam_Icikiwir_Company.Models
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

    }
}
