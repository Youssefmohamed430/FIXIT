namespace FIXIT.Domain.Abstractions;

/*
     التخزين Storage → يكون بـ UTC
     العرض Display → يكون ب Egypt Time
     الإدخال Input → تحوله من Egypt Time إلى UTC قبل الحفظ
*/
public static class EgyptTimeHelper
{
    private static readonly TimeZoneInfo EgyptTimeZone =
        TimeZoneInfo.FindSystemTimeZoneById("Egypt Standard Time");

    /// الوقت الحالي بتوقيت مصر
    public static DateTime Now => TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, EgyptTimeZone);

    /// التاريخ الحالي بتوقيت مصر
    public static DateOnly TodayDateOnly => DateOnly.FromDateTime(Now);

    /// الوقت الحالي كـ TimeSpan (مفيد للـ TimeOnly)
    public static TimeSpan CurrentTimeOfDay => Now.TimeOfDay;

    /// تحويل وقت مصر إلى UTC
    public static DateTime ConvertToUtc(DateTime egyptDateTime)
    {
        return TimeZoneInfo.ConvertTimeToUtc(egyptDateTime, EgyptTimeZone);
    }

    /// تحويل وقت UTC إلى مصر
    public static DateTime ConvertFromUtc(DateTime utcDateTime)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(utcDateTime, EgyptTimeZone);
    }

    /// تحويل تاريخ + وقت (Egypt) إلى UTC
    public static DateTime CombineToUtc(DateOnly date, TimeOnly time)
    {
        var egyptDateTime = date.ToDateTime(time);
        return ConvertToUtc(egyptDateTime);
    }

    /// تحويل UTC إلى DateOnly + TimeOnly (بتوقيت مصر)
    public static (DateOnly date, TimeOnly time) SplitFromUtc(DateTime utcDateTime)
    {
        var egyptDateTime = ConvertFromUtc(utcDateTime);
        return (DateOnly.FromDateTime(egyptDateTime), TimeOnly.FromDateTime(egyptDateTime));
    }
}
