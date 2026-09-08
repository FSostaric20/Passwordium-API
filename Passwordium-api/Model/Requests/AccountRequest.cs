using System.ComponentModel.DataAnnotations;

namespace Passwordium_api.Model.Requests {
    public class AccountRequest {
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Url { get; set; }
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}
