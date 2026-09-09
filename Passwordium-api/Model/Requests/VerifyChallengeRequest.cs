using System.ComponentModel.DataAnnotations;

namespace Passwordium_api.Model.Requests {
    public class VerifyChallengeRequest {
        [Required]
        public string? Signature { get; set; }
        [Required]
        public string? PublicKey { get; set; }
    }
}
