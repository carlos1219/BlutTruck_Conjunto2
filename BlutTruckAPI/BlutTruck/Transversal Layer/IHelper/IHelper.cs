using BlutTruck.Application_Layer.Models;
namespace BlutTruck.Transversal_Layer.IHelper
{
    public interface IHelper
    {
        string GetCurrentDateKey();
        Dictionary<string, object> FormatHealthData(HealthDataInputModel data);
    }
}
