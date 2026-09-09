namespace Passwordium_api.Model.Responses {
    public class AccountResponse {
        public int Id { get; set; }
        public required string EncryptedData { get; set; }
        public required string Nonce { get; set; }
        public required string Tag { get; set; }
    }
}
