using CoreLogic.Models;
using Microsoft.AspNetCore.Authorization;
using OrdersService.Data;
using OrdersService.Models;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

namespace OrdersService.Endpoints;

public static class OrdersEndpoints
{
    public static void AddOrdersEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapGet("/", () => "UserService");

        app.MapGet("/orders", [AllowAnonymous] async (IOrdersRepository ordersRepository) =>
        {
            var orders = await ordersRepository.GetAllOrders();
            return Results.Ok(orders);
        });

        app.MapGet("/orders/{id:int}", async (int id, IOrdersRepository ordersRepository) =>
        {
            var user = await ordersRepository.GetOrderById(id);
            return Results.Ok(user);
        });

        app.MapPost("/orders", async (Order order, IOrdersRepository ordersRepository) =>
        {
            var result = await ordersRepository.CreateOrder(order);
            return Results.Ok(result);
        });

        app.MapPut("/orders/{id:int}", async (int id, UpdateOrderForm userForm, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another order is not allowed!");

            var result = await ordersRepository.UpdateUser(userForm.ToOrder(id));
            return Results.Ok(result);
        });
        app.MapPatch("/orders/{id:int}", async (int id, UpdateOrderForm userForm, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another order is not allowed!");

            var result = await ordersRepository.UpdateUserPartial(userForm.ToOrder(id));
            return Results.Ok(result);
        });
        app.MapDelete("/orders", [Authorize] async (int id, IOrdersRepository ordersRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Deleting another order is not allowed!");

            var result = await ordersRepository.DeleteOrder(id);
            return Results.Ok(result);
        });

        app.MapGet("/resetdb", [AllowAnonymous] async (IOrdersRepository ordersRepository) =>
        {
            try
            {
                Log.Information($"Resetting Users DB");
                var result = await ordersRepository.ResetDb();
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "Register error!");
            }
        });
    }

    private static int GetUserIdFromJwt(string authHeader)
    {
        if (String.IsNullOrEmpty(authHeader))
            return -1;

        var token = authHeader.Replace("Bearer ", "");
        var handler = new JwtSecurityTokenHandler();
        var jwtSecurityToken = handler.ReadJwtToken(token);
        var userId = jwtSecurityToken.Claims.First(claim => claim.Type == "id").Value;
        return int.Parse(userId);
    }
}
