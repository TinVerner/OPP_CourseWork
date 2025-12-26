using Microsoft.EntityFrameworkCore;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.Repository
{
    public class IngredientRollRepository : IRepository<IngredientRoll>
    {
        private readonly DatabaseContext _db;
        private bool _disposed;

        public IngredientRollRepository()
        {
            _db = new DatabaseContext();
        }

        public IngredientRollRepository(DatabaseContext context)
        {
            _db = context;
        }

        public async Task AddAsync(IngredientRoll entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _db.IngredientRolls.AddAsync(entity);
        }

        public void Update(IngredientRoll entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.IngredientRolls.Update(entity);
        }

        public void Remove(IngredientRoll entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.IngredientRolls.Remove(entity);
        }

        public async Task<IngredientRoll?> GetByIdAsync(long id)
        {
            return await _db.IngredientRolls.FindAsync(id);
        }

        public async Task<List<IngredientRoll>> GetAllAsync()
        {
            return await _db.IngredientRolls.ToListAsync();
        }

        public async Task<IngredientRoll?> GetByIdWithRelationsAsync(long id)
        {
            return await _db.IngredientRolls
                .Include(ir => ir.Roll)
                .Include(ir => ir.Ingredient)
                .FirstOrDefaultAsync(ir => ir.Id == id);
        }

        public async Task<List<IngredientRoll>> GetByRollIdAsync(long rollId)
        {
            return await _db.IngredientRolls
                .Where(ir => ir.RollId == rollId)
                .Include(ir => ir.Ingredient)
                .ToListAsync();
        }

        public async Task<List<IngredientRoll>> GetByIngredientIdAsync(long ingredientId)
        {
            return await _db.IngredientRolls
                .Where(ir => ir.IngredientId == ingredientId)
                .Include(ir => ir.Roll)
                .ToListAsync();
        }

        public async Task<bool> ExistsAsync(long rollId, long ingredientId)
        {
            return await _db.IngredientRolls
                .AnyAsync(ir => ir.RollId == rollId && ir.IngredientId == ingredientId);
        }

        public async Task RemoveByRollIdAsync(long rollId)
        {
            var items = await _db.IngredientRolls
                .Where(ir => ir.RollId == rollId)
                .ToListAsync();

            _db.IngredientRolls.RemoveRange(items);
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

