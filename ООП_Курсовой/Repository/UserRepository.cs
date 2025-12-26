using Microsoft.EntityFrameworkCore;
using ООП_Курсовой.Tables;

namespace ООП_Курсовой.Repository
{
    public class UserRepository : IRepository<User>
    {
        private readonly DatabaseContext _db;
        private bool _disposed;

        public UserRepository()
        {
            _db = new DatabaseContext();
        }

        public UserRepository(DatabaseContext context)
        {
            _db = context;
        }

        public async Task AddAsync(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            await _db.Users.AddAsync(entity);
        }

        public void Update(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.Users.Update(entity);
        }

        public void Remove(User entity)
        {
            if (entity == null)
                throw new ArgumentNullException(nameof(entity));

            _db.Users.Remove(entity);
        }

        public async Task<User?> GetByIdAsync(long id)
        {
            return await _db.Users.FindAsync(id);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await _db.Users.ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _db.SaveChangesAsync();
        }

        public async Task<bool> LoginExistsAsync(string normalizedLogin)
        {
            if (string.IsNullOrWhiteSpace(normalizedLogin))
                return false;

            return await _db.Users
                .AnyAsync(u => u.Login.ToLower() == normalizedLogin);
        }

        public async Task<bool> EmailExistsAsync(string normalizedEmail)
        {
            if (string.IsNullOrWhiteSpace(normalizedEmail))
                return false;

            return await _db.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);
        }

        public async Task<bool> PhoneExistsAsync(string normalizedPhone)
        {
            if (string.IsNullOrWhiteSpace(normalizedPhone))
                return false;

            return await _db.Users
                .AnyAsync(u => u.Phone == normalizedPhone);
        }

        public async Task<long?> GetRoleIdByNameAsync(string roleName)
        {
            if (string.IsNullOrWhiteSpace(roleName))
                return null;

            return await _db.Roles
                .Where(r => r.Name == roleName)
                .Select(r => (long?)r.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<string?> ValidateNewUserAsync(string normalizedLogin, string normalizedEmail, string normalizedPhone)
        {
            if (await LoginExistsAsync(normalizedLogin))
                return "Логин уже занят.";

            if (await EmailExistsAsync(normalizedEmail))
                return "Email уже используется.";

            if (await PhoneExistsAsync(normalizedPhone))
                return "Телефон уже используется.";

            return null;
        }

        public async Task<User?> GetWithRoleByIdAsync(long id)
        {
            return await _db.Users
                .Include(u => u.Role)
                .SingleOrDefaultAsync(u => u.Id == id);
        }

        public async Task<string?> ValidateUpdateUserAsync(
            long userId,
            string normalizedLogin,
            string normalizedEmail,
            string normalizedPhone)
        {
            if (await _db.Users.AnyAsync(u =>
                    u.Id != userId && u.Login.ToLower() == normalizedLogin))
                return "Такой логин уже используется.";

            if (await _db.Users.AnyAsync(u =>
                    u.Id != userId && u.Email.ToLower() == normalizedEmail))
                return "Такой email уже используется.";

            if (await _db.Users.AnyAsync(u =>
                    u.Id != userId && u.Phone == normalizedPhone))
                return "Такой телефон уже используется.";

            return null;
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