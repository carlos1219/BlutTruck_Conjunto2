namespace BlutTruck.Data_Access_Layer.IRepositories
{
    public interface IApiRepository
    {
        Task<string> PredecirAsync(Dictionary<string, object> datos);
    }
}
