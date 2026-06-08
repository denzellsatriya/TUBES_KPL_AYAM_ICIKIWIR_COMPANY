using Tugas_Besar_Ayam_Icikiwir_Company.Models;

namespace Tugas_Besar_Ayam_Icikiwir_Company.Logic
{
    public static class PerpustakaanLogic
    {
        private static readonly Dictionary<(StatusBuku CurrentStatus, string Action), StatusBuku> TransitionTable = new()
        {
            { (StatusBuku.TERSEDIA, "PINJAM"), StatusBuku.DIPINJAM },
            { (StatusBuku.DIPINJAM, "KEMBALIKAN"), StatusBuku.TERSEDIA },
            { (StatusBuku.DIPINJAM, "LAPOR_HILANG"), StatusBuku.HILANG },
            { (StatusBuku.HILANG, "RESTOCK"), StatusBuku.TERSEDIA }
        };

        public static StatusBuku Transisi(StatusBuku currentStatus, string action)
        {
            if (TransitionTable.TryGetValue((currentStatus, action), out StatusBuku nextStatus))
            {
                return nextStatus;
            }

            return currentStatus;
        }
    }
}