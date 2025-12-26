using Microsoft.EntityFrameworkCore;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.Repository
{
    public class OrderItemRepository : IRepository<OrderItem>
    {
        private readonly DatabaseContext _db;
        private bool _disposed;

        public OrderItemRepository()
        {
            _db = new DatabaseContext();
        }

        public OrderItemRepository(DatabaseContext context)
        {
            _db = context;
        }

        public async Task AddAsync(OrderItem entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _db.OrderItems.AddAsync(entity);
        }

        public void Update(OrderItem entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.OrderItems.Update(entity);
        }

        public void Remove(OrderItem entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.OrderItems.Remove(entity);
        }

        public async Task<OrderItem?> GetByIdAsync(long id)
        {
            return await _db.OrderItems.FindAsync(id);
        }

        public async Task<List<OrderItem>> GetAllAsync()
        {
            return await _db.OrderItems.ToListAsync();
        }

        public async Task<OrderItem?> GetByIdWithMenuItemAsync(long id)
        {
            return await _db.OrderItems
                .Include(oi => oi.MenuItem)
                .FirstOrDefaultAsync(oi => oi.Id == id);
        }

        public async Task<List<OrderItem>> GetByOrderIdAsync(long orderId)
        {
            return await _db.OrderItems
                .Where(oi => oi.OrderId == orderId)
                .Include(oi => oi.MenuItem)
                .ToListAsync();
        }

        public async Task<List<OrderItem>> GetByMenuItemIdAsync(long menuItemId)
        {
            return await _db.OrderItems
                .Where(oi => oi.MenuItemId == menuItemId)
                .ToListAsync();
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

