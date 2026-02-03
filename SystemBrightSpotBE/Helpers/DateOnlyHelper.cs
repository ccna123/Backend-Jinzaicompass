namespace SystemBrightSpotBE.Helpers
{
    public class DateOnlyHelper
    {
        public static DateOnly? Parse(string? dateStr)
        {
            if (string.IsNullOrWhiteSpace(dateStr)) return null;

            return DateOnly.TryParseExact(dateStr, "yyyy/MM/dd", out var dob) ? dob : (DateOnly?)null;
        }
    }
}
