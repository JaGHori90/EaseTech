using Api.Dtos;
using Api.Enums;
using Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Api.Extentions;
using System.Security.Claims;

namespace Api.Controllers
{
    public static class OrderEndpoints
    {
        public static IEndpointRouteBuilder MapOrderEndPoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/CreateOrder", CreateOrder);
            app.MapGet("/GetOrderById/{id}", GetOrderById);
            app.MapGet("/GetOrderByCustomerId/{id}", GetOrderByCustomerId);
            app.MapGet("/GetAllOrderDto", GetAllOrderDto);
            app.MapPut("/UpdateOrderById/{id}", UpdateOrderById);
            
            return app;
        }

        [Authorize(Roles = "Employee,Admin")]
        private static async Task<IResult> UpdateOrderById(int id, HttpContext httpContext, AppDbContext dbContext, [FromBody] UpdateOrderDto updateOrder)
        {
            var userRole = httpContext.User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole != "Employee" && userRole != "Admin")
            {
                return Results.Forbid(); // Verweigert den Zugriff, wenn die Rolle nicht passt
            }

            var existingOrder = await dbContext.Orders
                .Include(x=>x.Service)
                .Include(x=> x.Customer)
                .Include(x=>x.Employee)
                .Where(x => x.OrderId == id)
                .FirstOrDefaultAsync();

            if (existingOrder == null)
            {
                return Results.NotFound(new { message = "Order not found." });
            }

            // Hole die neuen Entitäten basierend auf den IDs
            var Service = await dbContext.Services.FindAsync(updateOrder.ServiceId);
            var Employee = await dbContext.Users.FindAsync(updateOrder.EmployeeId);
            var Customer = await dbContext.Users.FindAsync(updateOrder.CustomerId);

            // Überprüfe, ob die Entitäten existieren
            if (Service == null)
            {
                return Results.NotFound(new { message = "Service not found." });
            }
            if (Employee == null)
            {
                return Results.NotFound(new { message = "Employee not found." });
            }
            if (Customer == null)
            {
                return Results.NotFound(new { message = "Customer not found." });
            }

            existingOrder.Service = Service;
            existingOrder.Employee=Employee;
            existingOrder.Customer=Customer;
            existingOrder.Appointment=updateOrder.Appointment;
            existingOrder.Date=updateOrder.Date;
            existingOrder.TimeOfService=updateOrder.TimeOfService;
            existingOrder.TotalCost = updateOrder.TotalCost;
            if (!Enum.TryParse(updateOrder.OrderStatus, true, out OrderStatus orderStatus) ||
                !Enum.TryParse(updateOrder.PaymentMethod, true, out PaymentMethod paymentMethod))
            {
                return Results.BadRequest(new { message = "Invalid OrderStatus or PaymentMethod." });
            }
            existingOrder.OrderStatus = orderStatus;
            existingOrder.PaymentMethod = paymentMethod;

