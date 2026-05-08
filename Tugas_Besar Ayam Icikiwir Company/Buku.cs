using System;

namespace Tugas_Besar_Ayam_Icikiwir.Models
{
    public enum StatusBuku { TERSEDIA, DIPINJAM, HILANG }

    public class Buku
    {
        public int Id { get; set; }
        public string Judul { get; set; } = "";
        public StatusBuku Status { get; set; }
        public DateTime? TanggalPinjam { get; set; }
    }

    public class LibrarySettings
    {
        public int DurasiPinjamHari { get; set; }
        public int DendaPerHari { get; set; }
        public int DendaBukuHilang { get; set; }
        public string NamaPerpustakaan { get; set; } = "";
    }
}