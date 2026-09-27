using System.Net;
using System.Net.Http.Json;
using Api.Enums;
using APITest.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace APITest
{
    [TestClass]
    public class RequestAndUserTests
    {
        private static EaseTechApiFactory _factory = null!;

        [ClassInitialize]
        public static void Setup(TestContext _) => _factory = new EaseTechApiFactory();

        [ClassCleanup]
        public static void Cleanup() => _factory.Dispose();

        [TestMethod]
        public async Task CreateRequest_Anonymous_ServerSetsStatusAndIgnoresEmployee()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/CreateRequest", new
            {
                name = "Kontakt Test",
                referrer = "Google",
                phoneNumber = "06641234567",
                message = "Bitte zurückrufen",
                date = "2020-01-01T00:00:00Z",
                contactStatus = "Gelöst",
                employeeId = ""
            });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var request = await _factory.WithDbAsync(db => db.UserRequests.SingleAsync(r => r.Name == "Kontakt Test"));
            Assert.AreEqual(ContactStatus.Unberührt, request.ContactStatus);
            Assert.IsNull(request.EmployeeId);
            Assert.IsTrue(request.Date > DateTime.UtcNow.AddMinutes(-5));
        }

        [TestMethod]
        public async Task CreateRequest_WithoutPhoneNumber_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/CreateRequest", new { name = "Ohne Nummer", phoneNumber = "" });

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateRequest_AsEmployee_AssignsEmployeeAndStatus()
        {
            var anon = _factory.CreateClient();
            await anon.PostAsJsonAsync("api/CreateRequest", new { name = "Update Test", phoneNumber = "06641111111", message = "Hi" });
            var id = await _factory.WithDbAsync(db => db.UserRequests.Where(r => r.Name == "Update Test").Select(r => r.Id).SingleAsync());
            var employee = await _factory.LoginAsEmployeeAsync();
            var employeeId = await employee.GetOwnUserIdAsync();

            var response = await employee.PutAsJsonAsync($"api/UpdateRequestById/{id}", new
            {
                name = "Update Test",
                phoneNumber = "06641111111",
                message = "Hi",
                date = DateTime.UtcNow,
                contactStatus = "Kontaktiert",
                employeeId
            });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var request = await _factory.WithDbAsync(db => db.UserRequests.SingleAsync(r => r.Id == id));
            Assert.AreEqual(ContactStatus.Kontaktiert, request.ContactStatus);
            Assert.AreEqual(employeeId, request.EmployeeId);
        }

        [TestMethod]
        public async Task UpdateOwnProfile_Succeeds()
        {
            var customer = await _factory.LoginAsCustomerAsync();
            var id = await customer.GetOwnUserIdAsync();

            var response = await customer.PutAsJsonAsync($"api/updateUserById/{id}", new
            {
                firstName = "Anna",
                lastName = "Schmidt",
                city = "Linz",
                birthday = "1998-07-15T00:00:00"
            });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var user = await _factory.WithDbAsync(db => db.Users.SingleAsync(u => u.Id == id));
            Assert.AreEqual("Linz", user.City);
        }

        [TestMethod]
        public async Task UpdateOtherProfile_AsCustomer_IsForbidden()
        {
            var customer = await _factory.LoginAsCustomerAsync();
            var employee = await _factory.LoginAsEmployeeAsync();
            var employeeId = await employee.GetOwnUserIdAsync();

            var response = await customer.PutAsJsonAsync($"api/updateUserById/{employeeId}", new
            {
                firstName = "Gehackt",
                birthday = "2000-01-01T00:00:00"
            });

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task GetProfileOfEmployee_AsCustomer_ReturnsOnlyName()
        {
            var customer = await _factory.LoginAsCustomerAsync();
            var employee = await _factory.LoginAsEmployeeAsync();
            var employeeId = await employee.GetOwnUserIdAsync();

            var response = await customer.GetAsync($"api/GetUserProfileById/{employeeId}");
            var body = await response.ReadJsonAsync();

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("Andrea", body.GetProperty("firstName").GetString());
            Assert.IsFalse(body.TryGetProperty("email", out _), "E-Mail darf nicht sichtbar sein");
            Assert.IsFalse(body.TryGetProperty("address", out _), "Adresse darf nicht sichtbar sein");
        }

        [TestMethod]
        public async Task GetProfileOfOtherCustomer_AsCustomer_IsForbidden()
        {
            var admin = await _factory.LoginAsAdminAsync();
            var customer = await _factory.LoginAsCustomerAsync();
            var customers = await admin.GetFromJsonAsync<List<System.Text.Json.JsonElement>>("api/GetAllCustomer");
            var ownId = await customer.GetOwnUserIdAsync();
            var other = customers!.Select(c => c.GetProperty("id").GetString()).FirstOrDefault(id => id != ownId);
            if (other == null)
            {
                await _factory.CreateClient().PostAsJsonAsync("api/signup", new
                {
                    firstName = "Zweiter", lastName = "Kunde", email = "zweiter@example.com", password = "Sicher!2345",
                    role = "Customer", gender = "", birthday = "2000-01-01T00:00:00", address = "", city = "", zipCode = "", phoneNumber = ""
                });
                other = await _factory.WithDbAsync(db => db.Users.Where(u => u.Email == "zweiter@example.com").Select(u => u.Id).SingleAsync());
            }

            var response = await customer.GetAsync($"api/GetUserProfileById/{other}");

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteUser_WithOpenOrder_ReturnsBadRequest()
        {
            var anon = _factory.CreateClient();
            await anon.PostAsJsonAsync("api/signup", new
            {
                firstName = "Loesch", lastName = "Mich", email = "loesch@example.com", password = "Sicher!2345",
                role = "Customer", gender = "", birthday = "2000-01-01T00:00:00", address = "", city = "", zipCode = "", phoneNumber = ""
            });
            var client = await _factory.LoginAsync("loesch@example.com", "Sicher!2345");
            var serviceId = await _factory.WithDbAsync(db => db.Services.Select(s => s.ServiceId).FirstAsync());
            (await client.PostAsJsonAsync("api/CreateOrder", new
            {
                serviceId, timeOfService = 10, orderStatus = "Angerichtet", paymentMethod = "Bar", totalCost = 0
            })).EnsureSuccessStatusCode();

            var response = await client.PostAsJsonAsync("api/deleteUser", new { password = "Sicher!2345" });

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task DeleteUser_WithoutOpenOrders_Succeeds()
        {
            var anon = _factory.CreateClient();
            await anon.PostAsJsonAsync("api/signup", new
            {
                firstName = "Weg", lastName = "Damit", email = "weg@example.com", password = "Sicher!2345",
                role = "Customer", gender = "", birthday = "2000-01-01T00:00:00", address = "", city = "", zipCode = "", phoneNumber = ""
            });
            var client = await _factory.LoginAsync("weg@example.com", "Sicher!2345");

            var response = await client.PostAsJsonAsync("api/deleteUser", new { password = "Sicher!2345" });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.IsFalse(await _factory.WithDbAsync(db => db.Users.AnyAsync(u => u.Email == "weg@example.com")));
        }
    }
}
