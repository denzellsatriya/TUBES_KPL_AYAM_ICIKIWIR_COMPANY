using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Tugas_Besar_Ayam_Icikiwir.Models;

namespace Tugas_Besar_Ayam_Icikiwir.Data
{
    //paraterization class -> supaya class bisa bekerja dengan tipe data apapun
    public class BukuRepository<T> where T : Buku, new()
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
            }
            catch { _daftarBuku = new List<T>(); }
        }

        public void SimpanData()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(_filePath, JsonSerializer.Serialize(_daftarBuku, options));
        }
        
        //parametrization method
        public List<T> GetAll() => _daftarBuku;
        public List<T> Cari(Func<T, bool> kriteria) => _daftarBuku.Where(kriteria).ToList();
        public T? AmbilSatu(Func<T, bool> kriteria) => _daftarBuku.FirstOrDefault(kriteria);

        public void TambahBuku(T baru)
        {
            baru.Id = _daftarBuku.Count > 0 ? _daftarBuku.Max(b => b.Id) + 1 : 1;
            _daftarBuku.Add(baru);
            SimpanData();
        }
        public T? GetById(int id)
        {
            return AmbilSatu(b => b.Id == id);
        }
    }
}