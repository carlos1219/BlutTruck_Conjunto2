namespace BlutTruck.Application_Layer.Models
{
    public class PersonalDataModel
    {
        public class ConnectionModel
        {
            public int? ConnectionStatus { get; set; }
        }
        public ConnectionModel Conexion { get; set; }
        public string DateOfBirth { get; set; }
        public bool? HasPredisposition { get; set; }
        public int? Height { get; set; }
        public int? Weight { get; set; }
        public int? Gender { get; set; }
        public int? Smoke { get; set; }
        public int? Alcohol { get; set; }
        public int? Choresterol { get; set; }
        public string? PhotoURL { get; set; }
        public string? Name { get; set; }
        public bool? Active { get; set; }

    }
}
