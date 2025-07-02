namespace BlutTruck.Application_Layer.Models
{
    public class WriteDataInputDTO
    {
        public UserCredentials Credentials { get; set; }
        public HealthDataInputModel HealthData { get; set; }
    }
}
