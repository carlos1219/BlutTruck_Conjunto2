namespace BlutTruck.Application_Layer.Models
{
    public class DeleteConnectionInputDTO
    {
        public UserCredentials Credentials { get; set; }
        public string ConnectedUserId { get; set; }
    }
}
