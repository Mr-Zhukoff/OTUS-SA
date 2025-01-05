namespace CoreLogic.Models;

public class UpdateUserForm
{
    public string FirstName { get; set; }

    public string LastName { get; set; }

    public string MiddleName { get; set; }

    public string Email { get; set; }

    public override string ToString()
    {
        return $"{LastName} {FirstName} {Email}";
    }
    public User ToUser(int userId)
    {
        return new User
        {
            Id = userId,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Email = Email
        };
    }
}
