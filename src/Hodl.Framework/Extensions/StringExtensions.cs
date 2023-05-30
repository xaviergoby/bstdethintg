using System.Text.RegularExpressions;

namespace Hodl.Extensions;

public static class StringExtensions
{
    public static T ParseEnum<T>(this string value) => (T)Enum.Parse(typeof(T), value, true);

    /// <summary>
    /// Splits the string in parts and inserts a fill char between the sections 
    /// in the returned string. if the last part is smaller than the partslength
    /// the leftover can optionally be added to the result. By default this is 
    /// done.
    /// 
    /// Example:
    ///   "123456789012".SplitFormat(4, '-')      > "1234-5678-9012"
    ///   "1234567890".SplitFormat(4, '-')        > "1234-5678-90"
    ///   "1234567890".SplitFormat(4, '-', false) > "1234-5678"
    /// </summary>
    /// <param name="value"></param>
    /// <param name="partsLength"></param>
    /// <param name="fillChar"></param>
    /// <returns></returns>
    public static string SplitFormat(this string value, int partsLength, char fillChar, bool addLeftover = true)
    {
        if (partsLength <= 0)
            throw new ArgumentOutOfRangeException(nameof(partsLength), "Length of parts must be more than 0.");

        var parts = Enumerable
            .Range(0, value.Length / partsLength)
            .Select(i => value.Substring(i * partsLength, partsLength))
            .ToList();

        int leftover = value.Length % partsLength;
        if (leftover > 0 && addLeftover)
            parts.Add(value[(value.Length - leftover)..]);

        return string.Join(fillChar, parts);
    }

    /// <summary>
    /// Generates a slug for the text. A slug is compatible with the URL path 
    /// component definitions. Spaces are converted to dashes and all characters
    /// are lower case.
    /// </summary>
    /// <param name="phrase"></param>
    /// <returns></returns>
    public static string GenerateSlug(this string phrase)
    {
        var str = phrase.ToLowerInvariant();

        // invalid chars
        str = Regex.Replace(str, @"[^a-z0-9\s-]", string.Empty);

        // convert multiple spaces into one space
        str = Regex.Replace(str, @"\s+", " ").Trim();

        // cut and trim
        str = str[..(str.Length <= 45 ? str.Length : 45)].Trim();
        str = str.Replace(' ', '-'); // hyphens

        return str;
    }

    /// <summary>
    /// This function converts a string input parameter that is formatted as 
    /// date or datetime to an actual DateTime object. The standards and format
    /// should be UTC. The returned DateTime is also DeteTimeKind.Utc to be able
    /// to use it in Postgres SQL queries.
    /// </summary>
    /// <param name="value">UTC formatted date or datetime string</param>
    /// <param name="defaultDate">The default date when a string is empty or not a date</param>
    /// <returns></returns>
    public static DateTime ParamToUtcDate(this string value, DateTime defaultDate)
    {
        if (!string.IsNullOrEmpty(value) && DateTime.TryParse(value, out DateTime d)) return new DateTime(d.Ticks, DateTimeKind.Utc);

        return new DateTime(defaultDate.Ticks, DateTimeKind.Utc);
    }
}
