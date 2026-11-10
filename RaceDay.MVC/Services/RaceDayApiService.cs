using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using RaceDay.MVC.Models;

namespace RaceDay.MVC.Services
{
    public interface IRaceDayApiService
    {
        // Auth
        Task<AuthResponseDto?> RegisterAsync(RegisterViewModel vm);
        Task<AuthResponseDto?> LoginAsync(LoginViewModel vm);
        Task LogoutAsync();

        // Profile
        Task<UserDto?> GetProfileAsync();
        Task<bool> UpdateProfileAsync(UpdateProfileDto dto);

        // Categories
        Task<List<CategoryDto>> GetCategoriesAsync();
        Task<CategoryDto?> CreateCategoryAsync(CreateCategoryViewModel vm);
        Task<bool> DeleteCategoryAsync(int id);

        // Events
        Task<List<EventDto>> GetEventsAsync();
        Task<EventDto?> GetEventAsync(int id);
        Task<EventDto?> CreateEventAsync(CreateEventApiDto dto);
        Task<EventDto?> UpdateEventAsync(int id, CreateEventApiDto dto);
        Task<bool> DeleteEventAsync(int id);
        Task<List<EnrolmentDto>> GetEventEnrolmentsAsync(int eventId);
        Task<bool> UpdateBannerAsync(int eventId, string? imageUrl);

        // Enrolments
        Task<List<EnrolmentDto>> GetMyEnrolmentsAsync();
        Task<EnrolmentDto?> EnrolAsync(int eventId);
        Task<bool> CancelEnrolmentAsync(int enrolmentId);

        // Results
        Task<List<ResultDto>> GetMyResultsAsync();
        Task<List<ResultDto>> GetEventResultsAsync(int eventId);
        Task<ResultDto?> CreateResultAsync(CreateResultViewModel vm);
        Task<ResultDto?> UpdateResultAsync(int id, UpdateResultApiDto dto);
        Task<bool> DeleteResultAsync(int id);

        // Azure Blob (via API)
        Task<string?> UploadEventBannerAsync(int eventId, IFormFile file);
        Task<string?> UploadProfilePictureAsync(IFormFile file);
    }

    /// <summary>
    /// All HTTP calls to the Part 2 API.
    /// Authentication is COOKIE-BASED (session): the HttpClient
    /// automatically sends the .RaceDay.Session cookie on every request.
    /// NO Bearer tokens are used.
    /// </summary>
    public class RaceDayApiService : IRaceDayApiService
    {
        private readonly HttpClient _http;
        private readonly ILogger<RaceDayApiService> _log;

