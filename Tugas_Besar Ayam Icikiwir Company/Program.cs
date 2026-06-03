
﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Tugas_Besar_Ayam_Icikiwir.Data;
using Tugas_Besar_Ayam_Icikiwir.Logic;
// PERHATIKAN: Pastikan 3 baris di bawah ini sesuai dengan nama folder/namespace model kamu
using Tugas_Besar_Ayam_Icikiwir.Models;
using Tugas_Besar_Ayam_Icikiwir_Company.Data; // Disamakan dengan namespace UserRepository yang baru

namespace Tugas_Besar_Ayam_Icikiwir_Company
{
    class Program
    {
        static LibrarySettings libSettings = new LibrarySettings();

        // Inisialisasi Repository
        static UserRepository userRepo = new UserRepository();
        static LogRepository<Buku> logBuku = new LogRepository<Buku>("LogBuku.json");
        static LogRepository<UserAccount> logUser = new LogRepository<UserAccount>("LogUser.json");

        static void Main(string[] args)
        {
            LoadConfiguration();
            BukuRepository<Buku> repo = new BukuRepository<Buku>();
            UserAccount userAktif = null;

            while (userAktif == null)
            {
                Console.WriteLine("\n=== APLIKASI PERPUSTAKAAN ===");
                Console.WriteLine("1. Login (Staff / Pengunjung)");
                Console.WriteLine("2. Registrasi Pengunjung Baru");
                Console.WriteLine("0. Keluar");
                Console.Write("Pilihan: ");
                string pilihan = Console.ReadLine();

                if (pilihan == "1")
                {
                    userAktif = ProsesLogin();
                }
                else if (pilihan == "2")
                {
                    ProsesRegistrasi();
                }
                else if (pilihan == "0")
                {
                    return;
                }
            }

            JalankanMenuUtama(userAktif, repo);
        }

