using ITI.gRBC.Server.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace ITI.gRBC.Server.Handlers
{
    public class ApiKeyAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IApiKeyAuthService apiKeyAuthService;

        public ApiKeyAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, IApiKeyAuthService apiKeyAuthService) : base(options, logger, encoder)
        {
            this.apiKeyAuthService = apiKeyAuthService;
        }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var isAuthenticated = apiKeyAuthService.Authenticate();

            if (!isAuthenticated)
            {
                return Task.FromResult(AuthenticateResult.Fail("Invalid API Key"));
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, "API UserName"),
                new Claim(ClaimTypes.Role, "API User")
            };

            var identity = new ClaimsIdentity(claims, Scheme.Name);

            var principal = new ClaimsPrincipal(identity);

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
