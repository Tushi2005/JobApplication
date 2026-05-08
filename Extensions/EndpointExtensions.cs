using JobApplication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

record RegisterRequest(string Email, string Password, string FullName);

namespace JobApplication.Extensions
{
    public static class EndpointExtensions
    {
        public static WebApplication MapCustomEndpoints(this WebApplication app)
        {
            var frontendUrl = app.Configuration["FrontendUrl"]!;

            // A MapIdentityApi adja a /login és /refresh endpointokat, de a fullName-t nem kezeli,
            // ezért van külön /api/auth/register endpointunk
            app.MapIdentityApi<ApplicationUser>()
                .RequireRateLimiting("login");

            app.MapGet("/api/me", async (HttpContext context, UserManager<ApplicationUser> userManager) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                return Results.Ok(new { user.Email, user.FullName });
            }).RequireAuthorization();

            app.MapPost("/api/auth/register", async (
                RegisterRequest request,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager) =>
            {
                var user = new ApplicationUser
                {
                    Email = request.Email,
                    UserName = request.Email,
                    FullName = request.FullName
                };

                var result = await userManager.CreateAsync(user, request.Password);
                if (!result.Succeeded)
                    return Results.Conflict(new { message = "A regisztráció sikertelen. Valószínűleg az email már használatban van." });

                var principal = await signInManager.CreateUserPrincipalAsync(user);
                return Results.SignIn(principal, authenticationScheme: IdentityConstants.BearerScheme);
            }).RequireRateLimiting("login");

            app.MapGet("/api/auth/google", () =>
                Results.Challenge(
                    new AuthenticationProperties
                    {
                        RedirectUri = "/api/auth/google/callback",
                        // Mindig megmutatja a fiókválasztót, hogy megosztott gépen
                        // ne lehessen véletlenül más fiókjába belépni
                        Parameters = { ["prompt"] = "select_account" }
                    },
                    ["Google"]
                ));

            app.MapGet("/api/auth/google/callback", async (
                HttpContext context,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager) =>
            {
                var result = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
                if (!result.Succeeded) return Results.Unauthorized();

                var email = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var fullName = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
                var providerKey = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (email == null || providerKey == null) return Results.Unauthorized();

                // Ha még nincs fiókja, automatikusan regisztráljuk a Google profil adataival
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser { Email = email, UserName = email, FullName = fullName };
                    await userManager.CreateAsync(user);
                    await userManager.AddLoginAsync(user, new UserLoginInfo("Google", providerKey, "Google"));
                }

                var tokenOptions = context.RequestServices
                    .GetRequiredService<IOptionsMonitor<BearerTokenOptions>>()
                    .Get(IdentityConstants.BearerScheme);

                var principal = await signInManager.CreateUserPrincipalAsync(user);
                var now = DateTimeOffset.UtcNow;

                // ExpiresUtc kötelező, különben a BearerTokenHandler érvénytelennek jelöli a tokent
                var accessToken = tokenOptions.BearerTokenProtector.Protect(
                    new AuthenticationTicket(principal,
                        new AuthenticationProperties
                        {
                            IssuedUtc = now,
                            ExpiresUtc = now + tokenOptions.BearerTokenExpiration
                        },
                        IdentityConstants.BearerScheme));

                var refreshToken = tokenOptions.RefreshTokenProtector.Protect(
                    new AuthenticationTicket(principal,
                        new AuthenticationProperties
                        {
                            IssuedUtc = now,
                            ExpiresUtc = now + tokenOptions.RefreshTokenExpiration
                        },
                        IdentityConstants.BearerScheme));

                // A tokeneket URL paraméterként adjuk át, mert Google callback után
                // nem tudunk közvetlenül a böngésző localStorage-ba írni
                return Results.Redirect(
                    $"{frontendUrl}/auth/callback?accessToken={accessToken}&refreshToken={refreshToken}");
            });

            return app;
        }
    }
}