        static void LoadConfiguration()
        {
            try
            {
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userconfig.json");
                if (File.Exists(path))
                {
                    string jsonString = File.ReadAllText(path);
                    using var jsonDoc = JsonDocument.Parse(jsonString);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    libSettings = JsonSerializer.Deserialize<LibrarySettings>(
                        jsonDoc.RootElement.GetProperty("Settings").ToString(), options)!;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[INFO]: Memuat konfigurasi default ({ex.Message}).");
            }
        }

        static UserAccount ProsesLogin()
        {
            Console.Write("\nUsername: "); string u = Console.ReadLine()?.Trim();
            Console.Write("Password: "); string p = Console.ReadLine()?.Trim();

            var user = userRepo.Login(u, p);

            if (user != null)
            {
                logUser.TambahLog(user, "LOGIN", user.Nama, $"Login sebagai {user.Role}");
                Console.WriteLine($"\n[+] Login Berhasil! Selamat datang, {user.Nama} ({user.Role}).");
                return user;
            }

            Console.WriteLine("[-] Login Gagal! Username atau password salah. Jika belum punya akun, silakan pilih menu Registrasi.");
            return null;
        }

        static void ProsesRegistrasi()
        {
            try
            {
                Console.WriteLine("\n--- FORM REGISTRASI ---");
                Console.Write("Nama (digunakan sbg Username): "); string username = Console.ReadLine() ?? "";
                Console.Write("Password: "); string password = Console.ReadLine() ?? "";
                Console.Write("Email: "); string email = Console.ReadLine() ?? "";
                Console.Write("NIK (Nomor Identitas): "); string nik = Console.ReadLine() ?? "";

                if (userRepo.Register(username, password, email, nik, out string pesan, out UserAccount newUser))
                {
                    logUser.TambahLog(newUser, "REGISTRASI", newUser.Nama, $"Pengunjung baru terdaftar | Email: {newUser.Email}");
                    Console.WriteLine($"\n[+] {pesan} Akun dibuat pada {newUser.TanggalDibuat:dd/MM/yyyy HH:mm}");
                }
                else
                {
                    Console.WriteLine($"\n[-] {pesan}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: {ex.Message}");
            }
        }

        static void JalankanMenuUtama(UserAccount user, BukuRepository<Buku> repo)
        {
            while (true)
            {
                Console.WriteLine($"\n=== MENU {user.Role.ToUpper()} ({user.Nama}) ===");
                Console.WriteLine($"    Akun dibuat: {user.TanggalDibuat:dd/MM/yyyy HH:mm}");
                Console.WriteLine("1. Lihat Semua Buku\n2. Cari Buku");
                if (user.Role == "Pengunjung")
                    Console.WriteLine("3. Pinjam Buku\n4. Kembali/Lapor Hilang\n5. Riwayat Aktivitas Saya");
                else
                    Console.WriteLine("3. Tambah Buku\n4. Restock Buku Hilang\n5. Riwayat Log Buku\n6. Riwayat Log Sistem");
                Console.WriteLine("0. Keluar");
                Console.Write("Pilih: ");
                string? input = Console.ReadLine();

                if (input == "0") break;
                switch (input)
                {
                    case "1":
                        repo.GetAll().ForEach(b => Console.WriteLine(
                            $"{b.Id}. {b.Judul} [{b.Status}] - Ditambahkan: {b.TanggalDibuat:dd/MM/yyyy}"));
                        break;
                    case "2":
                        Console.Write("Keyword: "); string key = Console.ReadLine() ?? "";
                        repo.Cari(b => b.Judul.Contains(key, StringComparison.OrdinalIgnoreCase))
                            .ForEach(b => Console.WriteLine($"{b.Id}. {b.Judul}"));
                        break;
                    case "3":
                        if (user.Role == "Pengunjung") TransaksiPinjam(repo, user);
                        else TambahBukuBaru(repo, user);
                        break;
                    case "4":
                        if (user.Role == "Pengunjung") TransaksiKembali(repo, user);
                        else TransaksiRestock(repo, user);
                        break;
                    case "5":
                        if (user.Role == "Pengunjung")
                        {
                            LogRepository<UserAccount>.TampilkanLog(user, $"Riwayat Aktivitas: {user.Nama}");
                        }
                        else
                        {
                            Console.Write("Masukkan ID Buku: ");
                            if (int.TryParse(Console.ReadLine(), out int bid))
                            {
                                var buku = repo.GetById(bid);
                                if (buku != null)
                                {
                                    logBuku.MuatLog(buku);
                                    LogRepository<Buku>.TampilkanLog(buku, $"Riwayat Buku: {buku.Judul}");
                                }
                                else Console.WriteLine("Buku tidak ditemukan.");
                            }
                        }
                        break;
                    case "6":
                        if (user.Role != "Pengunjung")
                            TampilkanLogSistem();
                        break;
                }
            }
        }

        static void TransaksiPinjam(BukuRepository<Buku> repo, UserAccount user)
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

                    logBuku.TambahLog(b, "PINJAM", user.Nama, $"Buku '{b.Judul}' dipinjam oleh {user.Nama}");
                    logUser.TambahLog(user, "PINJAM_BUKU", user.Nama, $"Meminjam buku '{b.Judul}' (ID: {b.Id})");

                    Console.WriteLine("Berhasil Pinjam!");
                }
                else Console.WriteLine("Buku tidak tersedia.");
            }
        }

        static void TransaksiKembali(BukuRepository<Buku> repo, UserAccount user)
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

