using System.Security.Claims;
using Duende.IdentityModel;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityService.Models;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Services;

public class CustomProfileService(UserManager<ApplicationUser> userManager)
    : IProfileService
{
    private readonly UserManager<ApplicationUser> _userManager = userManager;

    public async Task GetProfileDataAsync(ProfileDataRequestContext context)
    {
        var user = await _userManager.GetUserAsync(context.Subject);

        if (user == null) return;

        var existingClaims = await _userManager.GetClaimsAsync(user);

        var claims = new List<Claim>();

        if (!string.IsNullOrWhiteSpace(user.UserName))
            claims.Add(new Claim("username", user.UserName));

        context.IssuedClaims.AddRange(claims);

        var nameClaim = existingClaims.FirstOrDefault(x => x.Type == JwtClaimTypes.Name);
        if (nameClaim != null) context.IssuedClaims.Add(nameClaim);
    }

    public Task IsActiveAsync(IsActiveContext context)
    {
        return Task.CompletedTask;
    }
}