namespace API_Perpustakaan.Models // Sesuaikan dengan namespace proyek Anda
{
    public class Repository<T> where T : class
    {
        // Contoh implementasi sederhana
        private readonly List<T> _data = new();

        public void Add(T entity) => _data.Add(entity);
        public IEnumerable<T> GetAll() => _data;
        // Tambahkan method lain sesuai kebutuhan
    }
}