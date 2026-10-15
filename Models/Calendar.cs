using System;
using System.Collections.Generic;
using System.Linq;

namespace HabitTracker.Models;

/// <summary>
/// Календарь пользователя. Один на всё приложение.
/// 
/// Доступ к дням — через словарь по дате для O(1).
/// Дни без активности в словаре отсутствуют (создаются при первой отметке).
/// 
/// ВАЖНО для будущей БД: Dictionary не маппится EF Core напрямую.
/// Понадобится либо ValueConverter (сериализация в JSON-колонку),
/// либо отдельная таблица CalendarDays с навигацией.
/// </summary>
public class Calendar
{
    /// <summary>Ключ — дата без времени (00:00:00).</summary>
    public Dictionary<DateTime, CalendarDay> Days { get; set; } = new();

    /// <summary>
    /// Возвращает день на указанную дату, создавая его при отсутствии.
    /// </summary>
    public CalendarDay GetOrCreateDay(DateTime date)
    {
        var key = date.Date;

        if (!Days.TryGetValue(key, out var day))
        {
            day = new CalendarDay { Date = key };
            Days[key] = day;
        }

        return day;
    }

    /// <summary>Возвращает день или null, если активности в этот день не было.</summary>
    public CalendarDay? GetDay(DateTime date)
        => Days.TryGetValue(date.Date, out var day) ? day : null;

    /// <summary>Все дни в диапазоне [from; to] включительно, по возрастанию даты.</summary>
    public IEnumerable<CalendarDay> GetRange(DateTime from, DateTime to)
        => Days.Values
               .Where(d => d.Date >= from.Date && d.Date <= to.Date)
               .OrderBy(d => d.Date);
}