using System.Net;
using System.Net.Http.Json;
using APITest.Infrastructure;

namespace APITest
{
    [TestClass]
    public class AuthTests
    {
        private static EaseTechApiFactory _factory = null!;

        [ClassInitialize]
        public static void Setup(TestContext _) => _factory = new EaseTechApiFactory();

        [ClassCleanup]
        public static void Cleanup() => _factory.Dispose();

        private static object NewUser(string email, string role = "Customer", string password = "Sicher!2345") => new
        {
            firstName = "Test",
            lastName = "Person",
            email,
            password,
            role,
            gender = "",
            birthday = "2000-01-01T00:00:00",
            address = "",
            city = "",
            zipCode = "",
            phoneNumber = ""
        };

        [TestMethod]
        public async Task SignIn_WithValidCredentials_ReturnsTokenAndRole()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/signin",
                new { email = EaseTechApiFactory.CustomerEmail, password = EaseTechApiFactory.CustomerPassword });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var body = await response.ReadJsonAsync();
            Assert.IsFalse(string.IsNullOrEmpty(body.GetProperty("token").GetString()));
            Assert.AreEqual("Customer", body.GetProperty("role").GetString());
        }

        [TestMethod]
        public async Task SignIn_WithWrongPassword_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/signin",
                new { email = EaseTechApiFactory.CustomerEmail, password = "falsch" });

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SignIn_WithUnknownEmail_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/signin",
                new { email = "gibtsnicht@example.com", password = "egal" });

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SignIn_TooManyFailedAttempts_LocksAccount()
        {
            var client = _factory.CreateClient();
            await client.PostAsJsonAsync("api/signup", NewUser("lockout@example.com"));

            for (int i = 0; i < 5; i++)
            {
                await client.PostAsJsonAsync("api/signin", new { email = "lockout@example.com", password = "falsch" });
            }

            // Auch mit richtigem Passwort bleibt das Konto gesperrt
            var response = await client.PostAsJsonAsync("api/signin", new { email = "lockout@example.com", password = "Sicher!2345" });
            var body = await response.ReadJsonAsync();

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
            StringAssert.Contains(body.GetProperty("message").GetString(), "locked");
        }

        [TestMethod]
        public async Task SignUp_Anonymous_AsCustomer_Succeeds()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/signup", NewUser("neukunde@example.com"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var login = await client.PostAsJsonAsync("api/signin", new { email = "neukunde@example.com", password = "Sicher!2345" });
            Assert.AreEqual("Customer", (await login.ReadJsonAsync()).GetProperty("role").GetString());
        }

        [TestMethod]
        public async Task SignUp_WithoutRole_DefaultsToCustomer()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/signup", NewUser("ohnerolle@example.com", role: ""));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var login = await client.PostAsJsonAsync("api/signin", new { email = "ohnerolle@example.com", password = "Sicher!2345" });
            Assert.AreEqual("Customer", (await login.ReadJsonAsync()).GetProperty("role").GetString());
        }

        [TestMethod]
        [DataRow("Admin")]
        [DataRow("Employee")]
        public async Task SignUp_Anonymous_AsPrivilegedRole_IsForbidden(string role)
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/signup", NewUser($"evil-{role}@example.com", role));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task SignUp_Employee_CannotCreateAdmin()
        {
            var client = await _factory.LoginAsEmployeeAsync();

            var response = await client.PostAsJsonAsync("api/signup", NewUser("neueradmin@example.com", "Admin"));

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task SignUp_Admin_CanCreateEmployee()
        {
            var client = await _factory.LoginAsAdminAsync();

            var response = await client.PostAsJsonAsync("api/signup", NewUser("mitarbeiter2@example.com", "Employee"));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [TestMethod]
        public async Task SignUp_InvalidRole_ReturnsBadRequest()
        {
            var client = await _factory.LoginAsAdminAsync();

            var response = await client.PostAsJsonAsync("api/signup", NewUser("superuser@example.com", "SuperUser"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SignUp_WeakPassword_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/signup", NewUser("schwach@example.com", password: "abc"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task SignUp_DuplicateEmail_ReturnsBadRequest()
        {
            var client = _factory.CreateClient();

            var response = await client.PostAsJsonAsync("api/signup", NewUser(EaseTechApiFactory.CustomerEmail));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task ChangePassword_WithCorrectCurrentPassword_Succeeds()
        {
            var anon = _factory.CreateClient();
            await anon.PostAsJsonAsync("api/signup", NewUser("pwchange@example.com"));
            var client = await _factory.LoginAsync("pwchange@example.com", "Sicher!2345");

            var response = await client.PutAsJsonAsync("api/ChangePassword",
                new { currentPassword = "Sicher!2345", newPassword = "Neu!Sicher2345" });

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var login = await anon.PostAsJsonAsync("api/signin", new { email = "pwchange@example.com", password = "Neu!Sicher2345" });
            Assert.AreEqual(HttpStatusCode.OK, login.StatusCode);
        }

        [TestMethod]
        public async Task ChangePassword_WithWrongCurrentPassword_ReturnsBadRequest()
        {
            var client = await _factory.LoginAsCustomerAsync();

            var response = await client.PutAsJsonAsync("api/ChangePassword",
                new { currentPassword = "falsch", newPassword = "Neu!Sicher2345" });

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
