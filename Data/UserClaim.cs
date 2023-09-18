using Postgrest.Attributes;
using Postgrest.Models;

namespace _3ai.solutions.SupabaseWrapper.Data;

[Table("UserClaims")]
public class UserClaim : BaseModel
{
    [Column("userId")]
    public Guid UserId { get; set; }
    [Column("claim")]
    public string Claim { get; set; } = string.Empty;
    [Column("value")]
    public string Value { get; set; } = string.Empty;
}
