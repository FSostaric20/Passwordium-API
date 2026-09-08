using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Passwordium_api.Data;
using Passwordium_api.Model.Entities;
using Passwordium_api.Model.Requests;
using Passwordium_api.Model.Responses;
using System.Security.Cryptography;

namespace Passwordium_api.Services {
    public class UserService {
        private readonly DatabaseContext _context;
        private readonly TokenService _tokenService;
        private readonly HashService _hashService;

        public UserService(DatabaseContext context, TokenService tokenService, HashService hashService) {
            _context = context;
            _tokenService = tokenService;
            _hashService = hashService;
        }

        public async Task<LoginResponse> LoginAsync(UserRequest request) {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username) ?? throw new UnauthorizedAccessException("Invalid username or password.");
            bool passwordValid = _hashService.VerifyHashedPassword(user, request.Password);

            if (!passwordValid) {
                throw new UnauthorizedAccessException(
                    "Invalid username or password."
                );
            }

            string jwt = _tokenService.GenerateJwtToken(user);
            string refreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshTokenHash = _tokenService.HashRefreshToken(refreshToken);
            user.RefreshTokenExpiresAt = DateTime.UtcNow.AddDays(1);

            await _context.SaveChangesAsync();

            LoginResponse response = new LoginResponse {
                JWT = jwt,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = (DateTime)user.RefreshTokenExpiresAt
            };

            return response;
        }

        public async Task RegisterAsync(UserRequest request) {
            var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == request.Username);
            if (existingUser != null) {
                throw new InvalidDataException("A user with this username already exists.");
            }

            User newUser = new User {
                Username = request.Username,
                Password = request.Password
            };

            newUser = _hashService.HashPassword(newUser);

            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();
        }

        public async Task<LoginResponse> TokenRefreshAsync(TokenRefreshRequest request, string jwtToken) {
            var principal =
                _tokenService.GetPrincipalFromExpiredToken(
                    jwtToken
                );

            if (string.IsNullOrWhiteSpace(request.RefreshToken)) {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token."
                );
            }

            string? userIdClaim =
                principal.FindFirst("id")?.Value;

            if (!int.TryParse(userIdClaim, out int userId)) {
                throw new UnauthorizedAccessException(
                    "Invalid token."
                );
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null ||
                user.RefreshTokenHash == null ||
                user.RefreshTokenExpiresAt == null ||
                user.RefreshTokenExpiresAt <= DateTime.UtcNow) {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token."
                );
            }

            string incomingHash =
                _tokenService.HashRefreshToken(
                    request.RefreshToken
                );

            byte[] incomingHashBytes =
                Convert.FromBase64String(incomingHash);

            byte[] storedHashBytes =
                Convert.FromBase64String(
                    user.RefreshTokenHash
                );

            if (!CryptographicOperations.FixedTimeEquals(
                    incomingHashBytes,
                    storedHashBytes)) {
                throw new UnauthorizedAccessException(
                    "Invalid refresh token."
                );
            }

            string newJwt =
                _tokenService.GenerateJwtToken(user);

            string newRefreshToken =
                _tokenService.GenerateRefreshToken();

            user.RefreshTokenHash =
                _tokenService.HashRefreshToken(
                    newRefreshToken
                );

            user.RefreshTokenExpiresAt =
                DateTime.UtcNow.AddDays(1);

            await _context.SaveChangesAsync();

            return new LoginResponse {
                JWT = newJwt,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt =
                    user.RefreshTokenExpiresAt.Value
            };
        }
    }
}
