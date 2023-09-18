using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace _3ai.solutions.SupabaseWrapper.Data;
public class User : IdentityUser
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public static implicit operator User(Supabase.Gotrue.User user)
    {
        return new()
        {
            Id = user.Id ?? throw new ArgumentNullException(nameof(user)),
            UserName = user.UserMetadata.GetValueOrDefault(ClaimTypes.Name)?.ToString() ?? user.Email,
            Email = user.Email,
            EmailConfirmed = user.EmailConfirmedAt is not null,
            PhoneNumber = user.Phone,
            PhoneNumberConfirmed = user.PhoneConfirmedAt is not null,
            FirstName = user.UserMetadata.GetValueOrDefault(ClaimTypes.GivenName)?.ToString() ?? "",
            LastName = user.UserMetadata.GetValueOrDefault(ClaimTypes.Surname)?.ToString() ?? "",
        };
    }
}
