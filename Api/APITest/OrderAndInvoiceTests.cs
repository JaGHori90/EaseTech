using System.Net;
using System.Net.Http.Json;
using Api.Enums;
using Api.Models;
using APITest.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace APITest
{
    [TestClass]
    public class OrderAndInvoiceTests
    {
        private static EaseTechApiFactory _factory = null!;

        [ClassInitialize]
        public static void Setup(TestContext _) => _factory = new EaseTechApiFactory();

        [ClassCleanup]
        public static void Cleanup() => _factory.Dispose();

        private static Task<Service> FirstServiceAsync() =>
            _factory.WithDbAsync(db => db.Services.OrderBy(s => s.ServiceId).FirstAsync());

        private static object OrderBody(string customerId, int serviceId, string status = "Angerichtet",
            decimal totalCost = 0, string? employeeId = null) => new
        {
            employeeId,
            customerId,
            serviceId,
            date = "2025-06-01T10:00:00.000Z",
            timeOfService = 15,
            orderStatus = status,
            totalCost,
            paymentMethod = "Rechnung",
            appointment = "2025-06-01T10:00:00.000Z"
        };

        private static async Task<int> CreateOrderAsStaffAsync(string customerId, string status)
        {
            var employee = await _factory.LoginAsEmployeeAsync();
            var employeeId = await employee.GetOwnUserIdAsync();
            var service = await FirstServiceAsync();
            var response = await employee.PostAsJsonAsync("api/CreateOrder",
                OrderBody(customerId, service.ServiceId, status, 30m, employeeId));
            response.EnsureSuccessStatusCode();
            return (await response.ReadJsonAsync()).GetProperty("orderId").GetInt32();
        }

        [TestMethod]
        public async Task CreateOrder_AsCustomer_ServerControlsCustomerStatusAndPrice()
        {
            var customer = await _factory.LoginAsCustomerAsync();
            var customerId = await customer.GetOwnUserIdAsync();
            var service = await FirstServiceAsync();

            // Versuch: für jemand anderen, bereits "abgeschlossen" und gratis buchen
            var response = await customer.PostAsJsonAsync("api/CreateOrder",
                OrderBody("fremde-id", service.ServiceId, "Abgeschlossen", 0m));

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            var orderId = (await response.ReadJsonAsync()).GetProperty("orderId").GetInt32();
            var order = await _factory.WithDbAsync(db => db.Orders.SingleAsync(o => o.OrderId == orderId));

            Assert.AreEqual(customerId, order.CustomerId);
            Assert.AreEqual(OrderStatus.Angerichtet, order.OrderStatus);
            Assert.AreEqual(service.PricePerMinute * 15, order.TotalCost);
            Assert.IsNull(order.EmployeeId);
        }

        [TestMethod]
        public async Task CreateOrder_WithUnknownService_ReturnsBadRequest()
        {
            var customer = await _factory.LoginAsCustomerAsync();

            var response = await customer.PostAsJsonAsync("api/CreateOrder", OrderBody("x", 9999));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateOrder_WithInvalidStatus_ReturnsBadRequest()
        {
            var customer = await _factory.LoginAsCustomerAsync();
            var service = await FirstServiceAsync();

            var response = await customer.PostAsJsonAsync("api/CreateOrder", OrderBody("x", service.ServiceId, "GibtsNicht"));

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task GetOrderByCustomerId_ForOtherCustomer_IsForbidden()
        {
            var customer = await _factory.LoginAsCustomerAsync();

            var response = await customer.GetAsync("api/GetOrderByCustomerId/andere-kunden-id");

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task GetOrderById_OfOtherCustomer_IsForbidden()
        {
            var customer = await _factory.LoginAsCustomerAsync();
            var orderId = await CreateOrderAsStaffAsync(await OtherCustomerIdAsync(), "Angerichtet");

            var response = await customer.GetAsync($"api/GetOrderById/{orderId}");

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task UpdateOrder_AsCustomer_IsForbidden()
        {
            var customer = await _factory.LoginAsCustomerAsync();
            var orderId = await CreateOrderAsStaffAsync(await customer.GetOwnUserIdAsync(), "Angerichtet");

            var response = await customer.PutAsJsonAsync($"api/UpdateOrderById/{orderId}", new { });

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateInvoice_ForUnfinishedOrder_ReturnsBadRequest()
        {
            var customer = await _factory.LoginAsCustomerAsync();
            var orderId = await CreateOrderAsStaffAsync(await customer.GetOwnUserIdAsync(), "InBearbeitung");

            var response = await customer.PostAsync($"api/CreateInvoiceByOrderId/{orderId}", null);

            Assert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [TestMethod]
        public async Task CreateInvoice_ForOwnCompletedOrder_CalculatesTaxAndRejectsDuplicate()
        {
            var customer = await _factory.LoginAsCustomerAsync();
            var orderId = await CreateOrderAsStaffAsync(await customer.GetOwnUserIdAsync(), "Abgeschlossen");

            var first = await customer.PostAsync($"api/CreateInvoiceByOrderId/{orderId}", null);
            var second = await customer.PostAsync($"api/CreateInvoiceByOrderId/{orderId}", null);

            Assert.AreEqual(HttpStatusCode.OK, first.StatusCode);
            Assert.AreEqual(HttpStatusCode.Conflict, second.StatusCode);

            var invoice = await _factory.WithDbAsync(db => db.Invoices.SingleAsync(i => i.OrderId == orderId));
            Assert.AreEqual(30m, invoice.TotalAmount);
            Assert.AreEqual(25m, Math.Round(invoice.NetAmount, 2));   // 30 / 1,2
            Assert.AreEqual(5m, Math.Round(invoice.TaxAmount, 2));    // 20 % USt
            Assert.AreEqual(PaymentStatus.Ausstehend, invoice.PaymentStatus);

            var get = await customer.GetAsync($"api/GetInvoiceByOrderId/{orderId}");
            Assert.AreEqual(HttpStatusCode.OK, get.StatusCode);
        }

        [TestMethod]
        public async Task GetInvoice_OfOtherCustomer_IsForbidden()
        {
            var customer = await _factory.LoginAsCustomerAsync();
            var orderId = await CreateOrderAsStaffAsync(await OtherCustomerIdAsync(), "Abgeschlossen");
            var employee = await _factory.LoginAsEmployeeAsync();
            (await employee.PostAsync($"api/CreateInvoiceByOrderId/{orderId}", null)).EnsureSuccessStatusCode();

            var response = await customer.GetAsync($"api/GetInvoiceByOrderId/{orderId}");

            Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
        }

        private static async Task<string> OtherCustomerIdAsync()
        {
            var anon = _factory.CreateClient();
            var email = $"anderer-{Guid.NewGuid():N}@example.com";
            await anon.PostAsJsonAsync("api/signup", new
            {
                firstName = "Anderer", lastName = "Kunde", email, password = "Sicher!2345", role = "Customer",
                gender = "", birthday = "2000-01-01T00:00:00", address = "", city = "", zipCode = "", phoneNumber = ""
            });
            return await _factory.WithDbAsync(db => db.Users.Where(u => u.Email == email).Select(u => u.Id).SingleAsync());
        }
    }
}
