namespace BlutTruck.Application_Layer.Models
{
    public class GetConnectionStatusOutputDTO
    {
        public bool Success { get; set; }
        public int? ConnectionStatus { get; set; }
        public string ErrorMessage { get; set; }
    }
}
