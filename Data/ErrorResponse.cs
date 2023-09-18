namespace _3ai.solutions.ReceiptBook.SupabaseWrapper.Data
{
    public class ErrorResponse
    {
        public string? error { get; set; }

        public string? error_description { get; set; }
        public int? code { get; set; }
        public string? msg { get; set; }


        public static ErrorResponse FromJson(string json)
        {
            return System.Text.Json.JsonSerializer.Deserialize<ErrorResponse>(json) ?? new ErrorResponse();
        }

    }
}
