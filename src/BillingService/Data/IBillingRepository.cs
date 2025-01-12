using CoreLogic.Models;

namespace BillingService.Data;

public interface IBillingRepository
{
    Task<Account> CreateAccount(Account account);
    Task<bool> DeleteAccount(int accountId);
    Task<int> UpdateAccount(Account account);
    Task<Account> GetAccountById(int accountId);
    Task<List<Account>> GetAllAccounts();
    Task<List<Account>> GetAllUserAccounts(int userId);
    Task<Transaction> CreateTransaction(Transaction transaction);
    Task<bool> DeleteTransaction(int transactionId);
    Task<Transaction> GetTransactionById(int transactionId);
    Task<List<Transaction>> GetTransactionsByAccountId(int accountId);
    Task<List<Transaction>> GetAllTransactions();
    Task<bool> ResetDb();
    string GetConnectionInfo();
}
