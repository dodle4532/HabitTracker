using HabitTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace HabitTracker.Services;

public class UserService
{
    private readonly AppDBContext _dbContext;

    public UserService(AppDBContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>Получает текущего пользователя. Если его нет — создает нового.</summary>
    public async Task<User> GetOrCreateUserAsync()
    {
        var user = await _dbContext.Users.FirstOrDefaultAsync();
        if (user == null)
        {
            user = new User { Name = "Пользователь" };
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
        }
        return user;
    }

    /// <summary>Начисляет очки пользователю и обновляет рекордный стрик.</summary>
    public async Task AddPointsAsync(User user, int points)
    {
        user.TotalPoints += points;

        if (user.CurrentStreak > user.LongestStreak)
            user.LongestStreak = user.CurrentStreak;

        _dbContext.Users.Update(user);
        await _dbContext.SaveChangesAsync();
    }
}