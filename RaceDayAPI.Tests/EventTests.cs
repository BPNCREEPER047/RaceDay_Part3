using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDayAPI.Tests.Helpers;

namespace RaceDayAPI.Tests
{
    public class EventTests
    {
        private async Task<(HttpClient organiserClient, HttpClient participantClient)>
            SetupClientsAsync(string testName)
        {
            var factory = new TestWebAppFactory($"events_{testName}");

            var organiserClient = factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

            var participantClient = factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

            await TestHelpers.RegisterAndLoginAsync(
                organiserClient, "Org User", $"org_{testName}@test.com", "Pass1!", "Organiser");

            await TestHelpers.RegisterAndLoginAsync(
                participantClient, "Part User", $"part_{testName}@test.com", "Pass1!", "Participant");

            return (organiserClient, participantClient);
        }

        // ── Public access ──────────────────────────────────────────────────────

        [Fact]
        public async Task GetAllEvents_PublicAccess_Returns200()
        {
            var factory = new TestWebAppFactory("events_public");
            var client  = factory.CreateClient();

            var response = await client.GetAsync("/api/events");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetEventById_NonExistent_Returns404()
        {
            var factory = new TestWebAppFactory("events_404");
            var client  = factory.CreateClient();

            var response = await client.GetAsync("/api/events/9999");
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        // ── Create event ──────────────────────────────────────────────────────

        [Fact]
        public async Task CreateEvent_AsOrganiser_Returns201()
        {
            var (orgClient, _) = await SetupClientsAsync("create_ok");

            var response = await orgClient.PostAsJsonAsync("/api/events", new
            {
                eventName       = "Test Marathon",
                description     = "A test event",
                eventDate       = DateTime.UtcNow.AddMonths(2),
                location        = "Cape Town",
                distance        = 21.1,
                maxParticipants = 500,
                categoryID      = 1
            });

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task CreateEvent_AsParticipant_Returns403()
        {
            var (_, partClient) = await SetupClientsAsync("create_forbidden");

            var response = await partClient.PostAsJsonAsync("/api/events", new
            {
                eventName       = "Sneaky Event",
                eventDate       = DateTime.UtcNow.AddMonths(1),
                location        = "Somewhere",
                distance        = 10.0,
                maxParticipants = 100,
                categoryID      = 1
            });

            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task CreateEvent_Unauthenticated_Returns401()
        {
            var factory = new TestWebAppFactory("events_unauth_create");
            var client  = factory.CreateClient();

            var response = await client.PostAsJsonAsync("/api/events", new
            {
                eventName       = "Unauth Event",
                eventDate       = DateTime.UtcNow.AddMonths(1),
                location        = "Somewhere",
                distance        = 10.0,
                maxParticipants = 100,
                categoryID      = 1
            });

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ── Update / Delete ───────────────────────────────────────────────────

        [Fact]
        public async Task UpdateEvent_AsOrganiser_Returns200()
        {
            var (orgClient, _) = await SetupClientsAsync("update_ok");

            // Create first
            var createResp = await orgClient.PostAsJsonAsync("/api/events", new
            {
                eventName = "Update Me", eventDate = DateTime.UtcNow.AddMonths(3),
                location = "Joburg", distance = 10.0, maxParticipants = 100, categoryID = 1
            });
            createResp.StatusCode.Should().Be(HttpStatusCode.Created);
            var created = await createResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var id = Convert.ToInt32(created!["eventID"]);

            // Update
            var updateResp = await orgClient.PutAsJsonAsync($"/api/events/{id}", new
            {
                eventName = "Updated Name", eventDate = DateTime.UtcNow.AddMonths(3),
                location = "Durban", distance = 42.2, maxParticipants = 200, categoryID = 1
            });
            updateResp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task DeleteEvent_AsParticipant_Returns403()
        {
            var (orgClient, partClient) = await SetupClientsAsync("delete_forbidden");

            var createResp = await orgClient.PostAsJsonAsync("/api/events", new
            {
                eventName = "Delete Me", eventDate = DateTime.UtcNow.AddMonths(1),
                location = "CT", distance = 5.0, maxParticipants = 50, categoryID = 1
            });
            var created = await createResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var id = Convert.ToInt32(created!["eventID"]);

            var deleteResp = await partClient.DeleteAsync($"/api/events/{id}");
            deleteResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        // ── Enrolments for event ──────────────────────────────────────────────

        [Fact]
        public async Task GetEventEnrolments_AsParticipant_Returns403()
        {
            var (orgClient, partClient) = await SetupClientsAsync("enrol_forbidden");

            var createResp = await orgClient.PostAsJsonAsync("/api/events", new
            {
                eventName = "Enrol Test", eventDate = DateTime.UtcNow.AddMonths(1),
                location = "CT", distance = 5.0, maxParticipants = 50, categoryID = 1
            });
            var created = await createResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var id = Convert.ToInt32(created!["eventID"]);

            var response = await partClient.GetAsync($"/api/events/{id}/enrolments");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
