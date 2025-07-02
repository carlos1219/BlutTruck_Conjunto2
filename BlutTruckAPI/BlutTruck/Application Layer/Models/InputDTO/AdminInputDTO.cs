using System.Globalization;

namespace BlutTruck.Application_Layer.Models.InputDTO
{
    public class AdminInputDTO
    {
        public UserCredentials Credentials { get; set; }
        public string AdminId { get; set; }
        public string UserExtractId { get; set; }
    }
}