            await dbContext.SaveChangesAsync();
            return Results.Ok(new { message = "Order update successfully." });

        }

        [Authorize(Roles = "Admin,Employee")]
        private static async Task<IResult> GetAllOrderDto(HttpContext httpContext, AppDbContext dbContext)
        {         
            var allOrder = await dbContext.Orders.Select(x => new OrderListeDto
            {
                OrderId = x.OrderId,
                EmployeeId=x.Employee.Id,
                EmployeeName = x.Employee.FirstName+" "+x.Employee.LastName,
                CustomerName =  x.Customer.FirstName+" "+x.Customer.LastName,
                ServiceName =  x.Service.ServiceName,
                Date = x.Date,
                TimeOfService = x.TimeOfService,
                Totalcost=x.TotalCost,
                PaymentMethod = x.PaymentMethod,
                Appointment = x.Appointment,
                OrderStatus = x.OrderStatus,

            }).OrderBy(x => x.Date).ToListAsync();

            return Results.Ok(allOrder);
        }

        [Authorize]
        private static async Task<IResult> GetOrderByCustomerId(string id, AppDbContext dbContext, ClaimsPrincipal user)
        {
            if (!user.IsSelfOrStaff(id))
            {
                return Results.Forbid();
            }

            List<OrderListeDto> order = await dbContext.Orders
                .Where(x => x.CustomerId == id)
                .OrderBy(x=>x.Date)
                .Select(x => new OrderListeDto()
                {
                    OrderId = x.OrderId,
                    EmployeeId = x.Employee.Id,
                    EmployeeName = x.Employee.FirstName + " " + x.Employee.LastName,
                    CustomerName = x.Customer.FirstName + " " + x.Customer.LastName,
                    ServiceName = x.Service.ServiceName,
                    Date = x.Date,
                    TimeOfService = x.TimeOfService,
                    Totalcost = x.TotalCost,
                    PaymentMethod = x.PaymentMethod,
                    Appointment = x.Appointment,
                    OrderStatus = x.OrderStatus,
                }).ToListAsync();


            if (!order.Any())
            {
                return Results.NotFound(new { message = "Order  not found." });
            }

            return Results.Ok(order);
        }

        [Authorize]
        private static async Task<IResult> GetOrderById(int id, AppDbContext dbContext, ClaimsPrincipal user)
        {
            var order = await dbContext.Orders.Where(x => x.OrderId == id).FirstOrDefaultAsync();
            if (order == null)
            {
                return Results.NotFound(new { message = "Order not found." });
            }
            if (!user.IsSelfOrStaff(order.CustomerId))
            {
                return Results.Forbid();
            }

            return Results.Ok(order);

        }

        [Authorize] 
        private static async Task<IResult> CreateOrder(AppDbContext dbContext, ClaimsPrincipal user, [FromBody] OrderDto orderDto)
        {
            if (!Enum.TryParse<OrderStatus>(orderDto.OrderStatus, true, out OrderStatus orderStatus))
            {
                return Results.BadRequest(new { message = "Invalid OrderStatus." });
            }

            if (!Enum.TryParse<PaymentMethod>(orderDto.PaymentMethod, true, out PaymentMethod paymentMethod))
            {
                return Results.BadRequest(new { message = "Invalid PaymentMethod." });
            }

            var service = await dbContext.Services.FindAsync(orderDto.ServiceId);
            if (service == null)
            {
                return Results.BadRequest(new { message = "Service not found." });
            }

            var newOrder = new Order()
            {
                EmployeeId = string.IsNullOrWhiteSpace(orderDto.EmployeeId) ? null : orderDto.EmployeeId,
                CustomerId = orderDto.CustomerId,
                ServiceId = orderDto.ServiceId,
                Date = orderDto.Date ?? DateTime.UtcNow,
                TotalCost = orderDto.TotalCost,
                TimeOfService = orderDto.TimeOfService,
                PaymentMethod = paymentMethod,
                OrderStatus = orderStatus,
                Appointment = orderDto.Appointment ?? DateTime.UtcNow,
            };

            if (!user.IsStaff())
            {
                // Kunden dürfen nur für sich selbst buchen; Status und Preis bestimmt der Server
                newOrder.CustomerId = user.GetUserId();
                newOrder.EmployeeId = null;
                newOrder.OrderStatus = OrderStatus.Angerichtet;
                newOrder.TotalCost = service.PricePerMinute * orderDto.TimeOfService;
            }

            if (string.IsNullOrWhiteSpace(newOrder.CustomerId))
            {
                return Results.BadRequest(new { message = "CustomerId is required." });
            }

            dbContext.Orders.Add(newOrder);
            await dbContext.SaveChangesAsync();

            return Results.Ok(new { message = "Order created successfully.", orderId = newOrder.OrderId });
        }

        public record OrderDto(
            string EmployeeId,
            string CustomerId,
            int ServiceId,
            DateTime? Date,
            decimal TimeOfService,
            string OrderStatus,
            decimal TotalCost,
            string PaymentMethod,
            DateTime? Appointment
            );

        public record UpdateOrderDto(
           string EmployeeId,
           string CustomerId,
           int ServiceId,
           DateTime Date,
           decimal TimeOfService,
           string OrderStatus,
           decimal TotalCost,
           string PaymentMethod,
           DateTime Appointment
           );
    }
}
