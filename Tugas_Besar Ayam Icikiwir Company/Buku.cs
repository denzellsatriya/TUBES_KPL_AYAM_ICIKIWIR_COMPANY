namespace Tugas_Besar_Ayam_Icikiwir.Models
{
    public enum StatusBuku
    {
        TERSEDIA,
        DIPINJAM,
        HILANG
    }

    public class Buku
    {
        public int Id { get; set; }
        public string Judul { get; set; } = string.Empty;
        public StatusBuku Status { get; set; }
    }
}