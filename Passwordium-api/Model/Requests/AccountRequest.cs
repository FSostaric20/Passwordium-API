using System.ComponentModel.DataAnnotations;

namespace Passwordium_api.Model.Requests {
    public class AccountRequest {
        public int Id { get; set; }
        [Required]
        public required string EncryptedData { get; set; }
        public required string Nonce { get; set; }
        public required string Tag { get; set; }
    }
}
