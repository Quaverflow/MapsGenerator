using System.Text;

namespace MapsGenerator.Helpers;

public static class StringBuilderExtensions
{
    private const string Space = "    ";

    public static void AppendLine(this StringBuilder builder, string text, int indent)
        => builder.AppendLine(Space.Repeat(indent) + text);
}