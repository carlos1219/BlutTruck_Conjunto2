namespace BlutTruck.Application_Layer.Models
{
    public class GetPersonalDataOutputDTO
    {
        public bool Success { get; set; }
        public PersonalDataModel PersonalData { get; set; }
        public string ErrorMessage { get; set; }
    }
}