                        logBuku.TambahLog(b, "LAPOR_HILANG", user.Nama, $"Buku '{b.Judul}' dilaporkan hilang");
                        logUser.TambahLog(user, "LAPOR_HILANG", user.Nama, $"Melaporkan buku '{b.Judul}' hilang");
                    }
                    else
                    {
                        int telat = (int)Math.Ceiling((DateTime.Now - b.TanggalPinjam!.Value.AddDays(libSettings.DurasiPinjamHari)).TotalDays);
                        string dendaInfo = telat > 0 ? $"Denda: Rp{telat * libSettings.DendaPerHari:N0}" : "Tepat waktu";
                        if (telat > 0) Console.WriteLine($"Denda Telat: Rp{telat * libSettings.DendaPerHari:N0}");
                        b.Status = PerpustakaanLogic.Transisi(b.Status, "KEMBALIKAN");

                        logBuku.TambahLog(b, "KEMBALIKAN", user.Nama, $"Buku '{b.Judul}' dikembalikan | {dendaInfo}");
                        logUser.TambahLog(user, "KEMBALIKAN", user.Nama, $"Mengembalikan buku '{b.Judul}' | {dendaInfo}");
                    }
                    b.TanggalPinjam = null;
                    repo.SimpanData();
                }
            }
        }

        static void TambahBukuBaru(BukuRepository<Buku> repo, UserAccount user)
        {
            Console.Write("Judul Buku Baru: ");
            string judul = Console.ReadLine() ?? "Tanpa Judul";
            var bukuBaru = new Buku { Judul = judul, Status = StatusBuku.TERSEDIA };
            repo.TambahBuku(bukuBaru);

            logBuku.TambahLog(bukuBaru, "DITAMBAHKAN", user.Nama, $"Buku '{bukuBaru.Judul}' ditambahkan");
            Console.WriteLine($"Buku '{bukuBaru.Judul}' berhasil ditambahkan.");
        }

        static void TransaksiRestock(BukuRepository<Buku> repo, UserAccount user)
        {
            Console.Write("ID Buku Hilang: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var b = repo.AmbilSatu(x => x.Id == id && x.Status == StatusBuku.HILANG);
                if (b != null)
                {
                    b.Status = PerpustakaanLogic.Transisi(b.Status, "RESTOCK");
                    repo.SimpanData();

                    logBuku.TambahLog(b, "RESTOCK", user.Nama, $"Buku '{b.Judul}' di-restock");
                    Console.WriteLine("Status buku kembali TERSEDIA.");
                }
            }
        }

        static void TampilkanLogSistem()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogUser.json");
            if (!File.Exists(path))
            {
                Console.WriteLine("\n--- Log Sistem ---\n  (Belum ada riwayat sistem)");
                return;
            }
            try
            {
                var logs = JsonSerializer.Deserialize<List<LogEntry>>(
                    File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<LogEntry>();

                Console.WriteLine("\n--- Riwayat Penggunaan Sistem ---");
                foreach (var log in logs.OrderByDescending(l => l.Waktu))
                {
                    Console.WriteLine($"  [{log.Waktu:dd/MM/yyyy HH:mm}] {log.Aksi,-15} oleh: {log.OlehSiapa,-20} | {log.Keterangan}");
                }
            }
            catch { Console.WriteLine("[ERROR]: Gagal membaca log sistem."); }
        }
    }
}

