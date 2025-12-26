using Microsoft.EntityFrameworkCore;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.Repository
{
    public class CategoryRepository : IRepository<Category>
    {
        private readonly DatabaseContext _db;
        private bool _disposed;

        public CategoryRepository()
        {
            _db = new DatabaseContext();
        }

        public CategoryRepository(DatabaseContext context)
        {
            _db = context;
        }

        public async Task AddAsync(Category entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _db.Categories.AddAsync(entity);
        }

        public void Update(Category entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.Categories.Update(entity);
        }

        public void Remove(Category entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.Categories.Remove(entity);
        }

        public async Task<Category?> GetByIdAsync(long id)
        {
            return await _db.Categories.FindAsync(id);
        }

        public async Task<List<Category>> GetAllAsync()
        {
            return await _db.Categories.ToListAsync();
        }

        public async Task<List<Category>> GetAllOrderedByNameAsync()
        {
            return await _db.Categories
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Category?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await _db.Categories
                .FirstOrDefaultAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> NameExistsAsync(string name, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (excludeId.HasValue)
            {
                return await _db.Categories
                    .AnyAsync(c => c.Name.ToLower() == name.ToLower() && c.Id != excludeId.Value);
            }

            return await _db.Categories
                .AnyAsync(c => c.Name.ToLower() == name.ToLower());
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _db.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }
    }
}

