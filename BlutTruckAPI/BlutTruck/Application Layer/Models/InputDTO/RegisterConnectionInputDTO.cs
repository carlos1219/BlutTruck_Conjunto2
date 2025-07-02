namespace BlutTruck.Application_Layer.Models
{
    public class RegisterConnectionInputDTO
    {
        public string CurrentUserId { get; set; }
        public string ConnectedUserId { get; set; }
        public string IdToken { get; set; }
    }
}
