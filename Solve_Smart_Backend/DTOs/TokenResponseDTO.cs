namespace Solve_Smart_Backend.DTOs
{
    public class TokenResponseDTO
    {
        public string Token { get; set; }
        public DateTime ExpiryDate { get; set; }
        public UserDTO User { get; set; }
    }
    public class UserDTO
    {
        public string Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string Jobtitle { get; set; }
        public string PhoneNumber { get; set; }
        public List<string> Roles { get; set; }
    }
}
