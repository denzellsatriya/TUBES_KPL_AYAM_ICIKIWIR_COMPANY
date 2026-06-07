using API_Perpustakaan.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Tugas_Besar_Ayam_Icikiwir_Company.Models;

namespace API_Perpustakaan.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BukuController : ControllerBase
    {
        private readonly string jsonFilePath = "buku.json";
        private static readonly JsonSerializerOptions jsonOptions = new JsonSerializerOptions { WriteIndented = true };

        // 1. GET: Ambil Semua Buku
        [HttpGet]
        public ActionResult<List<Buku>> Get()
        {
            if (!System.IO.File.Exists(jsonFilePath))
            {
                return Ok(new List<Buku>());
            }

            var jsonText = System.IO.File.ReadAllText(jsonFilePath);
            var daftarBuku = JsonSerializer.Deserialize<List<Buku>>(jsonText) ?? new List<Buku>();

            return Ok(daftarBuku);
        }

        // 2. GET by ID: Ambil 1 Buku Berdasarkan ID
        [HttpGet("{id}")]
        public ActionResult<Buku> GetById(int id)
        {
            if (!System.IO.File.Exists(jsonFilePath))
            {
                return NotFound(new { message = "Data buku masih kosong." });
            }

            var jsonText = System.IO.File.ReadAllText(jsonFilePath);
            var daftarBuku = JsonSerializer.Deserialize<List<Buku>>(jsonText) ?? new List<Buku>();

            var buku = daftarBuku.FirstOrDefault(b => b.Id == id);

            if (buku == null)
            {
                return NotFound(new { message = $"Buku dengan ID {id} tidak ditemukan." });
            }

            return Ok(buku);
        }

        // 3. POST: Tambah Buku Baru 
        [HttpPost]
        public ActionResult Post([FromBody] BukuCreateDTO inputBaru)
        {
            var daftarBuku = new List<Buku>();

            if (System.IO.File.Exists(jsonFilePath))
            {
                var jsonText = System.IO.File.ReadAllText(jsonFilePath);
                daftarBuku = JsonSerializer.Deserialize<List<Buku>>(jsonText) ?? new List<Buku>();
            }

            int newId = 1;
            if (daftarBuku.Count > 0)
            {
                newId = daftarBuku.Max(b => b.Id) + 1;
            }

            Buku bukuBaru = new Buku
            {
                Id = newId,
                Judul = inputBaru.Judul,
                TanggalDibuat = inputBaru.TanggalDibuat,
                Status = StatusBuku.TERSEDIA,
                TanggalPinjam = null,
                Logs = []
            };

            daftarBuku.Add(bukuBaru);

            string newJsonText = JsonSerializer.Serialize(daftarBuku, jsonOptions);
            System.IO.File.WriteAllText(jsonFilePath, newJsonText);

            return Ok(bukuBaru);
        }

        // 4. PUT: Edit Buku Berdasarkan ID
        [HttpPut("{id}")]
        public ActionResult Put(int id, [FromBody] Buku inputUpdate)
        {
            if (!System.IO.File.Exists(jsonFilePath))
            {
                return NotFound(new { message = "Data buku masih kosong, tidak ada yang bisa di-update." });
            }

            var jsonText = System.IO.File.ReadAllText(jsonFilePath);
            var daftarBuku = JsonSerializer.Deserialize<List<Buku>>(jsonText) ?? new List<Buku>();

            var bukuLama = daftarBuku.FirstOrDefault(b => b.Id == id);
            if (bukuLama == null)
            {
                return NotFound(new { message = $"Buku dengan ID {id} tidak ditemukan." });
            }

            // Timpa data lama dengan data baru 
            bukuLama.Judul = inputUpdate.Judul;
            bukuLama.Status = inputUpdate.Status;
            bukuLama.TanggalPinjam = inputUpdate.TanggalPinjam;

            string newJsonText = JsonSerializer.Serialize(daftarBuku, jsonOptions);
            System.IO.File.WriteAllText(jsonFilePath, newJsonText);

            return Ok(bukuLama);
        }

        // 5. DELETE: Hapus Buku Berdasarkan ID
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            if (!System.IO.File.Exists(jsonFilePath))
            {
                return NotFound(new { message = "Data buku masih kosong, tidak ada yang bisa dihapus." });
            }

            var jsonText = System.IO.File.ReadAllText(jsonFilePath);
            var daftarBuku = JsonSerializer.Deserialize<List<Buku>>(jsonText) ?? new List<Buku>();

            var buku = daftarBuku.FirstOrDefault(b => b.Id == id);
            if (buku == null)
            {
                return NotFound(new { message = $"Buku dengan ID {id} tidak ditemukan." });
            }

            daftarBuku.Remove(buku);

            string newJsonText = JsonSerializer.Serialize(daftarBuku, jsonOptions);
            System.IO.File.WriteAllText(jsonFilePath, newJsonText);

            return Ok(new { message = $"Buku dengan ID {id} beserta judul '{buku.Judul}' berhasil dihapus." });
        }
    }
}