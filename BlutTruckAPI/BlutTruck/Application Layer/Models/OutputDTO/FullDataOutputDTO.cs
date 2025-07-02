namespace BlutTruck.Application_Layer.Models
{
    public class FullDataOutputDTO
    {
        // Datos personales del usuario
        public PersonalDataModel DatosPersonales { get; set; }

        // Diccionario donde la clave es la fecha y el valor los datos del día
        public Dictionary<string, HealthDataOutputModel> Dias { get; set; }

        public string CurrentUserIsAdmin { get; set; }
    }
}
