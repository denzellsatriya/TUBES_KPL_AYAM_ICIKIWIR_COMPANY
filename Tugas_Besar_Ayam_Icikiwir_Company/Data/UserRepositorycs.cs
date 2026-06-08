using System.Text.Json;
using Tugas_Besar_Ayam_Icikiwir_Company.Models;

namespace Tugas_Besar_Ayam_Icikiwir_Company.Data
{
    public class UserRepository
    {
        private readonly string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "user.json");

        public UserRepository()
        {
            if (!File.Exists(filePath))
            {
                // Buat akun admin default jika file belum ada
                var defaultAdmin = new List<UserAccount>
                {
                    new UserAccount {
                        Username = "admin",
                        Password = "admin",
                        Role = "Staff",
                        Nama = "Staff Perpus",
                        TanggalDibuat = DateTime.Now
                    }
                };
                SaveAllUsers(defaultAdmin);
            }
        }

        public List<UserAccount> GetAllUsers()
        {
            string json = File.ReadAllText(filePath);
            if (string.IsNullOrWhiteSpace(json)) return new List<UserAccount>();
            return JsonSerializer.Deserialize<List<UserAccount>>(json) ?? new List<UserAccount>();
        }

        private void SaveAllUsers(List<UserAccount> users)
        {
            var options = new JsonSerializerOptions { WriteIndented = true };
            File.WriteAllText(filePath, JsonSerializer.Serialize(users, options));
        }

        public UserAccount Login(string username, string password)
        {
            var users = GetAllUsers();
            return users.FirstOrDefault(u =>
                u.Username.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                u.Password == password);
        }

        public bool Register(string username, string password, string email, string nik, out string message, out UserAccount newUser)
        {
            var users = GetAllUsers();
            newUser = null;

            if (string.IsNullOrWhiteSpace(username))
            {
                message = "Gagal: Username tidak boleh kosong!";
                return false;
            }
            if (nik.Length < 5 || !nik.All(char.IsDigit))
            {
                message = "Gagal: NIK harus berupa angka dan minimal 5 digit!";
                return false;
            }
            if (!email.Contains("@"))
            {
                message = "Gagal: Format email tidak valid!";
                return false;
            }
            if (users.Any(u => u.Username.Equals(username, StringComparison.OrdinalIgnoreCase)))
            {
                message = "Gagal: Username sudah digunakan!";
                return false;
            }
            if (users.Any(u => u.NomorIdentitas == nik))
            {
                message = "Gagal: NIK sudah terdaftar!";
                return false;
            }

            newUser = new UserAccount
            {
                Username = username,
                Password = password,
                Nama = username, //  inputan username sebagai Nama
                Email = email,
                NomorIdentitas = nik,
                Role = "Pengunjung",
                TanggalDibuat = DateTime.Now
            };

            users.Add(newUser);
            SaveAllUsers(users);

            message = "Registrasi berhasil! Anda sekarang bisa login.";
            return true;
        }
    }
}