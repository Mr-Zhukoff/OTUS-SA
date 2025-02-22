﻿namespace UsersService.Models;

public class RegisterForm
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string Email { get; set; }

    public string Password { get; set; }

    public override string ToString()
    {
        return $"{LastName} {FirstName} {Email} {Password}";
    }

}
