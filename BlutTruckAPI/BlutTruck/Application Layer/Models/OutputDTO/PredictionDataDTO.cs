namespace BlutTruck.Application_Layer.Models.OutputDTO
{
    public class PredictionDataDTO
    {
        public string Resultado { get; set; }
        public List<string> Alertas { get; set; } 
        public string Nombre { get; set; }
    }
}
