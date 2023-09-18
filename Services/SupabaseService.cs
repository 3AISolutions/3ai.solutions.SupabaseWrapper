using System.Security.Claims;
using _3ai.solutions.SupabaseWrapper.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Supabase.Gotrue;

namespace _3ai.solutions.SupabaseWrapper.Services;

public sealed class SupabaseService
{
    public const string Callback = "supa/callback";
    public const string CallbackAccount = "supa/account";
    public const string CallbackAccountRegister = "supa/accountregister";

    private readonly SignInManager<Data.User> signInManager;
    private readonly SupabaseRepo supabaseRepo;

    public SupabaseService(SignInManager<Data.User> signInManager, SupabaseRepo supabaseRepo)
    {
        this.supabaseRepo = supabaseRepo;
        this.signInManager = signInManager;
    }

    private string RedirectTo
    {
        get
        {
            return $"{signInManager.Context.Request.Scheme}://{signInManager.Context.Request.Host}/{Callback}";
        }
    }

    private string RedirectToAccount
    {
        get
        {
            return $"{signInManager.Context.Request.Scheme}://{signInManager.Context.Request.Host}/{CallbackAccount}";
        }
    }

    private string RedirectToAccountRegister
    {
        get
        {
            return $"{signInManager.Context.Request.Scheme}://{signInManager.Context.Request.Host}/{CallbackAccountRegister}";
        }
    }


    public async Task<Dictionary<string, string>> GetProviderUrlsAsync()
    {
        Dictionary<string, string> providerUrls = new();
        var session = await supabaseRepo.Supabase.Auth.SignIn(Constants.Provider.Azure, new()
        {
            RedirectTo = RedirectTo,
            Scopes = "email User.Read",
        });
        providerUrls.Add(Constants.Provider.Azure.ToString(), session.Uri.ToString());
        session = await supabaseRepo.Supabase.Auth.SignIn(Constants.Provider.Google, new()
        {
            RedirectTo = RedirectTo,
        });
        providerUrls.Add(Constants.Provider.Google.ToString(), session.Uri.ToString());
        return providerUrls;
    }

    public async Task<AuthResponse> SignInAsync(string email, string password)
    {
        var response = new AuthResponse();
        try
        {
            var session = await supabaseRepo.Supabase.Auth.SignInWithPassword(email, password);
            await SignInUserAsync(session?.User);
            response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<AuthResponse> SignUpAsync(string email, string password)
    {
        var response = new AuthResponse();
        try
        {
            var session = await supabaseRepo.Supabase.Auth.SignUp(email, password, options: new() { RedirectTo = RedirectToAccountRegister });
            response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<AuthResponse> ResetPasswordForEmailAsync(string email)
    {
        var response = new AuthResponse();
        try
        {
            await supabaseRepo.Supabase.Auth.SendMagicLink(email, options: new() { RedirectTo = RedirectToAccount });
            response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
            response.Message = ex.Message;
        }
        return response;
    }

    public async Task<AuthResponse> SignInAsync(string access_token)
    {
        var response = new AuthResponse();
        try
        {
            //var sess = await supabaseRepo.Supabase.Auth.GetSessionFromUrl(new Uri(""), true);
            var user = await supabaseRepo.Supabase.Auth.GetUser(access_token);
            if (user is not null)
            {
                //TODO: Check if we need to cache this session
                await SignInUserAsync(user);
            }
            response.IsSuccess = true;
        }
        catch (Exception ex)
        {
            response.Exception = ex;
            response.Message = ex.Message;
        }
        return response;
    }

    private async Task SignInUserAsync(Supabase.Gotrue.User? user)
    {
        if (user?.Id is not null)
        {
            await signInManager.SignInWithClaimsAsync(
                user,
                new AuthenticationProperties()
                {
                    IsPersistent = true,
                },
                await supabaseRepo.GetClaimsAsync(Guid.Parse(user.Id))
            );
        }
    }

    public async Task<AuthResponse> UpdateUserPasswordAsync(string password)
    {
        var response = new AuthResponse();
        try
        {
            var userId = signInManager.Context.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userId is not null)
            {
                var resp = await supabaseRepo.UpdateUserPasswordAsync(userId.Value, password);
                response.IsSuccess = true;
            }
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Exception = ex;
            response.Message = ex.Message;
        }
        return response;
    }
}