﻿using System;
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

        static LogRepository<Buku> logBuku = new LogRepository<Buku>("LogBuku.json");
        static LogRepository<UserAccount> logUser = new LogRepository<UserAccount>("LogUser.json");

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
                string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "userconfig.json");
                if (File.Exists(path))
                {
                    string jsonString = File.ReadAllText(path);
                    using var jsonDoc = JsonDocument.Parse(jsonString);
                    var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

                    libSettings = JsonSerializer.Deserialize<LibrarySettings>(
                        jsonDoc.RootElement.GetProperty("Settings").ToString(), options)!;
                    staffList = JsonSerializer.Deserialize<List<UserAccount>>(
                        jsonDoc.RootElement.GetProperty("StaffAccounts").ToString(), options)!;

                    foreach (var s in staffList.Where(x => x.TanggalDibuat == default))
                        s.TanggalDibuat = new DateTime(2025, 1, 1);
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
                    new UserAccount
                    {
                        Username = "admin", Password = "admin", Role = "Staff", Nama = "Staff Perpus",
                        TanggalDibuat = DateTime.Now
                    }
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

                if (staff != null)
                {
                    logUser.TambahLog(staff, "LOGIN", staff.Nama,
                        $"Staff login sebagai {staff.Role}");
                    return staff;
                }

                Console.WriteLine("Login Gagal! Pastikan username/password benar.");
                if (staffList.Count > 0)
                    Console.WriteLine($"Petunjuk: Coba pakai username '{staffList[0].Username}'");
            }
        }

        static UserAccount RegistrasiPengunjung()
        {
            UserAccount p = new UserAccount
            {
                Role = "Pengunjung",
                TanggalDibuat = DateTime.Now
            };
            while (true)
            {
                try
                {
                    Console.Write("\nNama Lengkap: "); p.Nama = Console.ReadLine() ?? "";
                    Console.Write("Nomor Identitas: "); p.NomorIdentitas = Console.ReadLine() ?? "";
                    Console.Write("Email: "); p.Email = Console.ReadLine() ?? "";
                    p.ValidasiDataPengunjung();

                    logUser.TambahLog(p, "REGISTRASI", p.Nama,
                        $"Pengunjung baru terdaftar | Email: {p.Email}");

                    Console.WriteLine($"Akun berhasil dibuat pada {p.TanggalDibuat:dd/MM/yyyy HH:mm}.");
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
                Console.WriteLine($"    Akun dibuat: {user.TanggalDibuat:dd/MM/yyyy HH:mm}");
                Console.WriteLine("1. Lihat Semua Buku\n2. Cari Buku");
                if (user.Role == "Pengunjung")
                    Console.WriteLine("3. Pinjam Buku\n4. Kembali/Lapor Hilang\n5. Riwayat Aktivitas Saya");
                else
                    Console.WriteLine("3. Tambah Buku\n4. Restock Buku Hilang\n5. Riwayat Log Buku\n6. Riwayat Log Sistem");
                Console.WriteLine("0. Keluar");
                Console.Write("Pilih: ");
                string? input = Console.ReadLine();

                if (input == "0") break;
                switch (input)
                {
                    case "1":
                        repo.GetAll().ForEach(b => Console.WriteLine(
                            $"{b.Id}. {b.Judul} [{b.Status}] - Ditambahkan: {b.TanggalDibuat:dd/MM/yyyy}"));
                        break;
                    case "2":
                        Console.Write("Keyword: "); string key = Console.ReadLine() ?? "";
                        repo.Cari(b => b.Judul.Contains(key, StringComparison.OrdinalIgnoreCase))
                            .ForEach(b => Console.WriteLine($"{b.Id}. {b.Judul}"));
                        break;
                    case "3":
                        if (user.Role == "Pengunjung") TransaksiPinjam(repo, user);
                        else TambahBukuBaru(repo, user);
                        break;
                    case "4":
                        if (user.Role == "Pengunjung") TransaksiKembali(repo, user);
                        else TransaksiRestock(repo, user);
                        break;
                    case "5":
                        if (user.Role == "Pengunjung")
                        {
                            LogRepository<UserAccount>.TampilkanLog(user, $"Riwayat Aktivitas: {user.Nama}");
                        }
                        else
                        {
                            Console.Write("Masukkan ID Buku: ");
                            if (int.TryParse(Console.ReadLine(), out int bid))
                            {
                                var buku = repo.GetById(bid);
                                if (buku != null)
                                {
                                    logBuku.MuatLog(buku);
                                    LogRepository<Buku>.TampilkanLog(buku, $"Riwayat Buku: {buku.Judul}");
                                }
                                else Console.WriteLine("Buku tidak ditemukan.");
                            }
                        }
                        break;
                    case "6":
                        if (user.Role != "Pengunjung")
                            TampilkanLogSistem();
                        break;
                }
            }
        }

        static void TransaksiPinjam(BukuRepository<Buku> repo, UserAccount user)
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

                    logBuku.TambahLog(b, "PINJAM", user.Nama,
                        $"Buku '{b.Judul}' dipinjam oleh {user.Nama}");
                    logUser.TambahLog(user, "PINJAM_BUKU", user.Nama,
                        $"Meminjam buku '{b.Judul}' (ID: {b.Id})");

                    Console.WriteLine("Berhasil Pinjam!");
                }
                else Console.WriteLine("Buku tidak tersedia.");
            }
        }

        static void TransaksiKembali(BukuRepository<Buku> repo, UserAccount user)
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

                        logBuku.TambahLog(b, "LAPOR_HILANG", user.Nama,
                            $"Buku '{b.Judul}' dilaporkan hilang oleh {user.Nama}");
                        logUser.TambahLog(user, "LAPOR_HILANG", user.Nama,
                            $"Melaporkan buku '{b.Judul}' (ID: {b.Id}) hilang");
                    }
                    else
                    {
                        int telat = (int)Math.Ceiling((DateTime.Now - b.TanggalPinjam!.Value.AddDays(libSettings.DurasiPinjamHari)).TotalDays);
                        string dendaInfo = telat > 0 ? $"Denda: Rp{telat * libSettings.DendaPerHari:N0}" : "Tepat waktu";
                        if (telat > 0) Console.WriteLine($"Denda Telat: Rp{telat * libSettings.DendaPerHari:N0}");
                        b.Status = PerpustakaanLogic.Transisi(b.Status, "KEMBALIKAN");

                        logBuku.TambahLog(b, "KEMBALIKAN", user.Nama,
                            $"Buku '{b.Judul}' dikembalikan oleh {user.Nama} | {dendaInfo}");
                        logUser.TambahLog(user, "KEMBALIKAN", user.Nama,
                            $"Mengembalikan buku '{b.Judul}' (ID: {b.Id}) | {dendaInfo}");
                    }
                    b.TanggalPinjam = null;
                    repo.SimpanData();
                }
            }
        }

        static void TambahBukuBaru(BukuRepository<Buku> repo, UserAccount user)
        {
            Console.Write("Judul Buku Baru: ");
            string judul = Console.ReadLine() ?? "Tanpa Judul";
            var bukuBaru = new Buku { Judul = judul, Status = StatusBuku.TERSEDIA };
            repo.TambahBuku(bukuBaru);

            logBuku.TambahLog(bukuBaru, "DITAMBAHKAN", user.Nama,
                $"Buku '{bukuBaru.Judul}' ditambahkan ke perpustakaan oleh {user.Nama}");

            Console.WriteLine($"Buku '{bukuBaru.Judul}' berhasil ditambahkan pada {bukuBaru.TanggalDibuat:dd/MM/yyyy HH:mm}.");
        }

        static void TransaksiRestock(BukuRepository<Buku> repo, UserAccount user)
        {
            Console.Write("ID Buku Hilang: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var b = repo.AmbilSatu(x => x.Id == id && x.Status == StatusBuku.HILANG);
                if (b != null)
                {
                    b.Status = PerpustakaanLogic.Transisi(b.Status, "RESTOCK");
                    repo.SimpanData();

                    logBuku.TambahLog(b, "RESTOCK", user.Nama,
                        $"Buku '{b.Judul}' di-restock oleh {user.Nama}");

                    Console.WriteLine("Status buku kembali TERSEDIA.");
                }
            }
        }

        static void TampilkanLogSistem()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogUser.json");
            if (!File.Exists(path))
            {
                Console.WriteLine("\n--- Log Sistem --- \n  (Belum ada riwayat sistem)");
                return;
            }
            try
            {
                var logs = JsonSerializer.Deserialize<List<LogEntry>>(
                    File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<LogEntry>();

                Console.WriteLine("\n--- Riwayat Penggunaan Sistem Perpustakaan ---");
                if (logs.Count == 0) { Console.WriteLine("  (Belum ada riwayat)"); return; }

                foreach (var log in logs.OrderByDescending(l => l.Waktu))
                {
                    Console.WriteLine(
                        $"  [{log.Waktu:dd/MM/yyyy HH:mm}] {log.Aksi,-15} oleh: {log.OlehSiapa,-20} | {log.Keterangan}");
                }
            }
            catch { Console.WriteLine("[ERROR]: Gagal membaca log sistem."); }
        }
    }
}

