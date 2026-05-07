using System;
using Tugas_Besar_Ayam_Icikiwir.Models;
using Tugas_Besar_Ayam_Icikiwir.Data;
using Tugas_Besar_Ayam_Icikiwir.Logic;

namespace Tugas_Besar_Ayam_Icikiwir
{
    class Program
    {
        static void Main(string[] args)
        {
            BukuRepository<Buku> repo = new BukuRepository<Buku>();
            bool running = true;

            while (running)
            {
                Console.WriteLine("\n=== PERPUSTAKAAN AYAM ICIKIWIR ===");
                Console.WriteLine("1. Lihat Semua Buku");
                Console.WriteLine("2. Lihat Buku Tersedia");
                Console.WriteLine("3. Pinjam Buku");
                Console.WriteLine("4. Kembalikan Buku");
                Console.WriteLine("5. Keluar");
                Console.Write("Pilih menu: ");

                string? input = Console.ReadLine();
                switch (input)
                {
                    case "1":
                        repo.GetAll().ForEach(b => Console.WriteLine($"ID: {b.Id} | {b.Judul} | Status: {b.Status}"));
                        break;
                    case "2":
                        var tersedia = repo.Cari(b => b.Status == StatusBuku.TERSEDIA);
                        tersedia.ForEach(b => Console.WriteLine($"ID: {b.Id} | {b.Judul}"));
                        break;
                    case "3":
                        JalankanTransaksi(repo, "PINJAM");
                        break;
                    case "4":
                        JalankanTransaksi(repo, "KEMBALIKAN");
                        break;
                    case "5":
                        running = false;
                        break;
                }
            }
        }

        static void JalankanTransaksi(BukuRepository<Buku> repo, string aksi)
        {
            Console.Write($"Masukkan ID Buku ({aksi}): ");
            if (int.TryParse(Console.ReadLine(), out int id))
            {
                var buku = repo.AmbilSatu(b => b.Id == id);

                if (buku != null)
                {
                    StatusBuku awal = buku.Status;
                    buku.Status = PerpustakaanLogic.Transisi(buku.Status, aksi);

                    if (awal != buku.Status)
                        Console.WriteLine($"Berhasil! {buku.Judul} sekarang {buku.Status}.");
                    else
                        Console.WriteLine("Gagal: Transisi status tidak diperbolehkan.");
                }
                else Console.WriteLine("Buku tidak ditemukan.");
            }
        }
    }
}