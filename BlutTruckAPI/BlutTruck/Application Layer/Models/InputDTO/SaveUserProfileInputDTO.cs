namespace BlutTruck.Application_Layer.Models
{
    public class SaveUserProfileInputDTO
    {
        public UserCredentials Credentials { get; set; }
        public PersonalDataModel Profile { get; set; }
    }
}
