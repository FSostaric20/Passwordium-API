using System.ComponentModel.DataAnnotations;

namespace Passwordium_api.Model.Requests {
    public class UserRequest {
        [Required]
        public required string Username { get; set; }
        [Required]
        public required string Password { get; set; }
    }
}
