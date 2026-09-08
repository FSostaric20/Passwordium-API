using System.ComponentModel.DataAnnotations;

namespace Passwordium_api.Model.Requests {
    public class CheckPasswordsRequest {
        [Required]
        public int Id { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
