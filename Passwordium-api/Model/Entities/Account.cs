namespace Passwordium_api.Model.Entities {
    public class Account {
        public int Id { get; set; }
        public required string Name { get; set; }
        public string? Url { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
        public required int UserId { get; set; }
        public virtual required User User { get; set; }
    }
}
