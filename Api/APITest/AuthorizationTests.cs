using System.Net;
using APITest.Infrastructure;

namespace APITest
{
    /// <summary>
    /// Prüft, dass geschützte Endpunkte ohne Anmeldung (401) bzw. mit falscher Rolle (403) abgelehnt werden.
    /// </summary>
    [TestClass]
    public class AuthorizationTests
    {
        private static EaseTechApiFactory _factory = null!;

        [ClassInitialize]
        public static void Setup(TestContext _) => _factory = new EaseTechApiFactory();

        [ClassCleanup]
        public static void Cleanup() => _factory.Dispose();

        [TestMethod]
        [DataRow("api/UserProfile")]
        [DataRow("api/GetAllUserProfile")]
        [DataRow("api/GetAllRequest")]
        [DataRow("api/GetRequestById/1")]
        [DataRow("api/GetAllOrderDto")]
        [DataRow("api/GetAllInvoice")]
        [DataRow("api/GetAllCalls")]
        [DataRow("api/CreateToken")]
        public async Task ProtectedEndpoints_WithoutToken_Return401(string url)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(url);

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode, url);
        }

        [TestMethod]
        [DataRow("api/GetAllService")]
        [DataRow("api/GetServiceById/1")]
        [DataRow("api/IsEmpAvailable")]
        [DataRow("health")]
        public async Task PublicEndpoints_WithoutToken_Return200(string url)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(url);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, url);
        }

        [TestMethod]
        [DataRow("api/GetAllUserProfile")]
        [DataRow("api/GetAllCustomer")]
        [DataRow("api/GetAllEmployeesAndAdmins")]
        [DataRow("api/GetAllRequest")]
        [DataRow("api/GetAllOrderDto")]
        [DataRow("api/GetAllInvoice")]
        [DataRow("api/GetAllCalls")]
        public async Task StaffEndpoints_AsCustomer_Return403(string url)
        {
            var client = await _factory.LoginAsCustomerAsync();

            var response = await client.GetAsync(url);

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode, url);
        }

        [TestMethod]
        [DataRow("api/GetAllUserProfile")]
        [DataRow("api/GetAllCustomer")]
        [DataRow("api/GetAllEmployeesAndAdmins")]
        [DataRow("api/GetAllRequest")]
        [DataRow("api/GetAllOrderDto")]
        [DataRow("api/GetAllInvoice")]
        public async Task StaffEndpoints_AsEmployee_Return200(string url)
        {
            var client = await _factory.LoginAsEmployeeAsync();

            var response = await client.GetAsync(url);

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode, url);
        }

        [TestMethod]
        public async Task ManipulatedToken_Returns401()
        {
            var client = await _factory.LoginAsCustomerAsync();
            var token = client.DefaultRequestHeaders.Authorization!.Parameter!;
            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token[..^4] + "abcd");

            var response = await client.GetAsync("api/UserProfile");

            Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        }
    }
}
