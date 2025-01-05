using BillingService.Data;
using BillingService.Models;
using CoreLogic.Models;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

namespace BillingService.Endpoints;

public static class BillingEndpoints
{
    public static void AddBillingEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapGet("/", () => "BillingService");

        app.MapGet("/accounts", [AllowAnonymous] async (IBillingRepository billingRepository) =>
        {
            var users = await billingRepository.GetAllAccounts();
            return Results.Ok(users);
        });

        app.MapGet("/accounts/{id:int}", async (int id, IBillingRepository billingRepository) =>
        {
            var account = await billingRepository.GetAccountById(id);
            return Results.Ok(account);
        });

        app.MapPost("/accounts", async (Account account, IBillingRepository billingRepository) =>
        {
            var result = await billingRepository.CreateAccount(account);
            return Results.Ok(result);
        });

        app.MapPut("/accounts/{id:int}", async (int id, UpdateAccountForm accountForm, IBillingRepository billingRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await billingRepository.UpdateAccount(accountForm.ToUser(id));
            return Results.Ok(result);
        });

        app.MapPatch("/accounts/{id:int}", async (int id, UpdateAccountForm accountForm, IBillingRepository billingRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await billingRepository.UpdateAccountPartial(accountForm.ToUser(id));
            return Results.Ok(result);
        });

        app.MapDelete("/accounts", async (int id, IBillingRepository billingRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Deleting another user is not allowed!");

            var result = await billingRepository.DeleteAccount(id);
            return Results.Ok(result);
        });

        app.MapGet("/transactions", async (IBillingRepository billingRepository) =>
        {
            var transactions = await billingRepository.GetAllTransactions();
            return Results.Ok(transactions);
        });

        app.MapGet("/transactions/{id:int}", async (int id, IBillingRepository billingRepository) =>
        {
            var transaction = await billingRepository.GetTransactionById(id);
            return Results.Ok(transaction);
        });

        app.MapGet("/transactions/account/{accountid:int}", async (int accountid, IBillingRepository billingRepository) =>
        {
            var transaction = await billingRepository.GetTransactionsByAccountId(accountid);
            return Results.Ok(transaction);
        });

        app.MapPost("/transactions", async (Transaction transaction, IBillingRepository billingRepository) =>
        {
            var result = await billingRepository.CreateTransaction(transaction);
            return Results.Ok(result);
        });

        app.MapGet("/resetdb", [AllowAnonymous] async (IBillingRepository billingRepository) =>
        {
            try
            {
                Log.Information($"Resetting Billing DB");
                var result = await billingRepository.ResetDb();
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
