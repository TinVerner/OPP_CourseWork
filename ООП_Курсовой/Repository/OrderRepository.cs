using Microsoft.EntityFrameworkCore;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.Repository;

public class OrderRepository : IRepository<Order>
{
    private readonly DatabaseContext _db;
    private bool _disposed;

    public OrderRepository()
    {
        _db = new DatabaseContext();
    }

    public OrderRepository(DatabaseContext context)
    {
        _db = context;
    }

    public async Task AddAsync(Order entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        await _db.Orders.AddAsync(entity);
    }

    public void Update(Order entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _db.Orders.Update(entity);
    }

    public void Remove(Order entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        _db.Orders.Remove(entity);
    }

    public async Task<Order?> GetByIdAsync(long id)
    {
        return await _db.Orders.FindAsync(id);
    }

    public async Task<List<Order>> GetAllAsync()
    {
        return await _db.Orders.ToListAsync();
    }

    public async Task<Order?> GetByIdWithItemsAsync(long id)
    {
        return await _db.Orders
            .Include(o => o.User)
            .FirstOrDefaultAsync(o => o.Id == id);
    }

    public async Task<List<Order>> GetByUserIdAsync(long userId)
    {
        return await _db.Orders
            .Where(o => o.UserId == userId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByStatusAsync(OrderStatus status)
    {
        return await _db.Orders
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<Order>> GetByDeliveryTypeAsync(DeliveryType deliveryType)
    {
        return await _db.Orders
            .Where(o => o.DeliveryType == deliveryType)
            .OrderByDescending(o => o.CreatedAt)
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

