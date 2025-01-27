using CoreLogic.Models;
using CoreLogic.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using UsersService.Data;
using UsersService.Models;

namespace UsersService.Endpoints;

public static class UsersEndpoints
{
    public static void AddUsersEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapGet("/", [AllowAnonymous] () => "UserService");

        app.MapGet("/users", async (IUsersRepository userRepository) =>
        {
            var users = await userRepository.GetAllUsers();
            return Results.Ok(users);
        });

        app.MapGet("/users/{id:int}", async (int id, IUsersRepository userRepository) =>
        {
            var user = await userRepository.GetUserById(id);
            return Results.Ok(user);
        });

        app.MapGet("/users/{email}", async (string email, IUsersRepository userRepository) =>
        {
            var user = await userRepository.GetUserByEmail(email);
            return Results.Ok(user);
        });

        app.MapPost("/users", async (User user, IUsersRepository userRepository) =>
        {
            var result = await userRepository.CreateUser(user);
            return Results.Ok(result);
        });

        app.MapPut("/users/{id:int}", async (int id, UpdateUserForm userForm, IUsersRepository userRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != 0 && requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await userRepository.UpdateUser(userForm.ToUser(id));
            return Results.Ok(result);
        });
        app.MapPatch("/users/{id:int}", async (int id, UpdateUserForm userForm, IUsersRepository userRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != 0 && requestUserId != id)
                return Results.BadRequest("Modifying another user is not allowed!");

            var result = await userRepository.UpdateUserPartial(userForm.ToUser(id));
            return Results.Ok(result);
        });
        app.MapDelete("/users", [Authorize] async (int id, IUsersRepository userRepository, HttpRequest request) =>
        {
            int requestUserId = PasswordHasher.GetUserIdFromJwt(request.Headers["Authorization"]);

            if (requestUserId != 0 && requestUserId != id)
                return Results.BadRequest("Deleting another user is not allowed!");

            var result = await userRepository.DeleteUser(id);
            return Results.Ok(result);
        });
        app.MapPost("/login", [AllowAnonymous] async (LoginForm loginForm, IUsersRepository userRepository) =>
        {
            try
            {
                Log.Information($"Login form requested");
                User user;
                // Бэкдор для админа
                if (loginForm.Email == "admin@zhukoff.pro" && loginForm.Password == "P@ssw0rd")
                {
                    user = new User() { 
                        Id = 0,
                        Email = loginForm.Email,
                    };
                }
                else
                {
                    user = await userRepository.GetUserByEmail(loginForm.Email);
                    if (user == null)
                    {
                        Log.Warning($"User ({loginForm.Email}) not found!");
                        return Results.BadRequest($"Пользователь с email '{loginForm.Email}' не существует!");
                    }

                    var passwordHash = PasswordHasher.ComputeHash(loginForm.Password, user.PasswordSalt, config["Auth:Pepper"]);

                    if (!(user.PasswordHash == passwordHash))
                    {
                        return Results.BadRequest($"Пароль не правильный!");
                    }
                }

                JwtSecurityToken Sectoken = GetSecurityToken(config, user);

                var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

                Log.Information($"User {loginForm.Email} logged in");

                return Results.Ok(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "Login error!" );
            }
        });

        app.MapPost("/register", [AllowAnonymous] async (RegisterForm registerForm, IUsersRepository userRepository) =>
        {
            try
            {
                Log.Information($"Register form requested {registerForm.ToString()}");
                var existingUser = await userRepository.GetUserByEmail(registerForm.Email);
                if (existingUser != null)
                {
                    Log.Warning($"User already exist!");
                    return Results.BadRequest($"Пользователь с email {registerForm.Email} уже зарегистрирован!");
                }

                var user = new User
                {
                    FirstName = registerForm.FirstName,
                    LastName = registerForm.LastName,
                    MiddleName = registerForm.MiddleName,
                    Email = registerForm.Email,
                    PasswordSalt = PasswordHasher.GenerateSalt()
                };
                user.PasswordHash = PasswordHasher.ComputeHash(registerForm.Password, user.PasswordSalt, config["Auth:Pepper"]);
                var result = userRepository.CreateUser(user);

                Log.Information($"User {registerForm.Email} registered");

                return Results.Ok(result.Result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "Register error!");
            }
        });

        app.MapGet("/resetdb", [AllowAnonymous] async (IUsersRepository userRepository) =>
        {
            try
            {
                Log.Information($"Resetting Users DB");
                var result = await userRepository.ResetDb();
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "Register error!");
            }
        });

        app.MapGet("/health", [AllowAnonymous] (IUsersRepository userRepository) =>
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
                    pgconnstr = userRepository.GetConnectionInfo()
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


    private static JwtSecurityToken GetSecurityToken(IConfiguration config, User user)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var Sectoken = new JwtSecurityToken(config["Jwt:Issuer"],
          config["Jwt:Issuer"],
          claims: new List<Claim>
                {
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("id", user.Id.ToString()),
                },
          notBefore: DateTime.UtcNow,
          expires: DateTime.Now.AddMinutes(config.GetSection("Jwt:TokenDuration").Get<int>()),
          signingCredentials: credentials);
        return Sectoken;
    }
}
