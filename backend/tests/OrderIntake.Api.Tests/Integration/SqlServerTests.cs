using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderIntake.Api.Tests.TestSupport;
using OrderIntake.Domain.Orders;
using OrderIntake.Infrastructure.Persistence;

namespace OrderIntake.Api.Tests.Integration;

public sealed class SqlServerFactAttribute : FactAttribute
{
    public SqlServerFactAttribute()
    {
        if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OrderIntake_TestSqlConnection")))
            Skip = "Set OrderIntake_TestSqlConnection to a SQL Server connection with database creation permission.";
    }
}

public class SqlServerTests
{
    private sealed class TestDatabase : IDisposable
    {
        public string ConnectionString { get; }
        public TestDatabase()
        {
            var connection = new SqlConnectionStringBuilder(Environment.GetEnvironmentVariable("OrderIntake_TestSqlConnection"))
            {
                InitialCatalog = "OrderIntakeTests_" + Guid.NewGuid().ToString("N")
            };
            ConnectionString = connection.ConnectionString;
            using var context = Open();
            context.Database.Migrate();
        }
        public OrderIntakeDbContext Open() => new(new DbContextOptionsBuilder<OrderIntakeDbContext>()
            .UseSqlServer(ConnectionString).Options);
        public void Dispose()
        {
            using var context = Open();
            context.Database.EnsureDeleted();
        }
    }

    private static Order Candidate(string reference) => Order.Create(reference,
        new Customer("buyer@example.com", "Buyer"), new[] { new LineItem("CODE", "Item", 2, 12.345678m) },
        "USD", null, DateTimeOffset.UtcNow);

    [SqlServerFact]
    public void MigrationPersistenceUniqueReferenceAndRowVersionWorkOnSqlServer()
    {
        using var database = new TestDatabase();
        Guid id;
        using (var context = database.Open())
        {
            var result = new EfOrderRepository(context).GetOrCreateByReference("PO-1", () => Candidate("PO-1"));
            id = result.Order.Id;
            Assert.True(result.WasCreated);
            Assert.False(context.Database.GetPendingMigrations().Any());
        }
        using var first = database.Open();
        using var second = database.Open();
        var firstRepository = new EfOrderRepository(first);
        var secondRepository = new EfOrderRepository(second);
        var firstOrder = firstRepository.GetById(id)!;
        var secondOrder = secondRepository.GetById(id)!;
        Assert.Equal(24.691356m, firstOrder.Total);
        Assert.Equal("buyer@example.com", firstOrder.Customer.Email);
        Assert.False(firstRepository.GetOrCreateByReference("po-1", () => Candidate("po-1")).WasCreated);
        Assert.Null(firstOrder.ChangeStatus(OrderStatus.Confirmed, DateTimeOffset.UtcNow));
        Assert.Null(secondOrder.ChangeStatus(OrderStatus.Cancelled, DateTimeOffset.UtcNow));
        Assert.True(firstRepository.TrySaveChanges());
        Assert.False(secondRepository.TrySaveChanges());
        using var fresh = database.Open();
        Assert.Equal(OrderStatus.Confirmed, fresh.Orders.Single().Status);
    }

    [SqlServerFact]
    public async Task ConcurrentInsertsReturnOneWinner()
    {
        using var database = new TestDatabase();
        using var barrier = new Barrier(2);
        Task<(Order Order, bool WasCreated)> Submit() => Task.Run(() =>
        {
            using var context = database.Open();
            return new EfOrderRepository(context).GetOrCreateByReference("RACE", () =>
            {
                Assert.True(barrier.SignalAndWait(TimeSpan.FromSeconds(30)));
                return Candidate("RACE");
            });
        });
        var results = await Task.WhenAll(Submit(), Submit());
        Assert.Single(results.Where(result => result.WasCreated));
        Assert.Equal(results[0].Order.Id, results[1].Order.Id);
        using var check = database.Open();
        Assert.Equal(1, check.Orders.Count());
    }

    [SqlServerFact]
    public async Task HttpContractPersistsOrdersAndReportsValidationAndConflicts()
    {
        using var database = new TestDatabase();
        using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder => builder
            .UseEnvironment("Development").UseSetting("ConnectionStrings:OrderIntake", database.ConnectionString));
        using var client = factory.CreateClient();
        var request = RequestFactory.ValidOrder();
        using var created = await client.PostAsJsonAsync("/api/orders", request);
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        var location = created.Headers.Location!;
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync(location)).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PostAsJsonAsync("/api/orders", request)).StatusCode);
        request.Customer.Email = "different@example.com";
        Assert.Equal(HttpStatusCode.Conflict, (await client.PostAsJsonAsync("/api/orders", request)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await client.PatchAsJsonAsync(location + "/status", new { status = "Fulfilled" })).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.PatchAsJsonAsync(location + "/status", new { status = "Confirmed" })).StatusCode);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PatchAsJsonAsync(location + "/status", new { status = 99 })).StatusCode);
        request.ExternalReference = "   ";
        Assert.Equal(HttpStatusCode.BadRequest, (await client.PostAsJsonAsync("/api/orders", request)).StatusCode);
        using var fresh = database.Open();
        Assert.Equal(OrderStatus.Confirmed, fresh.Orders.Single().Status);
    }
}
