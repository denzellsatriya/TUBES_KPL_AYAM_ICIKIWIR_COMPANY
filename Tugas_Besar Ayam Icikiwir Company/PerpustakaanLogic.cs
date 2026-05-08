using Tugas_Besar_Ayam_Icikiwir.Models;

namespace Tugas_Besar_Ayam_Icikiwir.Logic
{
    public static class PerpustakaanLogic
    {
        public static StatusBuku Transisi(StatusBuku current, string action)
        {
            return (current, action) switch
            {
                (StatusBuku.TERSEDIA, "PINJAM") => StatusBuku.DIPINJAM,
                (StatusBuku.DIPINJAM, "KEMBALIKAN") => StatusBuku.TERSEDIA,
                (StatusBuku.DIPINJAM, "LAPOR_HILANG") => StatusBuku.HILANG,
                (StatusBuku.HILANG, "RESTOCK") => StatusBuku.TERSEDIA,
                _ => current
            };
        }
    }
}