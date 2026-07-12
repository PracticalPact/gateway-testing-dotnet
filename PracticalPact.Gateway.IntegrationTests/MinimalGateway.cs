using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace IntegrationTests;

public sealed class MinimalGateway
{
    public static WebApplication Create(Action<WebApplicationBuilder>? configureBuilder = null)
    {
        var builder = WebApplication.CreateBuilder();
        configureBuilder?.Invoke(builder);
        var app = builder.Build();
        app.Map("/{**path}", async (HttpContext context) =>
        {
            using HttpClient client = new();
            var response = await client.GetAsync("https://example.com");
            var content = await response.Content.ReadAsStringAsync();
            
            context.Response.StatusCode = (int)response.StatusCode;
            await response.Content.CopyToAsync(context.Response.Body);
            
            foreach (var header in response.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }

            foreach (var header in response.Content.Headers)
            {
                context.Response.Headers[header.Key] = header.Value.ToArray();
            }
        });
        
        return app;
    }
}
