
using Api.Dtos;
using Api.Enums;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Api.Extentions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Controllers
{
    public static class InvoiceEndpoints
    {
        #region Map Invoice Endpoints
        public static IEndpointRouteBuilder MapInvoiceEndPoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/CreateInvoiceByOrderId/{id}", CreateInvoiceByOrderId);
            app.MapGet("/GetInvoiceById/{id}", GetInvoiceById);
            app.MapGet("/GetInvoiceByOrderId/{id}", GetInvoiceByOrderId);
            app.MapGet("/GetAllInvoice", GetAllInvoice);
            app.MapPut("/UpdateInvoiceById/{id}", UpdateInvoiceById);

            return app;
        }

        #endregion

        #region APIs
        [Authorize]
        private static async Task<IResult> GetInvoiceByOrderId(int id, ClaimsPrincipal user, AppDbContext dbContext)
        {

            var invoice = await dbContext.Invoices.Where(x => x.OrderId == id).FirstOrDefaultAsync();
            if (invoice == null)
            {
                return Results.NotFound(new { message = "Invoice not found." });
            }
            if (!user.IsSelfOrStaff(invoice.CustomerId))
            {
                return Results.Forbid();
            }

            return Results.Ok(invoice);

        }

        [Authorize(Roles = "Admin")]
        private static async Task<IResult> UpdateInvoiceById(int id, HttpContext httpContext, AppDbContext dbContext, [FromBody] InvoiceDto invoiceDto)
        {
            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Employee" && userRole != "Admin")
            {
                return Results.Forbid(); // Verweigert den Zugriff, wenn die Rolle nicht passt
            }

            var existingInvoice = await dbContext.Invoices.Where(x => x.InvoiceId == id).FirstOrDefaultAsync();
            if (existingInvoice == null)
            {
                return Results.NotFound(new { message = "Invoice not found." });
            }

            existingInvoice.ServiceId = invoiceDto.serviceId ?? existingInvoice.ServiceId;
            existingInvoice.OrderId = invoiceDto.orderId ?? existingInvoice.OrderId;
            existingInvoice.CustomerId = invoiceDto.customerId ?? existingInvoice.CustomerId;
            existingInvoice.EmployeeId = invoiceDto.employeeId ?? existingInvoice.EmployeeId;
            existingInvoice.TotalAmount = invoiceDto.totalAmount;
            existingInvoice.TaxAmount = invoiceDto.taxAmount;
            existingInvoice.NetAmount = invoiceDto.netAmount;
            if (!Enum.TryParse(invoiceDto.paymentMethod, true, out PaymentMethod paymentMethod) ||
                !Enum.TryParse(invoiceDto.paymentStatus, true, out PaymentStatus paymentStatus))
            {
                return Results.BadRequest(new { message = "Invalid PaymentMethod or PaymentStatus." });
            }
            existingInvoice.PaymentMethod = paymentMethod;
            existingInvoice.PaymentStatus = paymentStatus;
            existingInvoice.PaymentReference = invoiceDto.paymentReference;
            existingInvoice.OrderDate = invoiceDto.orderDate;
            existingInvoice.InvoiceDate = invoiceDto.invoiceDate;

            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Invoice update successfully." });
        }

        [Authorize]
        private static async Task<IResult> GetInvoiceById(int id, ClaimsPrincipal user, AppDbContext dbContext)
        {
            var invoice = await dbContext.Invoices.Where(x => x.InvoiceId == id).FirstOrDefaultAsync();
            if (invoice == null)
            {
                return Results.NotFound(new { message = "Invoice not found." });
            }
            if (!user.IsSelfOrStaff(invoice.CustomerId))
            {
                return Results.Forbid();
            }

            return Results.Ok(invoice);

        }

        [Authorize(Roles = "Admin,Employee")]
        private static async Task<IResult> GetAllInvoice(AppDbContext dbContext, HttpContext httpContext)
        {
            var allInvoices = await dbContext.Invoices
                .Include(x => x.Customer)
                .Include(x => x.Order)
                .Include(x => x.Service)
                .Include(x => x.Employee)
                .AsNoTracking()
                .ToListAsync(); 

            var invoiceDtos = allInvoices
                .Select(x => new InvoiceListeDto()
                {
                    InvoiceId = x.InvoiceId,
                    CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                    EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName,
                    ServiceName = x.Service.ServiceName,
                    OrderId = x.OrderId,
                    OrderDate = x.OrderDate,
                    PaymentMethod = x.PaymentMethod,
                    TotalAmount = x.TotalAmount,
                    TaxAmount = x.TaxAmount,
                    NetAmount = x.NetAmount,
                    InvoiceDate = x.InvoiceDate,
                    PaymentStatus = x.PaymentStatus,
                    PaymentReference = x.PaymentReference,
                })
                .OrderBy(x => x.OrderDate) 
                .ThenBy(x => x.CustomerName)
                .ThenBy(x => x.EmployeeName)
                .ToList();

            return Results.Ok(invoiceDtos);
        }

        [Authorize]
        private static async Task<IResult> CreateInvoiceByOrderId(int id, ClaimsPrincipal user, AppDbContext dbContext)
        {
            var order = await dbContext.Orders
                .Where(x => x.OrderId == id)
                .Include(x => x.Service)
                .Include(x => x.Employee)
                .Include(x => x.Customer)
                .FirstOrDefaultAsync();
            if (order == null)
            {
                return Results.NotFound(new { message = "Order not found." });
            }
            if (!user.IsSelfOrStaff(order.CustomerId))
            {
                return Results.Forbid();
            }
            if (await dbContext.Invoices.AnyAsync(x => x.OrderId == id))
            {
                return Results.Conflict(new { message = "Invoice already exists." });
            }

            string OrderStatus = order.OrderStatus.ToString();
            string orderIdString = (string)order.OrderId.ToString();

            if (OrderStatus != "Abgeschlossen")
            {
                return Results.BadRequest(new { message = "Die Order is not finished !" });
            }

            double TotalCost = (double)order.TotalCost;
            double TaxAmount = (double)TotalCost-(TotalCost / 1.2);
            double NetAmount = (double)(TotalCost - TaxAmount);
            string date = DateTime.UtcNow.ToString("yyyyMMdd");

            string PaymentReference = "RE-" + orderIdString + "-" + date;

            var invoice = new Invoice()
            {
                CustomerId = order.CustomerId,
                OrderId = order.OrderId,
                EmployeeId = order.EmployeeId,
                ServiceId = order.ServiceId,
                TotalAmount = order.TotalCost,
                TaxAmount = (decimal)TaxAmount,
                NetAmount = (decimal)NetAmount,
                OrderDate = order.Date,
                PaymentMethod = order.PaymentMethod,
                InvoiceDate = DateTime.UtcNow,
                PaymentReference = PaymentReference,
                PaymentStatus = PaymentStatus.Ausstehend
            };

            dbContext.Invoices.Add(invoice);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Invoice created successfully." });

        }

        #endregion

        #region Records

        public record InvoiceDto(
            int? invoiceId,
            int? orderId,
            string customerId,
            string employeeId,
            int? serviceId,
            decimal totalAmount,
            decimal taxAmount,
            decimal netAmount,
            DateTime orderDate,
            DateTime invoiceDate,
            string paymentMethod,
            string paymentStatus,
            string paymentReference
            );
        #endregion

    }
}
