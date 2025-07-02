namespace Application.Services
{
    using BlutTruck.Application_Layer.IServices;
    using BlutTruck.Data_Access_Layer.IRepositories;
    using BlutTruck.Domain_Layer.Entities;
    using BlutTruck.Transversal_Layer.IHelper;
    using System.Threading.Tasks;
    using BlutTruck.Application_Layer.Models;
    using static System.Runtime.InteropServices.JavaScript.JSType;
    using static BlutTruck.Application_Layer.Models.PersonalDataModel;
    using Firebase.Auth;
    using Google.Type;
    using static BlutTruck.Data_Access_Layer.Repositories.HealthDataRepository;
    using BlutTruck.Application_Layer.Models.InputDTO;
    using BlutTruck.Application_Layer.Models.OutputDTO;

    public class HealthDataService : IHealthDataService
    {
        private readonly IHealthDataRepository _healthDataRepository;

        public HealthDataService(IHealthDataRepository healthDataRepository)
        {
            _healthDataRepository = healthDataRepository;
        }

        public async Task<string> AuthenticateAndGetTokenAsync()
        {
            return await _healthDataRepository.GetTokenAsync();
        }

        public async Task<string> VerifyUserTokenAsync(string idToken)
        {
            return await _healthDataRepository.VerifyIdTokenAsync(idToken);
        }

        public async Task<DeleteUserOutputDTO> DeleteDataUserAsync(DeleteUserInputDTO request)
        {
            return await _healthDataRepository.DeleteDataUserAsync(request);
        }

        public async Task<DeleteUserOutputDTO> DeleteUserAsync(DeleteUserInputDTO request)
        {
            return await _healthDataRepository.DeleteUserAsync(request);
        }
        public Task WriteDataAsync(WriteDataInputDTO request)
        {
            return _healthDataRepository.WriteDataAsync(request);
        }

        public Task WriteDataWebAsync(WriteDataInputDTO request)
        {
            return _healthDataRepository.WriteDataWebAsync(request);
        }

        public Task writePredictionAsync(PredictionInputDTO request)
        {
            return _healthDataRepository.writePredictionAsync(request);
        }

        public Task writeAdminAsync(AdminInputDTO request)
        {
            return _healthDataRepository.writeAdminAsync(request);
        }

        public Task<ReadDataOutputDTO> ReadDataAsync(ReadDataInputDTO request)
        {
            return _healthDataRepository.ReadDataAsync(request);
        }

        public Task<HealthDataOutputModel> GetSelectDateHealthDataAsync(SelectDateHealthDataInputDTO request)
        {
            if (string.IsNullOrEmpty(request.Credentials.UserId) || string.IsNullOrEmpty(request.DateKey))
            {
                throw new ArgumentException("El UserId y la fecha no pueden estar vacíos.");
            }
            return _healthDataRepository.GetSelectDateHealthDataAsync(request);
        }

        public Task<FullDataOutputDTO> GetFullHealthDataAsync(UserCredentials credentials)
        {
            return _healthDataRepository.GetFullHealthDataAsync(credentials);
        }

        public Task<FullDataOutputDTO> GetFullHealthDataAndConnectAsync(FullAndMonitoringInputDTO request)
        {
            return _healthDataRepository.GetFullHealthDataAndConnectAsync(request);
        }

        public Task<SaveUserProfileOutputDTO> SaveUserProfileAsync(SaveUserProfileInputDTO request)
        {
            return _healthDataRepository.SaveUserProfileAsync(request);
        }

        public Task<PersonalAndLatestDayDataOutputDTO> GetPersonalAndLatestDayDataAsync(GetPersonalAndLatestDayDataInputDTO request)
        {
            return _healthDataRepository.GetPersonalAndLatestDayDataAsync(request);
        }

        public Task<GetPersonalDataOutputDTO> GetPersonalDataAsync(GetPersonalDataInputDTO request)
        {
            return _healthDataRepository.GetPersonalDataAsync(request);
        }

        public Task<UpdateConnectionStatusOutputDTO> UpdateConnectionStatusAsync(UpdateConnectionStatusInputDTO request)
        {
            return _healthDataRepository.UpdateConnectionStatusAsync(request);
        }

        public Task<GetConnectionStatusOutputDTO> GetConnectionStatusAsync(GetConnectionStatusInputDTO request)
        {
            return _healthDataRepository.GetConnectionStatusAsync(request);
        }

        public Task<RegisterConnectionOutputDTO> RegisterConnectionAsync(RegisterConnectionInputDTO request)
        {
            return _healthDataRepository.RegisterConnectionAsync(request);
        }

        public Task<RegisterConnectionOutputDTO> RegisterCodeConnectionAsync(RegisterCodeConnectionInputDTO request)
        {
            return _healthDataRepository.RegisterCodeConnectionAsync(request);

        }
        public Task<PredictionDataDTO> GetPredictionAsync(UserCredentials credentials)
        {
            return _healthDataRepository.GetPredictionAsync(credentials);

        }

        public Task<IEnumerable<PredictionDataDTO>> GetListPredictionAsync(UserCredentials credentials)
        {
            return _healthDataRepository.GetListPredictionAsync(credentials);

        }
        public Task<DeleteConnectionOutputDTO> DeleteCodeConnectionAsync(DeleteCodeConnectionInputDTO request)
        {
            return _healthDataRepository.DeleteCodeConnectionAsync(request);

        }
        public Task<GetConnectionOutputDTO> GetCodeConnectionAsync(DeleteCodeConnectionInputDTO request)
        {
            return _healthDataRepository.GetCodeConnectionAsync(request);
        }

        public Task<DeleteConnectionOutputDTO> DeleteConnectionAsync(DeleteConnectionInputDTO request)
        {
            return _healthDataRepository.DeleteConnectionAsync(request);
        }

        public Task<List<ConnectedUserModel>> GetConnectedUsersAsync(ConnectedUsersInputDTO request)
        {
            return _healthDataRepository.GetConnectedUsersAsync(request);
        }

        public Task<RegisterUserOutputDTO> RegisterUserAsync(RegisterUserInputDTO request)
        {
            return _healthDataRepository.RegisterUserAsync(request);
        }

        public Task<RegisterUserOutputDTO> RegisterGoogleUserAsync(RegisterGoogleUserInputDTO request)
        {
            return _healthDataRepository.RegisterGoogleUserAsync(request);

        }
        public Task<LoginUserOutputDTO> LoginUserAsync(LoginUserInputDTO request)
        {
            return _healthDataRepository.LoginUserAsync(request);
        }

        public Task<GetMonitoringUsersOutputDTO> GetMonitoringUsersAsync(GetMonitoringUsersInputDTO request)
        {
            return _healthDataRepository.GetMonitoringUsersAsync(request);
        }

        public Task ChangePasswordAsync(ChangePasswordRequestInputDTO input)
        {
            return _healthDataRepository.ChangePasswordAsync(input);
        }

        public Task<PdfOutputDTO> GeneratePdfAsync(PdfInputDTO request)
        {
            return _healthDataRepository.GeneratePdfAsync(request);
        }
        public Task<PdfOutputDTO> GeneratePdfByDataTypeAsync(PdfInputByDataTypeDTO request)
        {
            return _healthDataRepository.GeneratePdfByDataTypeAsync(request);
        }
        public Task<GetFavoritesOutputDTO> GetFavoritesAsync(UserCredentials request)
        {
            return _healthDataRepository.GetFavoritesAsync(request);
        }
        public Task<BaseOutputDTO> SetFavoritesAsync(SetFavoritesInputDTO request)
        {
            return _healthDataRepository.SetFavoritesAsync(request);
        }
    }
}


