namespace BlutTruck.Application_Layer.Models
{
    public class ConnectedUserModel
    {
        public string ConnectedUserId { get; set; }
        public string Name { get; set; }
        public string PhotoURL { get; set; }
        public string LatestDay { get; set; }
        public string MaxHeartRate { get; set; }
        public string MinHeartRate { get; set; }
        public string AvgHeartRate { get; set; }
    }

}
