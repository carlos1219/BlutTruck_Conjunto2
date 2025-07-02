namespace BlutTruck.Application_Layer.Models
{
    public class ReadDataOutputDTO
    {
        public bool Success { get; set; }
        public string SelectedDate { get; set; }
        public HealthDataOutputModel Data { get; set; }
        public string ErrorMessage { get; set; }
    }
}
