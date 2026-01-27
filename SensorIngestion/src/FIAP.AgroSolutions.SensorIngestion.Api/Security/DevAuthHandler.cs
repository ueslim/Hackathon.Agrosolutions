using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace FIAP.AgroSolutions.SensorIngestion.Api.Security;

public class DevAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "DevAuth";
    public const string HeaderUserId = "x-dev-user-id";

    public DevAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder) : base(options, logger, encoder) { }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Para dev/demo: sempre autentica.
        // Se header vier, usa ele como NameIdentifier, senão usa um GUID fixo.
        var userId = Context.Request.Headers.TryGetValue(HeaderUserId, out var v) && Guid.TryParse(v, out var g)
            ? g
            : Guid.Parse("00000000-0000-0000-0000-000000000001");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "dev-user")
        };

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
