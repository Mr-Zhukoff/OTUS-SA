using CoreLogic.Models;
using CoreLogic.Security;
using Microsoft.AspNetCore.Authorization;
using ProductsService.Data;
using ProductsService.Models;
using Serilog;
using System.Reflection;

namespace ProductsService.Endpoints;

public static class ProductsEndpoints
{
    public static void AddProductsEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapGet("/", () => "ProductsService");

        app.MapGet("/products", [AllowAnonymous] async (IProductsRepository productsRepository) =>
        {
            var products = await productsRepository.GetAllProducts();
            return Results.Ok(products);
        });

        app.MapGet("/products/{id:int}", async (int id, IProductsRepository productsRepository) =>
        {
            var product = await productsRepository.GetProductById(id);
            return Results.Ok(product);
        });

        app.MapPost("/products", async (Product product, IProductsRepository productsRepository) =>
        {
            var result = await productsRepository.CreateProduct(product);
            return Results.Ok(result);
        });

        app.MapPut("/products/{id:int}", async (int id, UpdateProductForm notificationsForm, IProductsRepository productsRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await productsRepository.UpdateProduct(notificationsForm.ToProduct(id));
            return Results.Ok(result);
        });
        app.MapPatch("/products/{id:int}", async (int id, UpdateProductForm userForm, IProductsRepository productsRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await productsRepository.UpdateProduct(userForm.ToProduct(id));
            return Results.Ok(result);
        });
        app.MapDelete("/products", async (int id, IProductsRepository productsRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != id)
                return Results.BadRequest("Deleting another user is not allowed!");

            var result = await productsRepository.DeleteProduct(id);
            return Results.Ok(result);
        });

        app.MapGet("/resetdb", [AllowAnonymous] async (IProductsRepository productsRepository) =>
        {
            try
            {
                Log.Information($"Resetting Users DB");
                var result = await productsRepository.ResetDb();
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "Register error!");
            }
        });

        app.MapGet("/health", [AllowAnonymous] (IProductsRepository productsRepository) =>
        {
            try
            {
                Log.Information($"Health status requested {Environment.MachineName}");

                Assembly assembly = Assembly.GetExecutingAssembly();
                System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);

                return Results.Ok(new
                {
                    status = "OK",
                    app = Assembly.GetExecutingAssembly().FullName,
                    version = fvi.FileVersion,
                    machinename = Environment.MachineName,
                    osversion = Environment.OSVersion.VersionString,
                    processid = Environment.ProcessId,
                    timestamp = DateTime.Now,
                    pgconnstr = productsRepository.GetConnectionInfo()
                });
            }
            catch (Exception ex)
            {
                return Results.Ok(new
                {
                    status = "BAD",
                    machinename = Environment.MachineName,
                    osversion = Environment.OSVersion.VersionString,
                    processid = Environment.ProcessId,
                    message = ex.Message
                });
            }
        });
    }
}
