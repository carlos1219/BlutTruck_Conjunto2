namespace BlutTruck.Application_Layer.Models
{
    public class LoginUserOutputDTO
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string UserId { get; set; }
        public string ErrorMessage { get; set; }
    }
}
