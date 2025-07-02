using BlutTruck.Application_Layer.IServices;
using BlutTruck.Data_Access_Layer.IRepositories;

namespace BlutTruck.Application_Layer.Services
{
    public class PrediccionService : IPrediccionService
    {
        private readonly IApiRepository _apiRepository;

        public PrediccionService(IApiRepository apiRepository)
        {
            _apiRepository = apiRepository;
        }

        public async Task<string> PredecirAsync(Dictionary<string, object> datos)
        {
            try
            {
                return await _apiRepository.PredecirAsync(datos);
            }
            catch (Exception ex)
            {
                throw new Exception($"Error en el servicio: {ex.Message}");
            }
        }
    }
}
