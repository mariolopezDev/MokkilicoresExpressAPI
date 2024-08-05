namespace MokkilicoresExpressAPI.Models
{
    public class LoginRequest
    {
        public required string Identificacion { get; set; }
        public required string Password { get; set; }
    }

    public class LoginResponse
    {
        public required string Token { get; set; }
        public required string Identificacion { get; set; }
        public required string Role { get; set; }
    }
}
