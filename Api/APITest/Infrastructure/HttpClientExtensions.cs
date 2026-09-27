using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace APITest.Infrastructure
{
    public static class HttpClientExtensions
    {
        public static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);

        public static async Task<HttpClient> LoginAsync(this EaseTechApiFactory factory, string email, string password)
        {
            var client = factory.CreateClient();
            var response = await client.PostAsJsonAsync("api/signin", new { email, password });
            response.EnsureSuccessStatusCode();

            var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
            var token = body.GetProperty("token").GetString();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            return client;
        }

        public static Task<HttpClient> LoginAsAdminAsync(this EaseTechApiFactory f) =>
            f.LoginAsync(EaseTechApiFactory.AdminEmail, EaseTechApiFactory.AdminPassword);

        public static Task<HttpClient> LoginAsEmployeeAsync(this EaseTechApiFactory f) =>
            f.LoginAsync(EaseTechApiFactory.EmployeeEmail, EaseTechApiFactory.EmployeePassword);

        public static Task<HttpClient> LoginAsCustomerAsync(this EaseTechApiFactory f) =>
            f.LoginAsync(EaseTechApiFactory.CustomerEmail, EaseTechApiFactory.CustomerPassword);

        public static async Task<string> GetOwnUserIdAsync(this HttpClient client)
        {
            var profile = await client.GetFromJsonAsync<JsonElement>("api/UserProfile", Json);
            return profile.GetProperty("id").GetString()!;
        }

        public static async Task<JsonElement> ReadJsonAsync(this HttpResponseMessage response) =>
            await response.Content.ReadFromJsonAsync<JsonElement>(Json);
    }
}
