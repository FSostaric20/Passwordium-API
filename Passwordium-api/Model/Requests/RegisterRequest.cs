namespace Passwordium_api.Model.Requests {
    public class RegisterRequest {
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required string VaultSalt { get; set; }
        public required string EncryptedVaultKey { get; set; }
        public required string VaultKeyNonce { get; set; }
        public required string VaultKeyTag { get; set; }
    }
}
