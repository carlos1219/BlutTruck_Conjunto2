namespace BlutTruck.Application_Layer.Models
{
    public class GetUserEmailByIdOutputDTO
    {
        public bool Success { get; set; }
        public string Email { get; set; }
        public string ErrorMessage { get; set; }
    }
}
