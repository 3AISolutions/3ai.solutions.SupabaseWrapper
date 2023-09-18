using _3ai.solutions.ReceiptBook.SupabaseWrapper.Data;
using _3ai.solutions.SupabaseWrapper.Data;
using _3ai.solutions.SupabaseWrapper.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace _3ai.solutions.SupabaseWrapper;

public static class SupabaseDependencyInjection
{

    private static async Task<IResult> PostLoginAsync(SupabaseService supabaseService, IHttpContextAccessor contextAccessor, string? returnUrl)
    {
        string retURL = "/";
        if (contextAccessor.HttpContext is not null &&
            contextAccessor.HttpContext.Request.Form.TryGetValue("email", out var email) &&
            contextAccessor.HttpContext.Request.Form.TryGetValue("password", out var password))
        {
            var resp = await supabaseService.SignInAsync(email!, password!);
            if (!resp.IsSuccess)
            {
                var msg = "Invalid login credentials";
                if (!string.IsNullOrEmpty(resp.Message))
                {
                    var err = ErrorResponse.FromJson(resp.Message);
                    if (err is not null && !string.IsNullOrEmpty(err.error_description))
                        msg = err.error_description;
                }

                retURL = $"/login/login/{msg}";
            }
        }
        return Results.LocalRedirect(returnUrl ?? retURL);
    }

    private static async Task<IResult> PostSignUpAsync(SupabaseService supabaseService, IHttpContextAccessor contextAccessor, string? returnUrl)
    {
        string retURL = "/Login/SignupConfirm";
        if (contextAccessor.HttpContext is not null &&
            contextAccessor.HttpContext.Request.Form.TryGetValue("email", out var email) &&
            contextAccessor.HttpContext.Request.Form.TryGetValue("password", out var password))
        {
            var resp = await supabaseService.SignUpAsync(email!, password!);
            if (!resp.IsSuccess)
            {
                var msg = "Invalid email or password";
                if (!string.IsNullOrEmpty(resp.Message))
                {
                    var err = ErrorResponse.FromJson(resp.Message);
                    if (err is not null && !string.IsNullOrEmpty(err.msg))
                        msg = err.msg;
                }

                retURL = $"/login/signup/{msg}";

            }
        }
        return Results.LocalRedirect(returnUrl ?? retURL);
    }

    private static async Task<IResult> PostResetPasswordForEmailAsync(SupabaseService supabaseService, IHttpContextAccessor contextAccessor, string? returnUrl)
    {
        if (contextAccessor.HttpContext is not null &&
            contextAccessor.HttpContext.Request.Form.TryGetValue("email", out var email))
        {
            await supabaseService.ResetPasswordForEmailAsync(email!);
        }
        return Results.LocalRedirect(returnUrl ?? "/Login/resetconfirm");
    }

    private static async Task<IResult> GetLogoutAsync(SignInManager<User> signInManager, string? returnUrl)
    {
        await signInManager.SignOutAsync();
        return Results.LocalRedirect(returnUrl ?? "/Login");
    }

    private static async Task<IResult> GetCallbackAsync(SupabaseService supabaseService, string? error, string? error_description, string? error_code, string? refresh_token, string? token_type, string? access_token, ILogger<SupabaseService> logger)
    {
        if (error is not null)
            logger.LogError("GetCallbackAsync {error} {error_description} {error_code}", error, error_description, error_code);
        if (error is null && access_token is null)
            return Results.Content("<script>if (window.location.hash) {const newUrl = window.location.href.replace('#', '?'); window.location.href = newUrl; }</script>", "text/html");
        if (access_token is not null)
        {
            var resp = await supabaseService.SignInAsync(access_token);
            if (resp.Exception is not null)
                logger.LogError(resp.Exception, "GetCallbackAsync {token}", access_token);
        }
        return Results.Redirect("/");
    }

    private static async Task<IResult> GetCallbackAccountAsync(SupabaseService supabaseService, string? error, string? error_description, string? error_code, string? refresh_token, string? token_type, string? access_token)
    {
        if (error is null && access_token is null)
            return Results.Content("<script>if (window.location.hash) {const newUrl = window.location.href.replace('#', '?'); window.location.href = newUrl; }</script>", "text/html");
        if (access_token is not null)
            await supabaseService.SignInAsync(access_token);
        return Results.Redirect("/User/Account");
    }

    private static async Task<IResult> GetCallbackAccountRegisterAsync(SupabaseService supabaseService, string? error, string? error_description, string? error_code, string? refresh_token, string? token_type, string? access_token)
    {
        if (error is null && access_token is null)
            return Results.Content("<script>if (window.location.hash) {const newUrl = window.location.href.replace('#', '?'); window.location.href = newUrl; }</script>", "text/html");
        if (access_token is not null)
            await supabaseService.SignInAsync(access_token);
        return Results.Redirect("/Login/signupcomplete");
    }

    public static IEndpointRouteBuilder MapSupabaseEndpoints(this IEndpointRouteBuilder app)
    {
        var privateGroup = app.MapGroup("").ExcludeFromDescription();
        privateGroup.MapGet(SupabaseService.Callback, GetCallbackAsync);
        privateGroup.MapGet(SupabaseService.CallbackAccount, GetCallbackAccountAsync);
        privateGroup.MapGet(SupabaseService.CallbackAccountRegister, GetCallbackAccountRegisterAsync);
        privateGroup = app.MapGroup("supa").ExcludeFromDescription();
        privateGroup.MapPost("login", PostLoginAsync);
        privateGroup.MapPost("signup", PostSignUpAsync);
        privateGroup.MapPost("resetpassword", PostResetPasswordForEmailAsync);
        privateGroup.MapGet("logout", GetLogoutAsync);
        return app;
    }
}
