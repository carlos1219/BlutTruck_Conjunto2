namespace BlutTruck.Application_Layer.Models.OutputDTO
{
    // Un DTO base para las respuestas de la API, indica si la operación fue exitosa.
    public class BaseOutputDTO
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
    }
}
