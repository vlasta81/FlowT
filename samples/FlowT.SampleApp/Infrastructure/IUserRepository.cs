using FlowT.SampleApp.Domain;

namespace FlowT.SampleApp.Infrastructure;

/// <summary>
/// In-memory user repository (for demo purposes)
/// </summary>
public interface IUserRepository
{
    Task<List<User>> GetAllAsync(CancellationToken ct = default);
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<User> CreateAsync(User user, CancellationToken ct = default);
    Task<User?> UpdateAsync(User user, CancellationToken ct = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken ct = default);
}

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    private readonly SemaphoreSlim _lock = new(1, 1);

    public async Task<List<User>> GetAllAsync(CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _users.ToList();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _users.FirstOrDefault(u => u.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<User> CreateAsync(User user, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            _users.Add(user);
            return user;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<User?> UpdateAsync(User user, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            var index = _users.FindIndex(u => u.Id == user.Id);
            if (index == -1) return null;

            _users[index] = user;
            return user;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        await _lock.WaitAsync(ct);
        try
        {
            return _users.RemoveAll(u => u.Id == id) > 0;
        }
        finally
        {
            _lock.Release();
        }
    }
}
