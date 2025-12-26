using Microsoft.EntityFrameworkCore;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.Repository
{
    public class MenuSetComponentRepository : IRepository<MenuSetComponent>
    {
        private readonly DatabaseContext _db;
        private bool _disposed;

        public MenuSetComponentRepository()
        {
            _db = new DatabaseContext();
        }

        public MenuSetComponentRepository(DatabaseContext context)
        {
            _db = context;
        }

        public async Task AddAsync(MenuSetComponent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _db.MenuSetComponents.AddAsync(entity);
        }

        public void Update(MenuSetComponent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.MenuSetComponents.Update(entity);
        }

        public void Remove(MenuSetComponent entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.MenuSetComponents.Remove(entity);
        }

        public async Task<MenuSetComponent?> GetByIdAsync(long id)
        {
            return await _db.MenuSetComponents.FindAsync(id);
        }

        public async Task<List<MenuSetComponent>> GetAllAsync()
        {
            return await _db.MenuSetComponents.ToListAsync();
        }

        public async Task<MenuSetComponent?> GetByIdWithRelationsAsync(long id)
        {
            return await _db.MenuSetComponents
                .Include(msc => msc.Set)
                .Include(msc => msc.Item)
                .FirstOrDefaultAsync(msc => msc.Id == id);
        }

        public async Task<List<MenuSetComponent>> GetBySetIdAsync(long setId)
        {
            return await _db.MenuSetComponents
                .Where(msc => msc.SetId == setId)
                .Include(msc => msc.Item)
                .ToListAsync();
        }

        public async Task<List<MenuSetComponent>> GetByItemIdAsync(long itemId)
        {
            return await _db.MenuSetComponents
                .Where(msc => msc.ItemId == itemId)
                .Include(msc => msc.Set)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(long setId, long itemId)
        {
            return await _db.MenuSetComponents
                .AnyAsync(msc => msc.SetId == setId && msc.ItemId == itemId);
        }

        public async Task RemoveBySetIdAsync(long setId)
        {
            var items = await _db.MenuSetComponents
                .Where(msc => msc.SetId == setId)
                .ToListAsync();

            _db.MenuSetComponents.RemoveRange(items);
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

