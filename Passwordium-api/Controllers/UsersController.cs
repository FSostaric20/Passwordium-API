using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwordium_api.Data;
using Passwordium_api.Model.Requests;
using Passwordium_api.Model.Responses;
using Passwordium_api.Services;
using System.Security.Cryptography;

namespace Passwordium_api.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase {
        private readonly UserService _userService;
        private readonly DatabaseContext _context;
        private readonly TokenService _tokenService;

        public UsersController(UserService userService, DatabaseContext context, TokenService tokenService) {
            _userService = userService;
            _context = context;
            _tokenService = tokenService;
        }

        // POST: api/Users/Login
        [HttpPost("Login")]
        public async Task<ActionResult<LoginResponse>> Login(UserRequest request) {
            try {
                LoginResponse response = await _userService.LoginAsync(request);

                return Ok(response);
            } catch (InvalidDataException ex) {
                return Unauthorized(new { message = ex.Message });
            } catch (Exception ex) {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/Users/Register
        [HttpPost("Register")]
        public async Task<IActionResult> Register(RegisterRequest request) {
            try {
                await _userService.RegisterAsync(request);

                return Ok(new {
                    message = "User added to database!"
                });
            } catch (InvalidDataException ex) {
                return Conflict(new { message = ex.Message });
            }
        }

        // POST: api/Users/TokenRefresh
        [HttpPost("TokenRefresh")]
        public async Task<ActionResult<LoginResponse>> TokenRefresh(
            TokenRefreshRequest request) {
            try {
                string? jwt = HttpContext.Request
                    .Headers["Authorization"]
                    .FirstOrDefault()?
                    .Split(" ")
                    .Last();

                if (string.IsNullOrWhiteSpace(jwt)) {
                    return Unauthorized();
                }

                LoginResponse response =
                    await _userService.TokenRefreshAsync(
                        request,
                        jwt
                    );

                return Ok(response);
            } catch {
                return Unauthorized();
            }
        }

        // POST: api/Users/PublicKey
        [Authorize]
        [HttpPost("PublicKey")]
        public async Task<IActionResult> PublicKey( 
            PublicKeyRequest request) {
            string? userIdClaim = User.FindFirst("id")?.Value;

            if (!int.TryParse(userIdClaim, out int userId)) {
                return Unauthorized();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) {
                return NotFound();
            }

            user.PublicKey = request.PublicKey;

            await _context.SaveChangesAsync();

            return Ok(new {
                message = "Public key stored!"
            });
        }

        // POST: api/Users/Challenge
        [HttpPost("Challenge")]
        public async Task<IActionResult> Challenge(
            PublicKeyRequest request) {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PublicKey == request.PublicKey);

            if (user == null) {
                return NotFound(new {
                    message = "User does not exist."
                });
            }

            byte[] challengeBytes = RandomNumberGenerator.GetBytes(32);

            string challenge = Convert.ToBase64String(challengeBytes);

            user.Challenge = challenge;

            user.ChallengeExpiresAt = DateTime.UtcNow.AddMinutes(2);

            await _context.SaveChangesAsync();

            return Ok(new {challenge});
        }

        // POST: api/Users/VerifyChallenge
        [HttpPost("VerifyChallenge")]
        public async Task<IActionResult> VerifyChallenge(VerifyChallengeRequest request) {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.PublicKey == request.PublicKey);

            if (user == null) {
                return NotFound();
            }

            if (user.Challenge == null ||
                user.ChallengeExpiresAt == null ||
                user.ChallengeExpiresAt <= DateTime.UtcNow) {
                return Unauthorized(new {message = "Challenge expired or invalid."});
            }

            try {
                using ECDsa ecdsa = ECDsa.Create();

                byte[] publicKeyBytes =Convert.FromBase64String(request.PublicKey);

                ecdsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                byte[] signatureBytes = Convert.FromBase64String(request.Signature);

                byte[] challengeBytes = Convert.FromBase64String(user.Challenge);

                bool signatureIsValid =
                    ecdsa.VerifyData(
                        challengeBytes,
                        signatureBytes,
                        HashAlgorithmName.SHA256,
                        DSASignatureFormat.Rfc3279DerSequence
                    );

                if (!signatureIsValid) {
                    return Unauthorized(new {
                        message = "Signature is not valid."
                    });
                }

                user.Challenge = null;
                user.ChallengeExpiresAt = null;

                string jwt =
                    _tokenService.GenerateJwtToken(user);

                string refreshToken =
                    _tokenService.GenerateRefreshToken();

                user.RefreshTokenHash =
                    _tokenService.HashRefreshToken(
                        refreshToken
                    );

                user.RefreshTokenExpiresAt =
                    DateTime.UtcNow.AddDays(1);

                await _context.SaveChangesAsync();

                return Ok(new LoginResponse {
                    JWT = jwt,
                    RefreshToken = refreshToken,
                    RefreshTokenExpiresAt = user.RefreshTokenExpiresAt.Value,
                    VaultSalt = user.VaultSalt,
                    EncryptedVaultKey = user.EncryptedVaultKey,
                    VaultKeyNonce = user.VaultKeyNonce,
                    VaultKeyTag = user.VaultKeyTag
                });
            } catch (
                  CryptographicException) {
                return Unauthorized(new {
                    message = "Invalid challenge response."
                });
            }
        }
    }
}
