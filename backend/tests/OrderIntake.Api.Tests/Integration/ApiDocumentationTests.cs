using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OrderIntake.Api.Tests.Integration;

public class ApiDocumentationTests
{
    [Fact]
    public async Task SwaggerDescribesOrdersAndServesUi()
    {
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .UseEnvironment("Development")
            .UseSetting("ConnectionStrings:OrderIntake", "Server=DESKTOP-F56KH6K\\SQLEXPRESS;Database=OrderIntake;Trusted_Connection=True;TrustServerCertificate=True;"));
        using var client = factory.CreateClient();
        using var response = await client.GetAsync("/swagger/v1/swagger.json");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty("/api/orders", out _));
        Assert.True(document.RootElement.GetProperty("paths").TryGetProperty("/api/orders/{id}/status", out _));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/swagger/index.html")).StatusCode);
    }
}
