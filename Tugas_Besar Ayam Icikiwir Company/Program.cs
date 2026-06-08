using System.Text.Json;
using Tugas_Besar_Ayam_Icikiwir_Company.Models;
using Tugas_Besar_Ayam_Icikiwir_Company.Data;
using Tugas_Besar_Ayam_Icikiwir_Company.Logic;

namespace Tugas_Besar_Ayam_Icikiwir_Company
{

    class Program
    {
        static LibrarySettings libSettings = new LibrarySettings();

        //  GENERIC REPOSITORY USER DAN BUKU
        static Repository<UserAccount> userRepo = new Repository<UserAccount>("user.json");
        static Repository<Buku> bukuRepo = new Repository<Buku>("Buku.json");

        static LogRepository<Buku> logBuku = new LogRepository<Buku>("LogBuku.json");
        static LogRepository<UserAccount> logUser = new LogRepository<UserAccount>("LogUser.json");

        static void TampilkanRiwayatSatuBuku(Buku buku)
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LogBuku.json");
            if (!File.Exists(path))
            {
                Console.WriteLine($"\n--- Riwayat Buku: {buku.Judul} ---\n  (Belum ada riwayat)");
                return;
            }

            try
            {
                var logs = JsonSerializer.Deserialize<List<LogEntry>>(
                    File.ReadAllText(path),
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                    ?? new List<LogEntry>();

                var logBukuIni = logs
                    .Where(l => l.Keterangan.Contains($"'{buku.Judul}'", StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(l => l.Waktu)
                    .ToList();

                Console.WriteLine($"\n--- Riwayat Buku: {buku.Judul} ---");
                if (logBukuIni.Count == 0)
                {
                    Console.WriteLine("  (Belum ada riwayat untuk buku ini)");
                    return;
                }

                foreach (var log in logBukuIni)
                {
                    Console.WriteLine($"  [{log.Waktu:dd/MM/yyyy HH:mm}] {log.Aksi,-15} oleh: {log.OlehSiapa,-20} | {log.Keterangan}");
                }
            }
            catch
            {
                Console.WriteLine("[ERROR]: Gagal membaca riwayat buku dari LogBuku.json.");
            }
        }

        static void HomeMenu()
        {
            LoadConfiguration();
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
                    Environment.Exit(0);
                }
            }

            JalankanMenuUtama(userAktif, bukuRepo);
        }
        static void Main(string[] args)
        {
            HomeMenu();
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

            // Menggunakan method AmbilSatu dari Generic Repository
            var user = userRepo.AmbilSatu(x =>
                x.Username.Equals(u, StringComparison.OrdinalIgnoreCase) &&
                x.Password == p);

            if (user != null)
            {
                logUser.TambahLog(user, "LOGIN", user.Nama, $"Login sebagai {user.Role}");
                Console.WriteLine($"\n[+] Login Berhasil! Selamat datang, {user.Nama} ({user.Role}).");
                return user;
            }

            Console.WriteLine("[-] Login Gagal! Username atau password salah. Jika belum punya akun, silakan Register.");
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

                // Validasi data kembar 
                var cekUsername = userRepo.AmbilSatu(x => x.Username.Equals(username, StringComparison.OrdinalIgnoreCase));
                var cekNik = userRepo.AmbilSatu(x => x.NomorIdentitas == nik);

                if (username.Equals(""))
                {
                    Console.WriteLine("\n[-] Gagal: Username tidak boleh kosong!");
                    return;
                }
                if (!email.Contains("@"))
                {
                    Console.WriteLine("\n[-] Gagal: Format email tidak valid!");
                    return;
                }
                if (nik.Length < 5 || !nik.All(char.IsDigit))
                {
                    Console.WriteLine("\n[-] Gagal: NIK harus berupa angka dan minimal 5 digit!");
                    return;
                }
                if (cekUsername != null)
                {
                    Console.WriteLine("\n[-] Gagal: Username sudah digunakan!");
                    return;
                }
                if (cekNik != null)
                {
                    Console.WriteLine("\n[-] Gagal: NIK sudah terdaftar!");
                    return;
                }

                var newUser = new UserAccount
                {
                    Username = username,
                    Password = password,
                    Nama = username,
                    Email = email,
                    NomorIdentitas = nik,
                    Role = "Pengunjung",
                    TanggalDibuat = DateTime.Now
                };

                // Menyimpan ke JSON menggunakan method Tambah
                userRepo.Tambah(newUser);

                logUser.TambahLog(newUser, "REGISTRASI", newUser.Nama, $"Pengunjung baru terdaftar | Email: {newUser.Email}");
                Console.WriteLine($"\n[+] Registrasi berhasil! Akun dibuat pada {newUser.TanggalDibuat:dd/MM/yyyy HH:mm}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERROR]: {ex.Message}");
            }
        }

        static void JalankanMenuUtama(UserAccount user, Repository<Buku> repo)
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

                if (input == "0") HomeMenu();

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
                                var buku = repo.AmbilSatu(b => b.Id == bid);
                                if (buku != null)
                                {
                                    TampilkanRiwayatSatuBuku(buku);
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

        static void TransaksiPinjam(Repository<Buku> repo, UserAccount user)
        {
            Console.Write("ID Buku: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var b = repo.AmbilSatu(x => x.Id == id);
                if (b != null && b.Status == StatusBuku.TERSEDIA)
                {
                    b.Status = PerpustakaanLogic.Transisi(b.Status, "PINJAM");
                    b.TanggalPinjam = DateTime.Now;
                    repo.SimpanData();

                    logBuku.TambahLog(b, "PINJAM", user.Nama, $"Buku '{b.Judul}' dipinjam oleh {user.Nama}");
                    logUser.TambahLog(user, "PINJAM_BUKU", user.Nama, $"Meminjam buku '{b.Judul}' (ID: {b.Id})");

                    Console.WriteLine("Berhasil Pinjam!");
                }
                else Console.WriteLine("Buku tidak tersedia atau ID tidak valid.");
            }
        }

        static void TransaksiKembali(Repository<Buku> repo, UserAccount user)
        {
            Console.Write("ID Buku: ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var b = repo.AmbilSatu(x => x.Id == id);
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

        static void TambahBukuBaru(Repository<Buku> repo, UserAccount user)
        {
            Console.Write("Judul Buku Baru: ");
            string judul = Console.ReadLine() ?? "Tanpa Judul";

            int newId = repo.GetAll().Count > 0 ? repo.GetAll().Max(b => b.Id) + 1 : 1;

            var bukuBaru = new Buku
            {
                Id = newId,
                Judul = judul,
                Status = StatusBuku.TERSEDIA,
                TanggalDibuat = DateTime.Now
            };

            repo.Tambah(bukuBaru);

            logBuku.TambahLog(bukuBaru, "DITAMBAHKAN", user.Nama, $"Buku '{bukuBaru.Judul}' ditambahkan");
            Console.WriteLine($"Buku '{bukuBaru.Judul}' (ID: {newId}) berhasil ditambahkan pada {bukuBaru.TanggalDibuat:dd/MM/yyyy HH:mm}.");
        }

        static void TransaksiRestock(Repository<Buku> repo, UserAccount user)
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
                else Console.WriteLine("Gagal: Pastikan ID benar dan status buku saat ini adalah HILANG.");
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