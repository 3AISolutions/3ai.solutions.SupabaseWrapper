namespace _3ai.solutions.SupabaseWrapper.Data;

public class AuthResponse
{
    public string? Message { get; set; }
    public Exception? Exception { get; set; }
    public bool IsSuccess { get; set; }
}
