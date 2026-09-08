using Microsoft.IdentityModel.Tokens;
using Passwordium_api.Model.Entities;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace Passwordium_api.Services {
    public class TokenService {
        private readonly AppConfiguration _appConfiguration;

        public TokenService(AppConfiguration appConfiguration) {
            _appConfiguration = appConfiguration;
        }

        public string GenerateJwtToken(User user) {
            var tokenHandler =
                new JwtSecurityTokenHandler();

            var key = Encoding.UTF8.GetBytes(
                _appConfiguration.JwtKey
            );

            var tokenDescriptor = new SecurityTokenDescriptor { 
                Subject = new ClaimsIdentity(new[]{
                    new Claim("id", user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Username)
                }),

                Expires = DateTime.UtcNow.AddMinutes(15),

                Issuer = _appConfiguration.JwtIssuer,
                Audience = _appConfiguration.JwtAudience,

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256)
            };

            var token =
                tokenHandler.CreateToken(
                    tokenDescriptor
                );

            return tokenHandler.WriteToken(token);
        }

        public string GenerateRefreshToken() {
            byte[] randomNumber =
                RandomNumberGenerator.GetBytes(32);

            return Convert.ToBase64String(
                randomNumber
            );
        }

        public string HashRefreshToken(
            string refreshToken) {
            byte[] bytes =
                Encoding.UTF8.GetBytes(refreshToken);

            byte[] hash =
                SHA256.HashData(bytes);

            return Convert.ToBase64String(hash);
        }

        public ClaimsPrincipal
            GetPrincipalFromExpiredToken(
                string token) {
            var tokenValidationParameters =
                new TokenValidationParameters {
                    ValidateAudience = true,
                    ValidAudience = _appConfiguration.JwtAudience,
                    ValidateIssuer = true,
                    ValidIssuer = _appConfiguration.JwtIssuer,

                    ValidateIssuerSigningKey = true,

                    IssuerSigningKey =
                        new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                _appConfiguration.JwtKey
                            )
                        ),

                    ValidateLifetime = false
                };

            var tokenHandler =
                new JwtSecurityTokenHandler();

            ClaimsPrincipal principal =
                tokenHandler.ValidateToken(
                    token,
                    tokenValidationParameters,
                    out SecurityToken securityToken
                );

            if (securityToken
                    is not JwtSecurityToken jwt ||
                !jwt.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256,
                    StringComparison.OrdinalIgnoreCase)) {
                throw new SecurityTokenException(
                    "Invalid token."
                );
            }

            return principal;
        }
    }
}