        private static readonly JsonSerializerOptions JsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public RaceDayApiService(HttpClient http, ILogger<RaceDayApiService> log)
        {
            _http = http;
            _log  = log;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private StringContent Json(object body) =>
            new(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");

        private async Task<T?> ReadJson<T>(HttpResponseMessage resp)
        {
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync();
                _log.LogWarning("API {status}: {body}", (int)resp.StatusCode, err);
                return default;
            }
            var json = await resp.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<T>(json, JsonOpts);
        }

        // ── Auth ─────────────────────────────────────────────────────────────

        public async Task<AuthResponseDto?> RegisterAsync(RegisterViewModel vm)
        {
            var resp = await _http.PostAsync("api/auth/register", Json(new
            {
                fullName = vm.FullName,
                email    = vm.Email,
                password = vm.Password,
                role     = vm.Role
            }));

            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();

            // API returns { message, user }
            var reg = JsonSerializer.Deserialize<ApiUserWrapper>(json, JsonOpts);
            if (reg?.User == null) return null;

            return new AuthResponseDto { User = reg.User };
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginViewModel vm)
        {
            var resp = await _http.PostAsync("api/auth/login", Json(new
            {
                email    = vm.Email,
                password = vm.Password
            }));

            if (!resp.IsSuccessStatusCode) return null;
            var json = await resp.Content.ReadAsStringAsync();

            // API returns { message, user }
            var login = JsonSerializer.Deserialize<ApiUserWrapper>(json, JsonOpts);
            if (login?.User == null) return null;

            return new AuthResponseDto { User = login.User };
        }

        public async Task LogoutAsync()
        {
            await _http.PostAsync("api/auth/logout", Json(new { }));
        }

        // ── Profile ──────────────────────────────────────────────────────────

        public async Task<UserDto?> GetProfileAsync()
        {
            var resp = await _http.GetAsync("api/users/profile");
            return await ReadJson<UserDto>(resp);
        }

        public async Task<bool> UpdateProfileAsync(UpdateProfileDto dto)
        {
            var resp = await _http.PutAsync("api/users/profile", Json(dto));
            return resp.IsSuccessStatusCode;
        }

        // ── Categories ───────────────────────────────────────────────────────

        public async Task<List<CategoryDto>> GetCategoriesAsync()
        {
            var resp = await _http.GetAsync("api/categories");
            return await ReadJson<List<CategoryDto>>(resp) ?? new();
        }

        public async Task<CategoryDto?> CreateCategoryAsync(CreateCategoryViewModel vm)
        {
            var resp = await _http.PostAsync("api/categories", Json(new
            {
                categoryName = vm.CategoryName,
                description  = vm.Description
            }));
            return await ReadJson<CategoryDto>(resp);
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var resp = await _http.DeleteAsync($"api/categories/{id}");
            return resp.IsSuccessStatusCode;
        }

        // ── Events ───────────────────────────────────────────────────────────

        public async Task<List<EventDto>> GetEventsAsync()
        {
            var resp = await _http.GetAsync("api/events");
            return await ReadJson<List<EventDto>>(resp) ?? new();
        }

        public async Task<EventDto?> GetEventAsync(int id)
        {
            var resp = await _http.GetAsync($"api/events/{id}");
            return await ReadJson<EventDto>(resp);
        }

        public async Task<EventDto?> CreateEventAsync(CreateEventApiDto dto)
        {
            var resp = await _http.PostAsync("api/events", Json(dto));
            return await ReadJson<EventDto>(resp);
        }

        public async Task<EventDto?> UpdateEventAsync(int id, CreateEventApiDto dto)
        {
            var resp = await _http.PutAsync($"api/events/{id}", Json(dto));
            return await ReadJson<EventDto>(resp);
        }

        public async Task<bool> DeleteEventAsync(int id)
        {
            var resp = await _http.DeleteAsync($"api/events/{id}");
            return resp.IsSuccessStatusCode;
        }

        public async Task<List<EnrolmentDto>> GetEventEnrolmentsAsync(int eventId)
        {
            var resp = await _http.GetAsync($"api/events/{eventId}/enrolments");
            return await ReadJson<List<EnrolmentDto>>(resp) ?? new();
        }

        public async Task<bool> UpdateBannerAsync(int eventId, string? imageUrl)
        {
            var resp = await _http.PostAsync($"api/events/{eventId}/banner", Json(new { imageUrl }));
            return resp.IsSuccessStatusCode;
        }

        // ── Enrolments ───────────────────────────────────────────────────────

        public async Task<List<EnrolmentDto>> GetMyEnrolmentsAsync()
        {
            var resp = await _http.GetAsync("api/enrolments");
            return await ReadJson<List<EnrolmentDto>>(resp) ?? new();
        }

        public async Task<EnrolmentDto?> EnrolAsync(int eventId)
        {
            var resp = await _http.PostAsync("api/enrolments", Json(new { eventID = eventId }));
            return await ReadJson<EnrolmentDto>(resp);
        }

        public async Task<bool> CancelEnrolmentAsync(int enrolmentId)
        {
            var resp = await _http.DeleteAsync($"api/enrolments/{enrolmentId}");
            return resp.IsSuccessStatusCode;
        }

        // ── Results ──────────────────────────────────────────────────────────

        public async Task<List<ResultDto>> GetMyResultsAsync()
        {
            var resp = await _http.GetAsync("api/results");
            return await ReadJson<List<ResultDto>>(resp) ?? new();
        }

        public async Task<List<ResultDto>> GetEventResultsAsync(int eventId)
        {
            var resp = await _http.GetAsync($"api/results/event/{eventId}");
            return await ReadJson<List<ResultDto>>(resp) ?? new();
        }

        public async Task<ResultDto?> CreateResultAsync(CreateResultViewModel vm)
        {
            var resp = await _http.PostAsync("api/results", Json(new
            {
                enrolmentID = vm.EnrolmentID,
                finishTime  = vm.FinishTime,
                position    = vm.Position,
                notes       = vm.Notes
            }));
            return await ReadJson<ResultDto>(resp);
        }

        public async Task<ResultDto?> UpdateResultAsync(int id, UpdateResultApiDto dto)
        {
            var resp = await _http.PutAsync($"api/results/{id}", Json(dto));
            return await ReadJson<ResultDto>(resp);
        }

        public async Task<bool> DeleteResultAsync(int id)
        {
            var resp = await _http.DeleteAsync($"api/results/{id}");
            return resp.IsSuccessStatusCode;
        }

        // ── File uploads (multipart → API) ───────────────────────────────────

        public async Task<string?> UploadEventBannerAsync(int eventId, IFormFile file)
        {
            using var form   = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fc = new StreamContent(stream);
            fc.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(fc, "file", file.FileName);

            var resp = await _http.PostAsync($"api/events/{eventId}/banner-upload", form);
            if (!resp.IsSuccessStatusCode) return null;

            var json   = await resp.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOpts);
            return result?.GetValueOrDefault("imageUrl");
        }

        public async Task<string?> UploadProfilePictureAsync(IFormFile file)
        {
            using var form   = new MultipartFormDataContent();
            using var stream = file.OpenReadStream();
            var fc = new StreamContent(stream);
            fc.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            form.Add(fc, "file", file.FileName);

            var resp = await _http.PostAsync("api/users/profile/picture", form);
            if (!resp.IsSuccessStatusCode) return null;

            var json   = await resp.Content.ReadAsStringAsync();
            var result = JsonSerializer.Deserialize<Dictionary<string, string>>(json, JsonOpts);
            return result?.GetValueOrDefault("imageUrl");
        }
    }

    // Internal wrapper to deserialise { message, user } responses
    internal class ApiUserWrapper
    {
        public string? Message { get; set; }
        public UserDto? User { get; set; }
    }
}
