using Microsoft.EntityFrameworkCore;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.Repository
{
    public class IngredientRepository : IRepository<Ingredient>
    {
        private readonly DatabaseContext _db;
        private bool _disposed;

        public IngredientRepository()
        {
            _db = new DatabaseContext();
        }

        public IngredientRepository(DatabaseContext context)
        {
            _db = context;
        }

        public async Task AddAsync(Ingredient entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _db.Ingredients.AddAsync(entity);
        }

        public void Update(Ingredient entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.Ingredients.Update(entity);
        }

        public void Remove(Ingredient entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.Ingredients.Remove(entity);
        }

        public async Task<Ingredient?> GetByIdAsync(long id)
        {
            return await _db.Ingredients.FindAsync(id);
        }

        public async Task<List<Ingredient>> GetAllAsync()
        {
            return await _db.Ingredients.ToListAsync();
        }

        public async Task<List<Ingredient>> GetAllOrderedByNameAsync()
        {
            return await _db.Ingredients
                .OrderBy(i => i.Name)
                .ToListAsync();
        }

        public async Task<Ingredient?> GetByNameAsync(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return null;

            return await _db.Ingredients
                .FirstOrDefaultAsync(i => i.Name.ToLower() == name.ToLower());
        }

        public async Task<bool> NameExistsAsync(string name, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (excludeId.HasValue)
            {
                return await _db.Ingredients
                    .AnyAsync(i => i.Name.ToLower() == name.ToLower() && i.Id != excludeId.Value);
            }

            return await _db.Ingredients
                .AnyAsync(i => i.Name.ToLower() == name.ToLower());
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

