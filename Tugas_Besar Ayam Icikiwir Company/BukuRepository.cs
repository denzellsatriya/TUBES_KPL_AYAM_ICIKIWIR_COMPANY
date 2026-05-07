using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Tugas_Besar_Ayam_Icikiwir.Models;

namespace Tugas_Besar_Ayam_Icikiwir.Data
{
    // parametrization class ('where T : Buku, new()' untuk memastikan T adalah turunan Buku)
    public class BukuRepository<T> where T : Buku, new()
    {
        private List<T> _daftarBuku = new List<T>();
        private string _filePath = "Buku.json";

        public BukuRepository()
        {
            LoadData();
        }

        private void LoadData()
        {
            try
            {
                if (File.Exists(_filePath))
                {
                    string jsonString = File.ReadAllText(_filePath);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    _daftarBuku = JsonSerializer.Deserialize<List<T>>(jsonString, options) ?? new List<T>();
                }
                else
                {
                    Console.WriteLine("Peringatan: File Buku.json tidak ditemukan!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }

        // parameterization method 
        public List<T> Cari(Func<T, bool> kriteria)
        {
            return _daftarBuku.Where(kriteria).ToList();
        }

        // mengambil satu data
        public T? AmbilSatu(Func<T, bool> kriteria)
        {
            return _daftarBuku.FirstOrDefault(kriteria);
        }
        
        public List<T> GetAll() => _daftarBuku;

        public List<T> GetAvailable() => Cari(b => b.Status == StatusBuku.TERSEDIA);

        public T? GetById(int id) => AmbilSatu(b => b.Id == id);
    }
}