using System.ComponentModel.DataAnnotations;

namespace Passwordium_api.Model.Requests {
    public class PublicKeyRequest {
        [Required]
        public required string PublicKey { get; set; }
    }
}
