using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Tugas_Besar_Ayam_Icikiwir.Models;

namespace Tugas_Besar_Ayam_Icikiwir.Data
{
    // Code reuse via constraint generic ganda:
    // - "where T : Buku"            -> T pasti punya Id, Judul, Status, dll
    // - "where T : ITanggalDibuat"  -> T pasti punya TanggalDibuat
    // Sehingga TambahBuku() otomatis set TanggalDibuat untuk semua tipe T
    // tanpa duplikasi logika di subclass manapun
    public class BukuRepository<T> where T : Buku, ITanggalDibuat, new()
    {
        private List<T> _daftarBuku = new List<T>();
        private string _filePath = "Buku.json";

        public BukuRepository() { LoadData(); }

        private void LoadData()
        {
            if (!File.Exists(_filePath)) return;
            try
            {
                string jsonString = File.ReadAllText(_filePath);
                _daftarBuku = JsonSerializer.Deserialize<List<T>>(jsonString,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new List<T>();

                // Fallback untuk data lama yang belum punya TanggalDibuat (nilai default 0001-01-01)
                // Code reuse: logika fallback ini berlaku untuk semua T via ITanggalDibuat
                foreach (var item in _daftarBuku.Where(x => x.TanggalDibuat == default))
                    item.TanggalDibuat = new DateTime(2025, 1, 1);
            }
            catch { _daftarBuku = new List<T>(); }
        }

        public void SimpanData()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_daftarBuku, options));
        }

        // parametrization method
        public List<T> GetAll() => _daftarBuku;
        public List<T> Cari(Func<T, bool> kriteria) => _daftarBuku.Where(kriteria).ToList();
        public T? AmbilSatu(Func<T, bool> kriteria) => _daftarBuku.FirstOrDefault(kriteria);

        public void TambahBuku(T baru)
        {
            baru.Id = _daftarBuku.Count > 0 ? _daftarBuku.Max(b => b.Id) + 1 : 1;
            // Otomatis diisi karena T dijamin implement ITanggalDibuat (code reuse)
            // Tidak perlu tulis logika ini di subclass atau caller manapun
            baru.TanggalDibuat = DateTime.Now;
            _daftarBuku.Add(baru);
            SimpanData();
        }

        public T? GetById(int id) => AmbilSatu(b => b.Id == id);
    }
}