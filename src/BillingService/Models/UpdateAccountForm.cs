using CoreLogic.Models;

namespace BillingService.Models;

public class UpdateAccountForm
{
    public int UserId { get; set; }

    public string Number { get; set; }

    public string Description { get; set; }

    public decimal Balance { get; set; }

    public override string ToString()
    {
        return $"{UserId} {Number} {Description}";
    }
    public Account ToUser(int accountId)
    {
        return new Account
        {
            Id = accountId,
            Number = Number,
            Description = Description,
            Balance = Balance
        };
    }
}
