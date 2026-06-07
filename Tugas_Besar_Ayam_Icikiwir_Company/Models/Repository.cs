namespace API_Perpustakaan.Models 
{
    public class Repository<T> where T : class
    {
        private readonly List<T> _data = new();

        public void Add(T entity) => _data.Add(entity);
        public IEnumerable<T> GetAll() => _data;
    }
}