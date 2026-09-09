namespace Passwordium_api.Model.Responses {
    public class LoginResponse {
        public required string JWT { get; set; }
        public required string RefreshToken { get; set; }
        public DateTime RefreshTokenExpiresAt { get; set; }
        public required string VaultSalt { get; set; }
        public required string EncryptedVaultKey { get; set; }
        public required string VaultKeyNonce { get; set; }
        public required string VaultKeyTag { get; set; }
    }
}
