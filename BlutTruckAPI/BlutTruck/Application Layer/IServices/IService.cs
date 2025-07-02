using System.Threading.Tasks;
using BlutTruck.Application_Layer.Models;
using BlutTruck.Application_Layer.Models.InputDTO;
using BlutTruck.Application_Layer.Models.OutputDTO;
using static BlutTruck.Application_Layer.Models.PersonalDataModel;
using static BlutTruck.Data_Access_Layer.Repositories.HealthDataRepository;

namespace BlutTruck.Application_Layer.IServices
{
    public interface IHealthDataService
    {
        /// <summary>
        /// Autentica al usuario y obtiene el token de autenticación.
        /// </summary>
        /// <returns>Token de autenticación como cadena.</returns>
        Task<string> AuthenticateAndGetTokenAsync();

        /// <summary>
        /// Verifica la validez de un token de usuario.
        /// </summary>
        /// <param name="idToken">El token de usuario a verificar.</param>
        /// <returns>El UID del usuario autenticado.</returns>
        Task<string> VerifyUserTokenAsync(string idToken);

        Task WriteDataAsync(WriteDataInputDTO request);
        Task WriteDataWebAsync(WriteDataInputDTO request);
        Task writePredictionAsync(PredictionInputDTO request);
        Task<ReadDataOutputDTO> ReadDataAsync(ReadDataInputDTO request);
        Task<HealthDataOutputModel> GetSelectDateHealthDataAsync(SelectDateHealthDataInputDTO request);
        Task<FullDataOutputDTO> GetFullHealthDataAsync(UserCredentials credentials);
        Task<FullDataOutputDTO> GetFullHealthDataAndConnectAsync(FullAndMonitoringInputDTO request);
        Task<SaveUserProfileOutputDTO> SaveUserProfileAsync(SaveUserProfileInputDTO request);
        Task<PersonalAndLatestDayDataOutputDTO> GetPersonalAndLatestDayDataAsync(GetPersonalAndLatestDayDataInputDTO request);
        Task<GetPersonalDataOutputDTO> GetPersonalDataAsync(GetPersonalDataInputDTO request);
        Task<UpdateConnectionStatusOutputDTO> UpdateConnectionStatusAsync(UpdateConnectionStatusInputDTO request);
        Task<GetConnectionStatusOutputDTO> GetConnectionStatusAsync(GetConnectionStatusInputDTO request);
        Task<RegisterConnectionOutputDTO> RegisterConnectionAsync(RegisterConnectionInputDTO request);
        Task<DeleteConnectionOutputDTO> DeleteConnectionAsync(DeleteConnectionInputDTO request);
        Task<List<ConnectedUserModel>> GetConnectedUsersAsync(ConnectedUsersInputDTO request);
        Task<RegisterUserOutputDTO> RegisterUserAsync(RegisterUserInputDTO request);
        Task<RegisterUserOutputDTO> RegisterGoogleUserAsync(RegisterGoogleUserInputDTO request);
        Task<LoginUserOutputDTO> LoginUserAsync(LoginUserInputDTO request);
        Task<GetMonitoringUsersOutputDTO> GetMonitoringUsersAsync(GetMonitoringUsersInputDTO request);
        Task ChangePasswordAsync(ChangePasswordRequestInputDTO input);
        Task<PdfOutputDTO> GeneratePdfAsync(PdfInputDTO request);
        Task<RegisterConnectionOutputDTO> RegisterCodeConnectionAsync(RegisterCodeConnectionInputDTO request);
        Task<DeleteConnectionOutputDTO> DeleteCodeConnectionAsync(DeleteCodeConnectionInputDTO request);
        Task<GetConnectionOutputDTO> GetCodeConnectionAsync(DeleteCodeConnectionInputDTO request);
        Task<DeleteUserOutputDTO> DeleteDataUserAsync(DeleteUserInputDTO request);
        Task<DeleteUserOutputDTO> DeleteUserAsync(DeleteUserInputDTO request);
        Task<PredictionDataDTO> GetPredictionAsync(UserCredentials credentials);
        Task<IEnumerable<PredictionDataDTO>> GetListPredictionAsync(UserCredentials credentials);
        Task writeAdminAsync(AdminInputDTO request);
        Task<PdfOutputDTO> GeneratePdfByDataTypeAsync(PdfInputByDataTypeDTO request);
        Task<GetFavoritesOutputDTO> GetFavoritesAsync(UserCredentials request);
        Task<BaseOutputDTO> SetFavoritesAsync(SetFavoritesInputDTO request);
    }
}
