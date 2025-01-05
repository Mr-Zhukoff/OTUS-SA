using CoreLogic.Models;
using Microsoft.EntityFrameworkCore;
using System.Security.Principal;

namespace BillingService.Data;

public class BillingRepository(BillingDbContext context) : IBillingRepository
{
    private readonly BillingDbContext _context = context;

    public async Task<Account> CreateAccount(Account account)
    {
        var result = await _context.Accounts.AddAsync(account);
        await _context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<Transaction> CreateTransaction(Transaction transaction)
    {
        var result = await _context.Transactions.AddAsync(transaction);
        await _context.SaveChangesAsync();

        return result.Entity;
    }

    public async Task<bool> DeleteAccount(int accountId)
    {
        await _context.Accounts.Where(e => e.Id == accountId).ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTransaction(int transactionId)
    {
        await _context.Transactions.Where(e => e.Id == transactionId).ExecuteDeleteAsync();
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Account> GetAccountById(int accountId)
    {
        var account = await _context.Accounts.Where(u => u.Id == accountId).FirstOrDefaultAsync();
        return (account == null) ? null : account;
    }

    public async Task<List<Account>> GetAllAccounts()
    {
        var accounts = await _context.Accounts.AsNoTracking().ToListAsync();

        return accounts.ToList();
    }

    public async Task<List<Transaction>> GetAllTransactions()
    {
        var transactions = await _context.Transactions.AsNoTracking().ToListAsync();

        return transactions.ToList();
    }

    public async Task<Transaction> GetTransactionById(int transactionId)
    {
        var transaction = await _context.Transactions.Where(u => u.Id == transactionId).FirstOrDefaultAsync();
        return (transaction == null) ? null : transaction;
    }

    public async Task<List<Transaction>> GetTransactionsByAccountId(int accountId)
    {
        var transactions = await _context.Transactions.Where(u => u.AccountId == accountId).ToListAsync();

        return transactions.ToList();
    }

    public async Task<bool> ResetDb()
    {
        await _context.Database.EnsureDeletedAsync();
        var result = await _context.Database.EnsureCreatedAsync();
        return result;
    }

    public async Task<int> UpdateAccount(Account account)
    {
        await _context.Accounts.Where(e => e.Id == account.Id)
            .ExecuteUpdateAsync(x => x
            .SetProperty(p => p.UserId, p => account.UserId)
            .SetProperty(p => p.Number, p => account.Number)
            .SetProperty(p => p.Description, p => account.Description)
            .SetProperty(p => p.Balance, p => account.Balance)
            );

        await _context.SaveChangesAsync();

        return account.Id;
    }

    public async Task<int> UpdateAccountPartial(Account account)
    {
        var existingAccount = await _context.Accounts.Where(e => e.Id == account.Id).FirstOrDefaultAsync();

        if (existingAccount == null)
            return 0;

        if (account.UserId != existingAccount.UserId)
            existingAccount.UserId = account.UserId;

        if (account.Number != null)
            existingAccount.Number = account.Number;

        if (account.Description != null)
            existingAccount.Description = account.Description;

        if (account.Balance != existingAccount.Balance)
            existingAccount.Balance = account.Balance;

        _context.Accounts.Update(existingAccount);
        await _context.SaveChangesAsync();
        return account.Id;
    }
}
