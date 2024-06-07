namespace ITI.gRBC.Server.Services
{
    public class ApiKeyAuthService : IApiKeyAuthService
    {
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IConfiguration configuration;

        public ApiKeyAuthService(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            this.httpContextAccessor = httpContextAccessor;
            this.configuration = configuration;
        }

        public bool Authenticate()
        {
            var context = httpContextAccessor.HttpContext;

            if (context != null)
            {
                if (context.Request.Headers.TryGetValue(Consts.ApiKeyHeaderName, out var apiKey))
                {
                    var apiActualKey = configuration.GetSection(Consts.ApiKeySettingsKey).Value;

                    return apiActualKey == apiKey;
                }
            }

            return false;
        }
    }
}
