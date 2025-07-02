namespace BlutTruck.Application_Layer.Models.InputDTO
{
    public class PredictionInputDTO
    {
        public UserCredentials Credentials { get; set; }
        public string Prediction { get; set; }
        public List<string> SpecificAlerts { get; set; } = new List<string>(); 
    }
}
