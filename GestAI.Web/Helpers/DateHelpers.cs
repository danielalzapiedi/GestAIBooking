namespace GestAI.Web.Helpers
{
    public static class DateHelpers
    {
        public static DateOnly ToDateOnly(this DateTime dt) => DateOnly.FromDateTime(dt);
        public static DateTime ToDateTime(this DateOnly d) => d.ToDateTime(TimeOnly.MinValue);
    }

}
