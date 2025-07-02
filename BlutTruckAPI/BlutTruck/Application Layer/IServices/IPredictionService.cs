namespace BlutTruck.Application_Layer.IServices
{
    public interface IPrediccionService
    {
        Task<string> PredecirAsync(Dictionary<string, object> datos);
    }

}
