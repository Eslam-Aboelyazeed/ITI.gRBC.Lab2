namespace ITI.gRBC.Client.Services
{
    public class ApiKeyProviderService : IApiKeyProviderService
    {
        private readonly IConfiguration configuration;

        public ApiKeyProviderService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public string? GetApiKey()
        {
            return configuration.GetSection(Consts.ApiKeySettingName).Value;
        }
    }
}
