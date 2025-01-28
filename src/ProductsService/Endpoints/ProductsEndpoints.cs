using CoreLogic.Models;
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

        app.MapGet("/products", async (IProductsRepository productsRepository) =>
        {
            var products = await productsRepository.GetAllProducts();
            return Results.Ok(products);
        });

        //app.MapGet("/products/many/{ids:string}", async (string ids, IProductsRepository productsRepository) =>
        //{
        //    List<Product> products = new List<Product>();

        //    foreach(string stringid in ids.Split(','))
        //    {
        //        int id;
        //        if(int.TryParse(stringid, out id))
        //        {
        //            var product = await productsRepository.GetProductById(id);
        //            products.Add(product);
        //        }
        //    }
        //    return Results.Ok(products);
        //});

        app.MapGet("/products/{id:int}", async (int id, IProductsRepository productsRepository) =>
        {
            var product = await productsRepository.GetProductById(id);
            return Results.Ok(product);
        });

        app.MapPost("/products", async (ProductForm product, IProductsRepository productsRepository) =>
        {
            var result = await productsRepository.CreateProduct(product.ToProduct());
            return Results.Ok(result);
        });

        app.MapPost("/products/{id:int}/reserve/{qty:int}", async (int id, int qty, IProductsRepository productsRepository) =>
        {
            var product = await productsRepository.GetProductById(id);

            if(product.Quantity < qty)
                return Results.BadRequest("Insufficient quantity for reserve!");

            product.Reserved += qty;
            product.Quantity -= qty;

            var result = await productsRepository.UpdateProduct(product);
            return Results.Ok(result);
        });

        app.MapPost("/products/many", async (List<ProductForm> productforms, IProductsRepository productsRepository) =>
        {
            var products = new List<Product>();

            foreach (var form in productforms)
                products.Add(form.ToProduct());

            var result = await productsRepository.CreateProducts(products);
            return Results.Ok(result);
        });

        app.MapPut("/products/{id:int}", async (int id, ProductForm notificationsForm, IProductsRepository productsRepository, HttpRequest request) =>
        {
            var result = await productsRepository.UpdateProduct(notificationsForm.ToProduct(id));
            return Results.Ok(result);
        });
        app.MapPatch("/products/{id:int}", async (int id, ProductForm userForm, IProductsRepository productsRepository, HttpRequest request) =>
        {
            var result = await productsRepository.UpdateProduct(userForm.ToProduct(id));
            return Results.Ok(result);
        });
        app.MapDelete("/products", async (int id, IProductsRepository productsRepository, HttpRequest request) =>
        {
            var result = await productsRepository.DeleteProduct(id);
            return Results.Ok(result);
        });

        app.MapGet("/resetdb", [AllowAnonymous] async (IProductsRepository productsRepository) =>
        {
            try
            {
                Log.Information($"Resetting Products DB");
                var result = await productsRepository.ResetDb();
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "Reset Products DB error!");
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
