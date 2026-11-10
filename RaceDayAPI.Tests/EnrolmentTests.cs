using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using RaceDayAPI.Tests.Helpers;

namespace RaceDayAPI.Tests
{
    public class EnrolmentTests
    {
        private async Task<(HttpClient orgClient, HttpClient partClient, int eventId)>
            SetupAsync(string testName)
        {
            var factory = new TestWebAppFactory($"enrolments_{testName}");

            var orgClient = factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

            var partClient = factory.CreateClient(
                new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

            await TestHelpers.RegisterAndLoginAsync(
                orgClient, "Org", $"org_{testName}@test.com", "Pass1!", "Organiser");

            await TestHelpers.RegisterAndLoginAsync(
                partClient, "Part", $"part_{testName}@test.com", "Pass1!", "Participant");

            // Create an event as organiser
            var ev = await orgClient.PostAsJsonAsync("/api/events", new
            {
                eventName = "Enrolment Event", eventDate = DateTime.UtcNow.AddMonths(2),
                location = "Cape Town", distance = 10.0, maxParticipants = 100, categoryID = 1
            });
            var evData = await ev.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var eventId = Convert.ToInt32(evData!["eventID"]);

            return (orgClient, partClient, eventId);
        }

        // ── GET /api/enrolments ───────────────────────────────────────────────

        [Fact]
        public async Task GetMyEnrolments_AsParticipant_Returns200()
        {
            var (_, partClient, _) = await SetupAsync("get_my");

            var response = await partClient.GetAsync("/api/enrolments");
            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task GetMyEnrolments_AsOrganiser_Returns403()
        {
            var (orgClient, _, _) = await SetupAsync("get_org_forbidden");

            var response = await orgClient.GetAsync("/api/enrolments");
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task GetMyEnrolments_Unauthenticated_Returns401()
        {
            var factory = new TestWebAppFactory("enrolments_unauth");
            var client  = factory.CreateClient();

            var response = await client.GetAsync("/api/enrolments");
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        // ── POST /api/enrolments ──────────────────────────────────────────────

        [Fact]
        public async Task EnrolInEvent_AsParticipant_Returns201()
        {
            var (_, partClient, eventId) = await SetupAsync("enrol_success");

            var response = await partClient.PostAsJsonAsync("/api/enrolments", new { eventID = eventId });
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        [Fact]
        public async Task EnrolInEvent_AsOrganiser_Returns403()
        {
            var (orgClient, _, eventId) = await SetupAsync("enrol_org_forbidden");

            var response = await orgClient.PostAsJsonAsync("/api/enrolments", new { eventID = eventId });
            response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }

        [Fact]
        public async Task EnrolInEvent_Twice_Returns409()
        {
            var (_, partClient, eventId) = await SetupAsync("enrol_dup");

            await partClient.PostAsJsonAsync("/api/enrolments", new { eventID = eventId });
            var second = await partClient.PostAsJsonAsync("/api/enrolments", new { eventID = eventId });

            second.StatusCode.Should().Be(HttpStatusCode.Conflict);
        }

        [Fact]
        public async Task EnrolInEvent_NonExistentEvent_Returns404()
        {
            var (_, partClient, _) = await SetupAsync("enrol_notfound");

            var response = await partClient.PostAsJsonAsync("/api/enrolments", new { eventID = 9999 });
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task EnrolInEvent_EnrolmentIsRecordedCorrectly()
        {
            var (_, partClient, eventId) = await SetupAsync("enrol_recorded");

            await partClient.PostAsJsonAsync("/api/enrolments", new { eventID = eventId });

            var listResponse = await partClient.GetAsync("/api/enrolments");
            listResponse.StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await listResponse.Content.ReadFromJsonAsync<List<Dictionary<string, object>>>();
            body.Should().NotBeNullOrEmpty();
            body!.Should().Contain(e => Convert.ToInt32(e["eventID"]) == eventId);
        }

        // ── DELETE /api/enrolments/{id} ───────────────────────────────────────

        [Fact]
        public async Task CancelEnrolment_OwnEnrolment_Returns200()
        {
            var (_, partClient, eventId) = await SetupAsync("cancel_own");

            var enrolResp = await partClient.PostAsJsonAsync("/api/enrolments", new { eventID = eventId });
            var enrolData = await enrolResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var enrolId   = Convert.ToInt32(enrolData!["enrolmentID"]);

            var cancelResp = await partClient.DeleteAsync($"/api/enrolments/{enrolId}");
            cancelResp.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Fact]
        public async Task CancelEnrolment_OtherParticipantEnrolment_Returns403()
        {
            var factory = new TestWebAppFactory("cancel_other");

            var orgClient   = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });
            var part1Client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });
            var part2Client = factory.CreateClient(new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions { HandleCookies = true });

            await TestHelpers.RegisterAndLoginAsync(orgClient,   "Org",   "org_co@test.com",   "Pass1!", "Organiser");
            await TestHelpers.RegisterAndLoginAsync(part1Client, "P1",    "p1_co@test.com",    "Pass1!", "Participant");
            await TestHelpers.RegisterAndLoginAsync(part2Client, "P2",    "p2_co@test.com",    "Pass1!", "Participant");

            var ev = await orgClient.PostAsJsonAsync("/api/events", new
            {
                eventName = "Cancel Test", eventDate = DateTime.UtcNow.AddMonths(1),
                location = "CT", distance = 5.0, maxParticipants = 50, categoryID = 1
            });
            var evData  = await ev.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var eventId = Convert.ToInt32(evData!["eventID"]);

            var enrolResp = await part1Client.PostAsJsonAsync("/api/enrolments", new { eventID = eventId });
            var enrolData = await enrolResp.Content.ReadFromJsonAsync<Dictionary<string, object>>();
            var enrolId   = Convert.ToInt32(enrolData!["enrolmentID"]);

            // P2 tries to cancel P1's enrolment
            var cancelResp = await part2Client.DeleteAsync($"/api/enrolments/{enrolId}");
            cancelResp.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        }
    }
}
