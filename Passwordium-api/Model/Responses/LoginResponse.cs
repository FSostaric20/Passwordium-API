namespace Passwordium_api.Model.Responses {
    public class LoginResponse {
        public required string JWT { get; set; }
        public required string RefreshToken { get; set; }
        public required DateTime RefreshTokenExpiresAt { get; set; }
    }
}
