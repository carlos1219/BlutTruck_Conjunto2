namespace BlutTruck.Application_Layer.Models.InputDTO
{
    public class RegisterCodeConnectionInputDTO
    {
        public string CurrentUserId { get; set; }
        public string Code { get; set; }
        public string IdToken { get; set; }
    }
}
