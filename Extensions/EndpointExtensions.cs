using JobApplication.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace JobApplication.Extensions
{
    public static class EndpointExtensions
    {
        public static WebApplication MapCustomEndpoints(this WebApplication app)
        {
            app.MapIdentityApi<ApplicationUser>();

            app.MapGet("/api/me", async (HttpContext context, UserManager<ApplicationUser> userManager) =>
            {
                var user = await userManager.GetUserAsync(context.User);
                if (user == null) return Results.Unauthorized();
                return Results.Ok(new { user.Email, user.FullName });
            }).RequireAuthorization();

            app.MapGet("/api/auth/google", () =>
                Results.Challenge(
                    new AuthenticationProperties { RedirectUri = "/api/auth/google/callback" },
                    ["Google"]
                ));

            app.MapGet("/api/auth/google/callback", async (
                HttpContext context,
                UserManager<ApplicationUser> userManager,
                SignInManager<ApplicationUser> signInManager) =>
            {
                // 1. Lekérjük a Google által visszaadott adatokat
                var result = await context.AuthenticateAsync(IdentityConstants.ExternalScheme);
                if (!result.Succeeded) return Results.Unauthorized();

                // 2. Kinyerjük az email, név és Google ID adatokat
                var email = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
                var fullName = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? "";
                var providerKey = result.Principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                if (email == null || providerKey == null) return Results.Unauthorized();

                // 3. Ha még nincs ilyen user, létrehozzuk és összekapcsoljuk a Google accounttal
                var user = await userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    user = new ApplicationUser { Email = email, UserName = email, FullName = fullName };
                    await userManager.CreateAsync(user);
                    await userManager.AddLoginAsync(user, new UserLoginInfo("Google", providerKey, "Google"));
                }

                // 4. Létrehozzuk a Bearer tokent és visszaküldjük
                var principal = await signInManager.CreateUserPrincipalAsync(user);
                return Results.SignIn(principal, authenticationScheme: IdentityConstants.BearerScheme);
            });

            return app;
        }
    }
}
