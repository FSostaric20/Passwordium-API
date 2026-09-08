namespace Passwordium_api {
    public class AppConfiguration {
        public string JwtKey { get; set; } = null!;
        public string JwtIssuer { get; set; } = null!;
        public string JwtAudience { get; set; } = null!;
    }
}
