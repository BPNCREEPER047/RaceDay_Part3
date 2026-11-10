using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RaceDayAPI.Data;

namespace RaceDayAPI.Tests.Helpers
{
    /// <summary>
    /// Creates an in-memory test server for integration tests.
    /// Each test class gets a fresh isolated database.
    /// </summary>
    public class TestWebAppFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbName;

        public TestWebAppFactory(string? dbName = null)
        {
            _dbName = dbName ?? Guid.NewGuid().ToString();
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Remove the real SQL Server DbContext
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                if (descriptor != null)
                    services.Remove(descriptor);

                // Replace with in-memory database
                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase(_dbName));
            });
        }
    }

    /// <summary>
    /// Shared helper methods used across test classes.
    /// </summary>
    public static class TestHelpers
    {
        /// <summary>
        /// POSTs to /api/auth/register and returns the response.
        /// </summary>
        public static async Task<HttpResponseMessage> RegisterAsync(
            HttpClient client,
            string fullName,
            string email,
            string password,
            string role)
        {
            var payload = new
            {
                fullName,
                email,
                password,
                role
            };
            return await client.PostAsJsonAsync("/api/auth/register", payload);
        }

        /// <summary>
        /// POSTs to /api/auth/login. The session cookie is automatically
        /// stored on the HttpClient by the test server's cookie container.
        /// </summary>
        public static async Task<HttpResponseMessage> LoginAsync(
            HttpClient client,
            string email,
            string password)
        {
            var payload = new { email, password };
            return await client.PostAsJsonAsync("/api/auth/login", payload);
        }

        /// <summary>
        /// Registers then immediately logs in. Leaves the client authenticated.
        /// </summary>
        public static async Task RegisterAndLoginAsync(
            HttpClient client,
            string fullName,
            string email,
            string password,
            string role)
        {
            await RegisterAsync(client, fullName, email, password, role);
            await LoginAsync(client, email, password);
        }
    }
}
