
using ITI.gRBC.Client.Protos;
using ITI.gRBC.Client.Services;

namespace ITI.gRBC.Client
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();

            builder.Services.AddScoped<IApiKeyProviderService, ApiKeyProviderService>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddGrpcClient<InventoryService.InventoryServiceClient>(options =>
            {
                options.Address = new Uri(builder.Configuration.GetValue<string>("GRPCURL") ?? "https://localhost:7246");
            }).AddCallCredentials((context, metadata, serviceProvider) =>
            {
                var apiKeyProvider = serviceProvider.GetRequiredService<IApiKeyProviderService>();
                var apiKey = apiKeyProvider.GetApiKey();
                metadata.Add(Consts.ApiKeyHeaderName, apiKey?? "");
                return Task.CompletedTask;
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}
