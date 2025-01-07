using BillingService.Data;
using BillingService.Models;
using CoreLogic.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Serilog;
using System.IdentityModel.Tokens.Jwt;

namespace BillingService.Endpoints;

public static class BillingEndpoints
{
    public static void AddBillingEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapGet("/", [AllowAnonymous] () => "BillingService");

        app.MapGet("/accounts", async (IBillingRepository billingRepository, HttpRequest request) =>
        {
            var accounts = await billingRepository.GetAllUserAccounts(GetUserIdFromJwt(request.Headers["Authorization"]));
            return Results.Ok(accounts);
        });

        app.MapGet("/accounts/{id:int}", async (int id, IBillingRepository billingRepository) =>
        {
            var account = await billingRepository.GetAccountById(id);
            return Results.Ok(account);
        });

        app.MapPost("/accounts", async (UpdateAccountForm accountForm, IBillingRepository billingRepository, HttpRequest request) =>
        {
            int userId = GetUserIdFromJwt(request.Headers["Authorization"]);
            var account = accountForm.ToAccount();
            account.UserId = userId;
            account.Number = Guid.NewGuid().ToString("N").ToUpper();
            var result = await billingRepository.CreateAccount(account);
            return Results.Ok(result);
        });

        app.MapPut("/accounts/{id:int}", async (int id, UpdateAccountForm accountForm, IBillingRepository billingRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);
            var account = await billingRepository.GetAccountById(id);

            if (requestUserId != account.UserId)
                return Results.BadRequest("Modifying another user accountForm is not allowed!");

            var result = await billingRepository.UpdateAccount(accountForm.ToAccount(id));
            return Results.Ok(result);
        });

        app.MapPatch("/accounts/{id:int}", async (int id, UpdateAccountForm accountForm, IBillingRepository billingRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);
            var account = await billingRepository.GetAccountById(id);

            if (requestUserId != account.UserId)
                return Results.BadRequest("Modifying another user accountForm is not allowed!");

            var result = await billingRepository.UpdateAccount(accountForm.ToAccount(id));
            return Results.Ok(result);
        });

        app.MapDelete("/accounts", async (int id, IBillingRepository billingRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);
            var account = await billingRepository.GetAccountById(id);

            if (requestUserId != account.UserId)
                return Results.BadRequest("Deleting another user accountForm is not allowed!");

            if (account.Balance != 0)
                return Results.BadRequest("Deleting non zero accountForm is not allowed!");

            var result = await billingRepository.DeleteAccount(id);
            return Results.Ok(result);
        });

        app.MapGet("account/{accountid:int}/transactions", async (int accountid, IBillingRepository billingRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);
            var account = await billingRepository.GetAccountById(accountid);

            if (requestUserId != account.UserId)
                return Results.BadRequest("Access another user account data is not allowed!");

            var transactions = await billingRepository.GetTransactionsByAccountId(accountid);

            return Results.Ok(transactions);
        });

        app.MapPost("transactions", async (UpdateTransactionForm transactionForm, IBillingRepository billingRepository, HttpRequest request) =>
        {
            int requestUserId = GetUserIdFromJwt(request.Headers["Authorization"]);
            var account = await billingRepository.GetAccountById(transactionForm.AccountId);

            if (requestUserId != account.UserId)
                return Results.BadRequest("Access another user account data is not allowed!");

            var transaction = new Transaction()
            {
                AccountId = account.Id,
                UserId = requestUserId,
                Amount = transactionForm.Amount,
                Description = transactionForm.Description,
                CreatedOn = DateTime.UtcNow
            };

            var result = await billingRepository.CreateTransaction(transaction);
            return Results.Ok(result);
        });

        //app.MapGet("/transactions", async (IBillingRepository billingRepository) =>
        //{
        //    var transactions = await billingRepository.GetAllTransactions();
        //    return Results.Ok(transactions);
        //});

        app.MapGet("/transactions/{id:int}", async (int id, IBillingRepository billingRepository) =>
        {
            var transaction = await billingRepository.GetTransactionById(id);
            return Results.Ok(transaction);
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
