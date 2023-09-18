using System.Security.Claims;
using _3ai.solutions.SupabaseWrapper.Config;
using Microsoft.Extensions.Options;
using Supabase.Gotrue;

namespace _3ai.solutions.SupabaseWrapper.Services;

public sealed class SupabaseRepo
{
    private readonly string key;
    private readonly string url;
    private Supabase.Client? supabase;

    public SupabaseRepo(IOptions<SupabaseOptions> options)
    {
        key = options.Value.Key;
        url = options.Value.Url;
    }

    public Supabase.Client Supabase
    {
        get
        {
            supabase ??= new Supabase.Client(url, key);
            return supabase;
        }
    }

    public async Task<Data.User?> GetUserAsync(string id)
    {
        var user = await Supabase.AdminAuth(key).GetUserById(id);
        if (user is null)
            return null;
        return user;
    }

    public async Task<IEnumerable<Claim>> GetClaimsAsync(Guid userId)
    {
        var result = await Supabase.From<Data.UserClaim>().Where(x => x.UserId == userId).Get();
        return result.Models.Select(c => new Claim(c.Claim, c.Value)).Concat(new[] { new Claim(ClaimTypes.NameIdentifier, userId.ToString()) });
    }

    public async Task<User?> UpdateUserPasswordAsync(string userId, string password)
    {
        return await Supabase.AdminAuth(key).UpdateUserById(userId, new() { Password = password });
    }

    public async Task<User?> UpdateUserMetadataAsync(string userId, Dictionary<string, object> userMetadata)
    {
        return await Supabase.AdminAuth(key).UpdateUserById(userId, new() { Data = userMetadata });
    }

    public async Task UpsertClaimAsync(Data.UserClaim userClaim)
    {
        await Supabase.From<Data.UserClaim>().Upsert(userClaim);
    }

    public async Task DeleteClaimAsync(Data.UserClaim userClaim)
    {
        await Supabase.From<Data.UserClaim>().Where(x => x == userClaim).Delete();
    }
}
