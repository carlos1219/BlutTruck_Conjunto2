namespace BlutTruck.Application_Layer.Models
{
    public class GetMonitoringUsersOutputDTO
    {
        public bool Success { get; set; }
        public List<MonitorUserModel> MonitoringUsers { get; set; }
        public string ErrorMessage { get; set; }
    }
}
