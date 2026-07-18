namespace OmniBlox.Shared.Extensions;

public static class EnumExtensions
{
    public static T ToEnumOrDefault<T>(this string? value, T defaultValue = default) where T : struct, Enum
    {
        if (value is not null && Enum.TryParse<T>(value, true, out var result))
            return result;
        return defaultValue;
    }
}
