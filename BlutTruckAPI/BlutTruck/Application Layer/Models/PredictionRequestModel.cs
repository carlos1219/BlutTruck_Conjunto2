namespace BlutTruck.Application_Layer.Models
{
    public class PredictionRequestModel
    {
        public int Edad { get; set; } = 61;
        public int Genero { get; set; } = 1;
        public int AlturaCm { get; set; } = 168;
        public double PesoKg { get; set; } = 74.0;
        public int PresionSistolica { get; set; } = 150;
        public int PresionDiastolica { get; set; } = 90;
        public int Colesterol { get; set; } = 2;
        public int Glucosa { get; set; } = 1;
        public int Fuma { get; set; } = 0;
        public int BebeAlcohol { get; set; } = 0;
        public int Activo { get; set; } = 1;
        public int EnfermedadCardiaca { get; set; } = 1;
    }
}
