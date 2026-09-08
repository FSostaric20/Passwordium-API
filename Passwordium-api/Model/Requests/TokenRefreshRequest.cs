using System.ComponentModel.DataAnnotations;

namespace Passwordium_api.Model.Requests {
    public class TokenRefreshRequest {
        [Required]
        public required string RefreshToken { get; set; }
    }
}
