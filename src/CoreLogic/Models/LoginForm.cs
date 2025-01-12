namespace CoreLogic.Models;

public class LoginForm
{
    public string Email { get; set; }
    public string Password { get; set; }
    public LoginForm()
    {
        
    }
    public LoginForm(string email, string password)
    {
        Email = email;
        Password = password;
    }
    public override string ToString()
    {
        return $"{Email} {Password}";
    }
}
