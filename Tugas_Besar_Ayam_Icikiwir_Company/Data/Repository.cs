using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace Tugas_Besar_Ayam_Icikiwir_Company.Data
{
    public class Repository<T> where T : class
    {
        private readonly string filePath;
        private List<T> dataList;

        public Repository(string fileName)
        {
            filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
            LoadData();
        }

        private void LoadData()
        {
            if (!File.Exists(filePath))
            {
                dataList = new List<T>();
                SimpanData();
            }
            else
            {
                string json = File.ReadAllText(filePath);
                dataList = string.IsNullOrWhiteSpace(json) ? new List<T>() :
                           JsonSerializer.Deserialize<List<T>>(json) ?? new List<T>();
            }
        }

        public void SimpanData()
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(dataList, options));
        }

        public List<T> GetAll()
        {
            return dataList;
        }

        public void Tambah(T item)
        {
            dataList.Add(item);
            SimpanData();
        }

        public T AmbilSatu(Func<T, bool> predicate)
        {
            return dataList.FirstOrDefault(predicate);
        }

        public List<T> Cari(Func<T, bool> predicate)
        {
            return dataList.Where(predicate).ToList();
        }
    }
}