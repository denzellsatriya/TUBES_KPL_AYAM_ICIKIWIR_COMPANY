using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Linq;
using Tugas_Besar_Ayam_Icikiwir.Models;
using Tugas_Besar_Ayam_Icikiwir.Data;

namespace Tugas_Besar_Ayam_Icikiwir
{
    class Program
    {
        static LibrarySettings libSettings = new LibrarySettings();
        static List<UserAccount> staffList = new List<UserAccount>();

        static void Main(string[] args)
        {
            LoadConfiguration(); // [RUNTIME CONFIG]

            BukuRepository<Buku> repo = new BukuRepository<Buku>();
            UserAccount userAktif = new UserAccount();

            Console.WriteLine($"=== {libSettings.NamaPerpustakaan.ToUpper()} ===");
            Console.WriteLine("Pilih Peran:");
            Console.WriteLine("1. Staff (Login)");
            Console.WriteLine("2. Pengunjung (Registrasi)");
            Console.Write("Pilihan: ");
            string? pilihan = Console.ReadLine();

            if (pilihan == "1") userAktif = LoginStaff();
            else userAktif = RegistrasiPengunjung();

            JalankanMenuUtama(userAktif, repo);
        }

        static void LoadConfiguration()
        {
            try
            {
                string path = "userconfig.json";
                if (File.Exists(path))
                {
                    var jsonDoc = JsonDocument.Parse(File.ReadAllText(path));
                    libSettings = JsonSerializer.Deserialize<LibrarySettings>(jsonDoc.RootElement.GetProperty("Settings").ToString())!;
                    staffList = JsonSerializer.Deserialize<List<UserAccount>>(jsonDoc.RootElement.GetProperty("StaffAccounts").ToString())!;
                }
            }
            catch
            {
                libSettings = new LibrarySettings { DurasiPinjamHari = 7, DendaPerHari = 5000, DendaBukuHilang = 50000, NamaPerpustakaan = "Perpus Default" };
            }
        }

        static UserAccount LoginStaff()
        {
            while (true)
            {
                Console.Write("\nUsername: "); string? u = Console.ReadLine();
                Console.Write("Password: "); string? p = Console.ReadLine();
                var staff = staffList.FirstOrDefault(s => s.Username == u && s.Password == p);
                if (staff != null) return staff;
                Console.WriteLine("Login Gagal!");
            }
        }

        static UserAccount RegistrasiPengunjung()
        {
            UserAccount p = new UserAccount { Role = "Pengunjung" };
            Console.WriteLine("\n--- Form Pengunjung ---");
            Console.Write("Nama: "); p.Nama = Console.ReadLine() ?? "Guest";
            Console.Write("ID (NIM/NIK): "); p.NomorIdentitas = Console.ReadLine() ?? "-";
            return p;
        }

        static void JalankanMenuUtama(UserAccount user, BukuRepository<Buku> repo)
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine($"\n=== MENU {user.Role.ToUpper()} ({user.Nama}{user.Username}) ===");
                Console.WriteLine("1. Lihat Semua Buku");
                Console.WriteLine("2. Cari Buku");

                if (user.Role == "Pengunjung")
                {
                    Console.WriteLine("3. Pinjam Buku");
                    Console.WriteLine("4. Kembalikan / Lapor Hilang");
                }
                else
                {
                    Console.WriteLine("3. Tambah Buku");
                    Console.WriteLine("4. Restock Buku Hilang");
                }
                Console.WriteLine("0. Keluar");
                Console.Write("Pilih menu: ");
                string? input = Console.ReadLine();

                switch (input)
                {
                    case "1": repo.GetAll().ForEach(b => Console.WriteLine($"{b.Id}. {b.Judul} [{b.Status}]")); break;
                    case "2":
                        Console.Write("Cari: "); string key = Console.ReadLine() ?? "";
                        repo.Cari(b => b.Judul.Contains(key, StringComparison.OrdinalIgnoreCase)).ForEach(b => Console.WriteLine($"{b.Id}. {b.Judul}"));
                        break;
                    case "3":
                        if (user.Role == "Pengunjung") TransaksiPinjam(repo);
                        else Console.WriteLine("Fungsi Tambah Buku...");
                        break;
                    case "4":
                        if (user.Role == "Pengunjung") TransaksiKembali(repo);
                        else TransaksiRestock(repo);
                        break;
                    case "0": running = false; break;
                }
            }
        }

        static void TransaksiPinjam(BukuRepository<Buku> repo)
        {
            Console.Write("ID Buku: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var buku = repo.AmbilSatu(b => b.Id == id);
                if (buku != null && buku.Status == StatusBuku.TERSEDIA)
                {
                    buku.Status = StatusBuku.DIPINJAM;
                    buku.TanggalPinjam = DateTime.Now;
                    Console.WriteLine($"Pinjam Berhasil! Kembali sebelum: {DateTime.Now.AddDays(libSettings.DurasiPinjamHari):dd MMM yyyy}");
                }
                else Console.WriteLine("Buku tidak tersedia.");
            }
        }

        static void TransaksiKembali(BukuRepository<Buku> repo)
        {
            Console.Write("ID Buku: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var buku = repo.AmbilSatu(b => b.Id == id);
                if (buku != null && buku.Status == StatusBuku.DIPINJAM)
                {
                    Console.WriteLine("1. Kembali Normal\n2. Lapor Hilang");
                    string? opsi = Console.ReadLine();
                    if (opsi == "2")
                    {
                        buku.Status = StatusBuku.HILANG;
                        buku.TanggalPinjam = null;
                        Console.WriteLine($"Lapor Hilang Berhasil. Denda: Rp{libSettings.DendaBukuHilang:N0}");
                    }
                    else
                    {
                        int hariTelat = (int)Math.Ceiling((DateTime.Now - buku.TanggalPinjam!.Value.AddDays(libSettings.DurasiPinjamHari)).TotalDays);
                        if (hariTelat > 0) Console.WriteLine($"Telat {hariTelat} Hari. Denda: Rp{hariTelat * libSettings.DendaPerHari:N0}");
                        else Console.WriteLine("Kembali Tepat Waktu.");
                        buku.Status = StatusBuku.TERSEDIA;
                        buku.TanggalPinjam = null;
                    }
                }
            }
        }

        static void TransaksiRestock(BukuRepository<Buku> repo)
        {
            Console.Write("ID Buku Hilang untuk di-Restock: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var buku = repo.AmbilSatu(b => b.Id == id && b.Status == StatusBuku.HILANG);
                if (buku != null)
                {
                    buku.Status = StatusBuku.TERSEDIA;
                    Console.WriteLine("Buku berhasil tersedia kembali.");
                }
                else Console.WriteLine("Buku tidak ditemukan dengan status HILANG.");
            }
        }
    }
}