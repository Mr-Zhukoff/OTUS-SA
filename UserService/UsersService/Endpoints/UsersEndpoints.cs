using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UsersService.Data;
using UsersService.Models;
using UsersService.Security;

namespace UsersService.Endpoints;

public static class UsersEndpoints
{
    public static void AddUsersEndpoints(this IEndpointRouteBuilder app, IConfiguration config)
    {
        app.MapGet("/", () => "UserService");

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

        app.MapPut("/users", async (User user, IUsersRepository userRepository) =>
        {
            var result = await userRepository.UpdateUser(user);
            return Results.Ok(result);
        });
        app.MapPatch("/users", async (User user, IUsersRepository userRepository) =>
        {
            var result = await userRepository.UpdateUserPartial(user);
            return Results.Ok(result);
        });
        app.MapDelete("/users", async (int id, IUsersRepository userRepository) =>
        {
            var result = await userRepository.DeleteUser(id);
            return Results.Ok(result);
        });
        app.MapPost("/login", async (LoginForm loginForm, IUsersRepository userRepository) =>
        {
            try
            {
                Log.Information($"Login form requested {loginForm.ToString()}");
                var existingUser = await userRepository.GetUserByEmail(loginForm.Email);
                if (existingUser == null)
                {
                    Log.Warning($"User ({loginForm.Email}) not found!");
                    return Results.BadRequest($"Пользователь с email '{loginForm.Email}' не существует!");
                }

                var passwordHash = PasswordHasher.ComputeHash(loginForm.Password, existingUser.PasswordSalt, config["Auth:Pepper"]);

                if (!(existingUser.PasswordHash == passwordHash))
                {
                    return Results.BadRequest($"Пароль не правильный!");
                }

                var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]));
                var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                var Sectoken = new JwtSecurityToken(config["Jwt:Issuer"],
                  config["Jwt:Issuer"],
                  claims: new List<Claim>
                        {
                        new Claim(ClaimTypes.Email, existingUser.Email),
                        new Claim("id", existingUser.Id.ToString()),
                        },
                  notBefore: DateTime.UtcNow,
                  expires: DateTime.Now.AddMinutes(120),
                  signingCredentials: credentials);

                var token = new JwtSecurityTokenHandler().WriteToken(Sectoken);

                return Results.Ok(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "Login error!" );
            }
        });

        app.MapPost("/register", async (RegisterForm registerForm, IUsersRepository userRepository) =>
        {
            try
            {
                Log.Information($"Register form requested {registerForm.ToString()}");
                var existingUser = userRepository.GetUserByEmail(registerForm.Email);
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

                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Log.Error(ex, ex.Message);
                return Results.Problem(ex.Message, null, 500, "Register error!");
            }
        });


    }
}
