namespace _3ai.solutions.SupabaseWrapper.Config;

public record SupabaseOptions
{
    public string Key { get; init; } = string.Empty;
    public string Url { get; init; } = string.Empty;
}
