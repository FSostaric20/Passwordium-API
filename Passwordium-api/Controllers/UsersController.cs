using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Passwordium_api.Data;
using Passwordium_api.Model.Requests;
using Passwordium_api.Model.Responses;
using Passwordium_api.Services;
using System.Security.Cryptography;
using System.Text;

namespace Passwordium_api.Controllers {
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase {
        private readonly UserService _userService;
        private readonly DatabaseContext _context;
        private readonly TokenService _tokenService;
        private readonly HashService _hashService;

        public UsersController(UserService userService, DatabaseContext context, TokenService tokenService, HashService hashService) {
            _userService = userService;
            _context = context;
            _tokenService = tokenService;
            _hashService = hashService;
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
        public async Task<IActionResult> Register(UserRequest request) {
            try {
                await _userService.RegisterAsync(request);

                return Ok(new { message = "User added to database!" });
            } catch (InvalidDataException ex) {
                return Conflict(new { message = ex.Message });
            } catch (Exception ex) {
                return NotFound(new { message = ex.Message });
            }
        }

        // POST: api/Users/TokenRefresh
        [Authorize(AuthenticationSchemes = "NoExpiryCheck")]
        [HttpPost("TokenRefresh")]
        public async Task<ActionResult<LoginResponse>> TokenRefresh(TokenRefreshRequest request) {
            try {
                string jwt = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                LoginResponse response = await _userService.TokenRefreshAsync(request, jwt);

                return Ok(response);
            } catch (Exception ex) {
                return Unauthorized(new { message = ex.Message });
            }

        }

        // POST: api/Users/PublicKey
        [Authorize]
        [HttpPost("PublicKey")]
        public async Task<IActionResult> PublicKey(PublicKeyRequest request) {
            string jwt = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            int userId = _tokenService.GetUserIdFromJWT(jwt);

            var userReal = await _context.Users.FirstOrDefaultAsync(a => a.Id == userId);
            if (userReal == null) {
                return NotFound(new { message = "User does not exists." });
            }

            userReal.PublicKey = request.PublicKey;

            _context.Entry(userReal).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                return NotFound(new { message = "Did not add public key." });
            }

            return Ok(new { message = "Public key stored!" });
        }

        // POST: api/Users/Challenge
        [HttpPost("Challenge")]
        public async Task<IActionResult> challenge(PublicKeyRequest response) {
            var user = await _context.Users.FirstOrDefaultAsync(a => a.PublicKey == response.PublicKey);
            if (user == null) {
                return NotFound(new { message = "User does not exists." });
            }

            user.ChallengeExpiresAt = DateTime.UtcNow.AddMinutes(2);
            _context.Entry(user).State = EntityState.Modified;

            try {
                await _context.SaveChangesAsync();
            } catch (DbUpdateConcurrencyException) {
                return NotFound(new { message = "Did not generate challenge." });
            }

            string hash = _hashService.GetHash(response.PublicKey + user.ChallengeExpiresAt);
            return Ok(new { message = hash });
        }

        // POST: api/Users/VerifyChallenge
        [HttpPost("VerifyChallenge")]
        public async Task<IActionResult> VerifyChallenge(VerifyChallengeRequest request) {
            var user = await _context.Users.FirstOrDefaultAsync(a => a.PublicKey == request.PublicKey);
            if (user == null) {
                return NotFound(new { message = "User does not exists." });
            }

            if (user.ChallengeExpiresAt < DateTime.UtcNow) {
                return Unauthorized(new { message = "Challenge expired." });
            }

            try {
                using (ECDsa ECDsa = ECDsa.Create()) {
                    byte[] publicKeyBytes = Convert.FromBase64String(request.PublicKey);
                    ECDsa.ImportSubjectPublicKeyInfo(publicKeyBytes, out _);

                    byte[] signatureBytes = Convert.FromBase64String(request.Signature);
                    string data = _hashService.GetHash(request.PublicKey + user.ChallengeExpiresAt);
                    byte[] dataBytes = Encoding.UTF8.GetBytes(data);

                    bool signatureIsValid = ECDsa.VerifyData(dataBytes, signatureBytes, HashAlgorithmName.SHA256, DSASignatureFormat.Rfc3279DerSequence);

                    if (signatureIsValid) {
                        string jwt = _tokenService.GenerateJwtToken(user);
                        user = _tokenService.GenerateRefreshToken(user, _context);

                        LoginResponse response = new LoginResponse {
                            JWT = jwt,
                            RefreshToken = user.RefreshTokenHash,
                            RefreshTokenExpiresAt = (DateTime)user.RefreshTokenExpiresAt
                        };

                        return Ok(response);
                    } else {
                        return Unauthorized(new { message = "Signature is not valid." });
                    }
                }
            } catch (Exception ex) {
                return Unauthorized(new { message = ex.Message });
            }
        }
    }
}
