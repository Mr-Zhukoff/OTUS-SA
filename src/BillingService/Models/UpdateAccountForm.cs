using CoreLogic.Models;

namespace BillingService.Models;

public class UpdateAccountForm
{
    public string Number { get; set; }

    public string Description { get; set; }

    public decimal Balance { get; set; }

    public override string ToString()
    {
        return $"{Number} {Description}";
    }
    public Account ToAccount(int accountId = 0)
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
