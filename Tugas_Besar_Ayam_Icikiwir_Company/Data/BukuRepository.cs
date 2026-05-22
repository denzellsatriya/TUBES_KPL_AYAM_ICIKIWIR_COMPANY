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
        private readonly string _filePath;

        // Gunakan AppDomain.CurrentDomain.BaseDirectory agar path selalu
        // mengarah ke folder executable (bin/Debug/net8.0/), bukan working dir
        public BukuRepository()
        {
            _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Buku.json");
            LoadData();
        }

        private static JsonSerializerOptions BuatOptions() => new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            // Agar "Status": 0 (integer) bisa di-deserialize ke enum StatusBuku
            Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter() },
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString
        };

        private void LoadData()
        {
            if (!File.Exists(_filePath))
            {
                Console.WriteLine($"[INFO] File tidak ditemukan: {_filePath}");
                return;
            }
            try
            {
                string jsonString = File.ReadAllText(_filePath);
                // Buku.json menyimpan Status sebagai integer (0,1,2).
                // JsonSerializer standar tidak otomatis konversi int -> enum,
                // sehingga kita parse manual lewat JsonDocument lalu mapping.
                var rawList = JsonSerializer.Deserialize<List<System.Text.Json.JsonElement>>(jsonString) ?? new();
                _daftarBuku = new List<T>();
                foreach (var el in rawList)
                {
                    var item = new T();
                    if (el.TryGetProperty("Id", out var eId)) item.Id = eId.GetInt32();
                    if (el.TryGetProperty("Judul", out var eJudul)) item.Judul = eJudul.GetString() ?? "";
                    if (el.TryGetProperty("Status", out var eStatus))
                        item.Status = (StatusBuku)eStatus.GetInt32();
                    if (el.TryGetProperty("TanggalDibuat", out var eTgl))
                        item.TanggalDibuat = eTgl.GetDateTime();
                    if (el.TryGetProperty("TanggalPinjam", out var ePinjam) &&
                        ePinjam.ValueKind != System.Text.Json.JsonValueKind.Null)
                        item.TanggalPinjam = ePinjam.GetDateTime();
                    item.Logs = new List<LogEntry>();
                    _daftarBuku.Add(item);
                }

                // Fallback untuk data lama yang belum punya TanggalDibuat
                foreach (var i in _daftarBuku.Where(x => x.TanggalDibuat == default))
                    i.TanggalDibuat = new DateTime(2025, 1, 1);

                Console.WriteLine($"[INFO] {_daftarBuku.Count} buku berhasil dimuat.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR] Gagal membaca Buku.json: {ex.Message}");
                _daftarBuku = new List<T>();
            }
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
