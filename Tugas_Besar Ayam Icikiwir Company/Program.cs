using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Tugas_Besar_Ayam_Icikiwir.Models;
using Tugas_Besar_Ayam_Icikiwir.Data;
using Tugas_Besar_Ayam_Icikiwir.Logic;

namespace Tugas_Besar_Ayam_Icikiwir
{
    class Program
    {
        static LibrarySettings libSettings = new LibrarySettings();
        static List<UserAccount> staffList = new List<UserAccount>();

        static void Main(string[] args)
        {
            LoadConfiguration();
            BukuRepository<Buku> repo = new BukuRepository<Buku>();
            UserAccount userAktif;

            Console.WriteLine("APLIKASI PERPUSTAKAAN");
            Console.WriteLine("1. Staff (Login)\n2. Pengunjung (Registrasi)");
            Console.Write("Pilihan: ");
            userAktif = (Console.ReadLine() == "1") ? LoginStaff() : RegistrasiPengunjung();

            JalankanMenuUtama(userAktif, repo);
        }

        static void LoadConfiguration()
        {
            try
            {
                string path = "userconfig.json";
                if (File.Exists(path))
                {
                    string jsonString = File.ReadAllText(path);
                    using var jsonDoc = JsonDocument.Parse(jsonString);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    //runtime config
                    libSettings = JsonSerializer.Deserialize<LibrarySettings>(jsonDoc.RootElement.GetProperty("Settings").ToString(), options)!;
                    staffList = JsonSerializer.Deserialize<List<UserAccount>>(jsonDoc.RootElement.GetProperty("StaffAccounts").ToString(), options)!;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR!]: Gagal memuat JSON ({ex.Message}), menggunakan akun default.");
            }

            if (staffList == null || staffList.Count == 0)
            {
                staffList = new List<UserAccount>
                {
                    new UserAccount { Username = "admin", Password = "admin", Role = "Staff", Nama = "Staff Perpus" }
                };
            }
        }

        static UserAccount LoginStaff()
        {
            while (true)
            {
                Console.WriteLine($"\nJumlah akun staff terdaftar: {staffList.Count}");
                Console.Write("Username: "); string? u = Console.ReadLine()?.Trim();
                Console.Write("Password: "); string? p = Console.ReadLine()?.Trim();

                var staff = staffList.FirstOrDefault(s =>
                    s.Username.Equals(u, StringComparison.OrdinalIgnoreCase) && s.Password == p);

                if (staff != null) return staff;

                Console.WriteLine("Login Gagal! Pastikan username/password benar.");
                if (staffList.Count > 0)
                    Console.WriteLine($"Petunjuk: Coba pakai username '{staffList[0].Username}'");
            }
        }

        static UserAccount RegistrasiPengunjung()
        {
            UserAccount p = new UserAccount { Role = "Pengunjung" };
            while (true)
            {
                try
                {
                    Console.Write("\nNama Lengkap: "); p.Nama = Console.ReadLine() ?? "";
                    Console.Write("Nomor Identitas: "); p.NomorIdentitas = Console.ReadLine() ?? "";
                    Console.Write("Email: "); p.Email = Console.ReadLine() ?? "";
                    p.ValidasiDataPengunjung();
                    return p;
                }
                catch (Exception ex) { Console.WriteLine($"[ERROR]: {ex.Message}"); }
            }
        }

        static void JalankanMenuUtama(UserAccount user, BukuRepository<Buku> repo)
        {
            while (true)
            {
                Console.WriteLine($"\n=== MENU {user.Role.ToUpper()} ({user.Nama}{user.Username}) ===");
                Console.WriteLine("1. Lihat Semua Buku\n2. Cari Buku");
                if (user.Role == "Pengunjung")
                {
                    Console.WriteLine("3. Pinjam Buku\n4. Kembali/Lapor Hilang");
                }
                else
                {
                    Console.WriteLine("3. Tambah Buku\n4. Restock Buku Hilang");
                }
                Console.WriteLine("0. Keluar");
                Console.Write("Pilih: ");
                string? input = Console.ReadLine();

                if (input == "0") break;
                switch (input)
                {
                    case "1": 
                        repo.GetAll().ForEach(b => Console.WriteLine($"{b.Id}. {b.Judul} [{b.Status}]")); 
                        break;
                    case "2":
                        Console.Write("Keyword: "); string key = Console.ReadLine() ?? "";
                        repo.Cari(b => b.Judul.Contains(key, StringComparison.OrdinalIgnoreCase)).ForEach(b => Console.WriteLine($"{b.Id}. {b.Judul}"));
                        break;
                    case "3":
                        if (user.Role == "Pengunjung") TransaksiPinjam(repo);
                        else TambahBukuBaru(repo);
                        break;
                    case "4":
                        if (user.Role == "Pengunjung") TransaksiKembali(repo);
                        else TransaksiRestock(repo);
                        break;
                }
            }
        }

        static void TransaksiPinjam(BukuRepository<Buku> repo)
        {
            Console.Write("ID Buku: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var b = repo.GetById(id);
                if (b != null && b.Status == StatusBuku.TERSEDIA)
                {
                    b.Status = PerpustakaanLogic.Transisi(b.Status, "PINJAM");
                    b.TanggalPinjam = DateTime.Now;
                    repo.SimpanData();
                    Console.WriteLine("Berhasil Pinjam!");
                }
                else Console.WriteLine("Buku tidak tersedia.");
            }
        }

        static void TransaksiKembali(BukuRepository<Buku> repo)
        {
            Console.Write("ID Buku: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var b = repo.GetById(id);
                if (b != null && b.Status == StatusBuku.DIPINJAM)
                {
                    Console.WriteLine("1. Kembali\n2. Hilang");
                    if (Console.ReadLine() == "2")
                    {
                        b.Status = PerpustakaanLogic.Transisi(b.Status, "LAPOR_HILANG");
                        Console.WriteLine($"Denda: Rp{libSettings.DendaBukuHilang:N0}");
                    }
                    else
                    {
                        int telat = (int)Math.Ceiling((DateTime.Now - b.TanggalPinjam!.Value.AddDays(libSettings.DurasiPinjamHari)).TotalDays);
                        if (telat > 0) Console.WriteLine($"Denda Telat: Rp{telat * libSettings.DendaPerHari:N0}");
                        b.Status = PerpustakaanLogic.Transisi(b.Status, "KEMBALIKAN");
                    }
                    b.TanggalPinjam = null;
                    repo.SimpanData();
                }
            }
        }

        static void TambahBukuBaru(BukuRepository<Buku> repo)
        {
            Console.Write("Judul Buku Baru: ");
            string judul = Console.ReadLine() ?? "Tanpa Judul";
            repo.TambahBuku(new Buku { Judul = judul, Status = StatusBuku.TERSEDIA });
            Console.WriteLine("Buku berhasil ditambahkan.");
        }

        static void TransaksiRestock(BukuRepository<Buku> repo)
        {
            Console.Write("ID Buku Hilang: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var b = repo.AmbilSatu(x => x.Id == id && x.Status == StatusBuku.HILANG);
                if (b != null)
                {
                    b.Status = PerpustakaanLogic.Transisi(b.Status, "RESTOCK");
                    repo.SimpanData();
                    Console.WriteLine("Status buku kembali TERSEDIA.");
                }
            }
        }
    }
}