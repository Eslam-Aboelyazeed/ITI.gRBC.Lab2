using ITI.gRBC.Server.Handlers;
using ITI.gRBC.Server.Protos;
using ITI.gRBC.Server.Services;
using Microsoft.AspNetCore.Authentication;

namespace ITI.gRBC.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();

            // Add services to the container.
            builder.Services.AddGrpc();

            builder.Services.AddScoped<IApiKeyAuthService, ApiKeyAuthService>();

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = Consts.ApiKeySchemeName;
            }).AddScheme<AuthenticationSchemeOptions, ApiKeyAuthHandler>(Consts.ApiKeySchemeName, configureOptions => { });

            builder.Services.AddAuthorization();
            //builder.Services.AddSingleton<List<Product>>(new List<Product>());

            var app = builder.Build();

            // Configure the HTTP request pipeline.

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapGrpcService<InventoryServiceClass>();

            app.Run();
        }
    }
}
