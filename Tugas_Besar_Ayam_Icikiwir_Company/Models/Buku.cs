namespace Tugas_Besar_Ayam_Icikiwir_Company.Models
{
    public enum StatusBuku { TERSEDIA, DIPINJAM, HILANG }

    public class Buku : ITanggalDibuat, ILoggable
    {
        public int Id { get; set; }
        public string Judul { get; set; } = "";
        public StatusBuku Status { get; set; }
        public DateTime? TanggalPinjam { get; set; }
        public DateTime TanggalDibuat { get; set; }
        public List<LogEntry> Logs { get; set; } = new List<LogEntry>();
    }

    public class LibrarySettings
    {
        public int DurasiPinjamHari { get; set; }
        public int DendaPerHari { get; set; }
        public int DendaBukuHilang { get; set; }
        public string NamaPerpustakaan { get; set; } = "";
    }
}
