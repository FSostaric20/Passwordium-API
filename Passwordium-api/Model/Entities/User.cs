namespace Passwordium_api.Model.Entities {
    public class User {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public string? RefreshTokenHash { get; set; }
        public DateTime? RefreshTokenExpiresAt { get; set; }
        public string? PublicKey { get; set; }
        public DateTime? ChallengeExpiresAt { get; set; }

        public virtual ICollection<Account>? Accounts { get; set; }
    }
}
