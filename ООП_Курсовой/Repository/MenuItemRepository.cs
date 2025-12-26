using Microsoft.EntityFrameworkCore;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.Repository
{
    public class MenuItemRepository : IRepository<MenuItem>
    {
        private readonly DatabaseContext _db;
        private bool _disposed;

        public MenuItemRepository()
        {
            _db = new DatabaseContext();
        }

        public MenuItemRepository(DatabaseContext context)
        {
            _db = context;
        }

        public async Task AddAsync(MenuItem entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _db.MenuItems.AddAsync(entity);
        }

        public void Update(MenuItem entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.MenuItems.Update(entity);
        }

        public void Remove(MenuItem entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.MenuItems.Remove(entity);
        }

        public async Task<MenuItem?> GetByIdAsync(long id)
        {
            return await _db.MenuItems.FindAsync(id);
        }

        public async Task<List<MenuItem>> GetAllAsync()
        {
            return await _db.MenuItems.ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<MenuItem?> GetByIdWithIngredientsAsync(long id)
        {
            return await _db.MenuItems
                .Include(m => m.IngredientRolls)
                    .ThenInclude(ir => ir.Ingredient)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task<List<MenuItem>> GetByItemTypeAsync(string itemType)
        {
            return await _db.MenuItems
                .Where(m => m.ItemType == itemType)
                .ToListAsync();
        }

        public async Task<List<MenuItem>> GetByItemTypeWithIngredientsAsync(string itemType)
        {
            return await _db.MenuItems
                .Where(m => m.ItemType == itemType)
                .Include(m => m.IngredientRolls)
                    .ThenInclude(ir => ir.Ingredient)
                .ToListAsync();
        }

        public async Task<List<MenuItem>> GetCustomItemsByUserIdAsync(long userId)
        {
            return await _db.MenuItems
                .Where(m => m.UserId == userId && (m.ItemType == "custom_roll" || m.ItemType == "custom_set"))
                .Include(m => m.IngredientRolls)
                    .ThenInclude(ir => ir.Ingredient)
                .ToListAsync();
        }

        public async Task<List<MenuItem>> GetByCategoryIdAsync(long? categoryId)
        {
            if (categoryId == null)
                return await _db.MenuItems
                    .Where(m => m.CategoryId == null)
                    .ToListAsync();

            return await _db.MenuItems
                .Where(m => m.CategoryId == categoryId)
                .ToListAsync();
        }

        public async Task<bool> NameExistsAsync(string name, long? excludeId = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;

            if (excludeId.HasValue)
            {
                return await _db.MenuItems
                    .AnyAsync(m => m.Name.ToLower() == name.ToLower() && m.Id != excludeId.Value);
            }

            return await _db.MenuItems
                .AnyAsync(m => m.Name.ToLower() == name.ToLower());
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

