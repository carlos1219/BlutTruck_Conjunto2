namespace BlutTruck.Application_Layer.Models
{
    public class RegisterUserOutputDTO
    {
        public bool Success { get; set; }
        public string Token { get; set; }
        public string ErrorMessage { get; set; }
    }
}
