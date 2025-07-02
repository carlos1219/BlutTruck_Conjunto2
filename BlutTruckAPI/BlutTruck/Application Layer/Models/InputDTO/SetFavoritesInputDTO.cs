namespace BlutTruck.Application_Layer.Models.InputDTO
{
    public class SetFavoritesInputDTO
    {
        // Reutilizamos la misma estructura de credenciales
        public UserCredentials Credentials { get; set; }

        // La lista de identificadores de las tarjetas favoritas (ej: ["calorias", "peso"])
        public List<string> Favorites { get; set; }
    }
}
