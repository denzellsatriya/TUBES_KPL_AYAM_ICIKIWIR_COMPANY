using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Tugas_Besar_Ayam_Icikiwir.Models;

namespace Tugas_Besar_Ayam_Icikiwir.Data
{
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

        public List<T> GetAll() => _daftarBuku;
        public List<T> GetAvailable() => _daftarBuku.Where(b => b.Status == StatusBuku.TERSEDIA).ToList();
        public T? GetById(int id) => _daftarBuku.FirstOrDefault(b => b.Id == id);
    }
}