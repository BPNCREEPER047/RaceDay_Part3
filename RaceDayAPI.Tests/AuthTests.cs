using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDayAPI.Tests.Helpers;

namespace RaceDayAPI.Tests
{
    public class AuthTests : IClassFixture<TestWebAppFactory>
    {
        private readonly HttpClient _client;

        public AuthTests(TestWebAppFactory factory)
        {
            _client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                HandleCookies = true  // Maintains session cookies between requests
            });
        }

        // ── Registration ──────────────────────────────────────────────────────

        [Fact]
        public async Task Register_WithValidOrganiser_Returns201()
        {
            var response = await TestHelpers.RegisterAsync(
                _client, "Test Organiser", "organiser@test.com", "Password1!", "Organiser");

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Register_WithValidParticipant_Returns201()
        {
            var response = await TestHelpers.RegisterAsync(
                _client, "Test Participant", "participant@test.com", "Password1!", "Participant");

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task Register_WithInvalidRole_Returns400()
        {
            var response = await TestHelpers.RegisterAsync(
                _client, "Bad Role", "badrole@test.com", "Password1!", "Admin");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Register_WithDuplicateEmail_Returns409()
        {
            await TestHelpers.RegisterAsync(
                _client, "First User", "dup@test.com", "Password1!", "Participant");

            var response = await TestHelpers.RegisterAsync(
                _client, "Second User", "dup@test.com", "Password1!", "Participant");

            response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task Register_WithMissingFields_Returns400()
        {
            var response = await _client.PostAsJsonAsync("/api/auth/register", new { email = "missing@test.com" });
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        // ── Login ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Login_WithValidCredentials_Returns200AndSessionCookie()
        {
            await TestHelpers.RegisterAsync(
                _client, "Login User", "login@test.com", "Password1!", "Participant");

            var response = await TestHelpers.LoginAsync(_client, "login@test.com", "Password1!");

            response.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await response.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            body.Should().ContainKey("message");
        }

        [Fact]
        public async Task Login_WithWrongPassword_Returns401()
        {
            await TestHelpers.RegisterAsync(
                _client, "Wrong Pass", "wrong@test.com", "CorrectPass1!", "Participant");

            var response = await TestHelpers.LoginAsync(_client, "wrong@test.com", "WrongPass!");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Login_WithUnknownEmail_Returns401()
        {
            var response = await TestHelpers.LoginAsync(_client, "nobody@test.com", "Password1!");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ── Profile requires auth ─────────────────────────────────────────────

        [Fact]
        public async Task GetProfile_WithoutLogin_Returns401()
        {
            var freshClient = new TestWebAppFactory("profile_unauth_test")
                .CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
                {
                    HandleCookies = true
                });

            var response = await freshClient.GetAsync("/api/users/profile");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task GetProfile_AfterLogin_Returns200()
        {
            var factory = new TestWebAppFactory("profile_auth_test");
            var client  = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                HandleCookies = true
            });

            await TestHelpers.RegisterAndLoginAsync(
                client, "Profile User", "profile@test.com", "Password1!", "Participant");

            var response = await client.GetAsync("/api/users/profile");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }
    }
}
