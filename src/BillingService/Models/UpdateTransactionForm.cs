using CoreLogic.Models;

namespace BillingService.Models;

public class UpdateTransactionForm
{
    public int AccountId { get; set; }

    public string Description { get; set; }

    public decimal Amount { get; set; }

    public override string ToString()
    {
        return $"{Amount} {Description}";
    }
    public Transaction ToTransaction(int transactionId = 0)
    {
        return new Transaction
        {
            Id = transactionId,
            AccountId = AccountId,
            Description = Description,
            Amount = Amount
        };
    }
}
