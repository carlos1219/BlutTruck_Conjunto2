using static BlutTruck.Application_Layer.Models.PersonalDataModel;

namespace BlutTruck.Application_Layer.Models
{
    public class UpdateConnectionStatusInputDTO
    {
        public UserCredentials Credentials { get; set; }
        public ConnectionModel ConnectionStatus { get; set; }
    }
}
