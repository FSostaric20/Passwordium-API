using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwordium_api.Data;
using Passwordium_api.Model.Entities;
using Passwordium_api.Model.Requests;
using Passwordium_api.Model.Responses;
using Passwordium_api.Services;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;

namespace Passwordium_api.Controllers {
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase {
        private readonly DatabaseContext _context;

        public AccountsController(DatabaseContext context) {
            _context = context;
        }

        // GET: api/Accounts
        [HttpGet]
        public async Task<ActionResult<List<AccountResponse>>> GetAccounts() {
            int userId = GetCurrentUserId();

            var accounts = await _context.Accounts
                .Where(a => a.UserId == userId)
                .Select(a => new AccountResponse {
                    Id = a.Id,
                    EncryptedData = a.EncryptedData,
                    Nonce = a.Nonce,
                    Tag = a.Tag
                })
                .ToListAsync();

            return Ok(accounts);
        }

        // PUT:/api/Accounts
        [HttpPut]
        public async Task<IActionResult> PutAccount(AccountRequest account) {
            int userId = GetCurrentUserId();

            var existingAccount = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.Id == account.Id &&
                    a.UserId == userId);

            if (existingAccount == null) {
                return NotFound();
            }

            existingAccount.EncryptedData =
                account.EncryptedData;

            existingAccount.Nonce =
                account.Nonce;

            existingAccount.Tag =
                account.Tag;

            await _context.SaveChangesAsync();

            return Ok(new {
                message = "Account updated!"
            });
        }

        // POST: api/Accounts
        [HttpPost]
        public async Task<IActionResult> PostAccount(AccountRequest account) {
            int userId = GetCurrentUserId();

            Account newAccount = new Account {
                EncryptedData = account.EncryptedData,
                Nonce = account.Nonce,
                Tag = account.Tag,
                UserId = userId,
                User = await _context.Users.FirstAsync(u => u.Id == userId)
            };

            _context.Accounts.Add(newAccount);
            await _context.SaveChangesAsync();

            return Ok(new {
                message = "Account added to database!"
            });
        }

        // DELETE: api/Accounts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAccount(int id) {
            int userId = GetCurrentUserId();

            var account = await _context.Accounts
                .FirstOrDefaultAsync(a =>
                    a.Id == id &&
                    a.UserId == userId
                );

            if (account == null) {
                return NotFound();
            }

            _context.Accounts.Remove(account);
            await _context.SaveChangesAsync();

            return Ok(new {
                message = "Account deleted from database!"
            });
        }

        private int GetCurrentUserId() {
            string? userIdClaim = User.FindFirst("id")?.Value;

            if (!int.TryParse(userIdClaim, out int userId)) {
                throw new UnauthorizedAccessException("Invalid user.");
            }

            return userId;
        }
    }
}
