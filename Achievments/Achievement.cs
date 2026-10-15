using HabitTracker.Models;

namespace HabitTracker.Achievements;

/// <summary>
/// Базовое достижение. Конкретные условия реализуются в наследниках.
/// Экземпляры — статические элементы реестра, в БД не хранятся.
/// В <see cref="User.UnlockedAchievementIds"/> хранится только <see cref="Id"/>.
/// </summary>
public abstract class Achievement
{
    /// <summary>Уникальный строковый идентификатор (например, "streak_7").</summary>
    public abstract string Id { get; }

    public abstract string Title { get; }

    public abstract string Description { get; }

    /// <summary>Имя PNG-иконки для UI (например, "trophy_gold.png").</summary>
    public abstract string IconName { get; }

    public abstract AchievementTier Tier { get; }

    /// <summary>
    /// Проверяет, выполнено ли условие получения достижения для данного пользователя.
    /// Вызывается после каждой отметки привычки.
    /// </summary>
    public abstract bool IsUnlocked(User user);
}