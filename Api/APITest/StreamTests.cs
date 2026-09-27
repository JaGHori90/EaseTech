using System.Net;
using System.Net.Http.Json;
using APITest.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace APITest
{
    [TestClass]
    public class StreamTests
    {
        private static EaseTechApiFactory _factory = null!;

        [ClassInitialize]
        public static void Setup(TestContext _) => _factory = new EaseTechApiFactory();

        [ClassCleanup]
        public static void Cleanup() => _factory.Dispose();

        [TestMethod]
        public async Task RegisterCall_AsEmployee_UsesOwnUserIdAndUpserts()
        {
            var employee = await _factory.LoginAsEmployeeAsync();
            var employeeId = await employee.GetOwnUserIdAsync();

            var first = await employee.PostAsJsonAsync("api/RegisterCall", new { userId = "fremde-id", callId = "call-aaaaaaaaaa", isAvailable = false });
            var second = await employee.PostAsJsonAsync("api/RegisterCall", new { userId = "fremde-id", callId = "call-bbbbbbbbbb", isAvailable = false });

            Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
            Assert.AreEqual(HttpStatusCode.OK, second.StatusCode);
            var calls = await _factory.WithDbAsync(db => db.CallStreams.Where(c => c.UserId == employeeId).ToListAsync());
            Assert.AreEqual(1, calls.Count, "Pro Mitarbeiter darf es nur einen Call-Eintrag geben");
            Assert.AreEqual("call-bbbbbbbbbb", calls[0].CallId);
            Assert.IsFalse(await _factory.WithDbAsync(db => db.CallStreams.AnyAsync(c => c.UserId == "fremde-id")));
        }

        [TestMethod]
        public async Task RegisterCall_AsCustomer_IsForbidden()
        {
            var customer = await _factory.LoginAsCustomerAsync();

            var response = await customer.PostAsJsonAsync("api/RegisterCall", new { callId = "call-cccccccccc", isAvailable = true });

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task Availability_And_Token_Flow()
        {
            var employee = await _factory.LoginAsEmployeeAsync();
            var register = await employee.PostAsJsonAsync("api/RegisterCall", new { callId = "call-dddddddddd", isAvailable = false });
            var id = (await register.ReadJsonAsync()).GetProperty("id").GetInt32();
            var anon = _factory.CreateClient();
            var customer = await _factory.LoginAsCustomerAsync();

            // Nicht verfügbar → kein Token
            Assert.IsFalse(await anon.GetFromJsonAsync<bool>("api/IsEmpAvailable"));
            Assert.AreEqual(HttpStatusCode.NotFound, (await customer.GetAsync("api/CreateToken")).StatusCode);

            // Mitarbeiter schaltet sich verfügbar
            var update = await employee.PutAsJsonAsync($"api/UpdateCallsStatus/{id}", new { callId = "call-dddddddddd", isAvailable = true });
            Assert.AreEqual(HttpStatusCode.OK, update.StatusCode);
            Assert.IsTrue(await anon.GetFromJsonAsync<bool>("api/IsEmpAvailable"));

            var tokenResponse = await customer.GetAsync("api/CreateToken");
            Assert.AreEqual(HttpStatusCode.OK, tokenResponse.StatusCode);
            var body = await tokenResponse.ReadJsonAsync();
            Assert.IsFalse(string.IsNullOrEmpty(body.GetProperty("token").GetString()));
            Assert.AreEqual("call-dddddddddd", body.GetProperty("callId").GetString());
            Assert.AreEqual("test-api-key", body.GetProperty("apiKey").GetString());
        }

        [TestMethod]
        public async Task UpdateCallStatus_OfOtherEmployee_IsForbidden()
        {
            var employee = await _factory.LoginAsEmployeeAsync();
            var register = await employee.PostAsJsonAsync("api/RegisterCall", new { callId = "call-eeeeeeeeee", isAvailable = false });
            var id = (await register.ReadJsonAsync()).GetProperty("id").GetInt32();

            var admin = await _factory.LoginAsAdminAsync();
            await admin.PostAsJsonAsync("api/signup", new
            {
                firstName = "Zweite", lastName = "Mitarbeiterin", email = "ma2@example.com", password = "Sicher!2345",
                role = "Employee", gender = "", birthday = "2000-01-01T00:00:00", address = "", city = "", zipCode = "", phoneNumber = ""
            });
            var otherEmployee = await _factory.LoginAsync("ma2@example.com", "Sicher!2345");

            var response = await otherEmployee.PutAsJsonAsync($"api/UpdateCallsStatus/{id}", new { callId = "x", isAvailable = true });

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }
    }
}
