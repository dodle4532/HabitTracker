using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace HabitTracker.Models;

/// <summary>
/// Пользователь приложения. Один на всё приложение.
/// </summary>
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;

    #region Очки и уровень

    // ── Очки и уровень ─────────────────────────────────────────────

    /// <summary>Сумма всех заработанных очков за всё время.</summary>
    public int TotalPoints { get; set; } = 0;

    /// <summary>
    /// Уровень вычисляется из <see cref="TotalPoints"/> по формуле sqrt(points / K).
    /// K задаётся константой <see cref="LevelCoefficient"/>.
    /// Не хранится в БД.
    /// </summary>
    [NotMapped]
    public int Level => (int)Math.Floor(Math.Sqrt(TotalPoints / LevelCoefficient));

    /// <summary>Коэффициент кривой уровней. Чем больше — тем медленнее рост.</summary>
    public const double LevelCoefficient = 50.0;

    /// <summary>Очки, необходимые для достижения указанного уровня (обратная формула).</summary>
    public static int PointsForLevel(int level) => (int)(level * level * LevelCoefficient);

    #endregion

    #region Общий стрик

    // ── Общий стрик ────────────────────────────────────────────────

    /// <summary>
    /// Дней подряд, когда ВСЕ запланированные на день привычки выполнены полностью.
    /// Неполное выполнение любой привычки сбрасывает стрик (очки при этом начисляются).
    /// </summary>
    public int CurrentStreak { get; set; } = 0;

    /// <summary>Рекордный общий стрик за всё время.</summary>
    public int LongestStreak { get; set; } = 0;

    /// <summary>
    /// Дата последней активности. Нужна, чтобы корректно сбросить стрик при возврате
    /// после долгого отсутствия (например, если пропущен целый день).
    /// </summary>
    public DateTime? LastActivityDate { get; set; }

    #endregion

    #region Достижения

    // ── Достижения ─────────────────────────────────────────────────

    /// <summary>
    /// Id полученных достижений. Сами достижения — статический реестр в коде
    /// (см. Achievements), здесь только ссылки на них.
    /// </summary>
    public List<string> UnlockedAchievementIds { get; set; } = new();

    #endregion
}