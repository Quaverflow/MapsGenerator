namespace MapsGenerator.Helpers.Extensions;

public static class StringExtensions
{
    public static string Repeat(this string text, int n)
    {
        var textAsSpan = text.AsSpan();
        var span = new Span<char>(new char[textAsSpan.Length * n]);
        for (var i = 0; i < n; i++)
        {
            textAsSpan.CopyTo(span.Slice(i * textAsSpan.Length, textAsSpan.Length));
        }

        return span.ToString();
    }

    public static string FirstCharToLower(this string input)
    {
        return string.IsNullOrEmpty(input)
            ? string.Empty
            : $"{char.ToLower(input[0])}{input.Substring(1)}";
    }
    public static string FirstCharToUpper(this string input)
    {
        return string.IsNullOrEmpty(input)
            ? string.Empty
            : $"{char.ToUpper(input[0])}{input.Substring(1)}";
    }
}