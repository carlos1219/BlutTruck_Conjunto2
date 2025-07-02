namespace BlutTruck.Application_Layer.Models
{
    public class PersonalAndLatestDayDataOutputDTO
    {
        public bool Success { get; set; }
        public PersonalDataModel PersonalData { get; set; }
        public LatestDayData DiaMasReciente { get; set; }
        public string ErrorMessage { get; set; }
    }
}
