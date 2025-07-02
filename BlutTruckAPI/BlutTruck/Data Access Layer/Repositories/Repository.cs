using BlutTruck.Data_Access_Layer.IRepositories;
using System.Net.Http;
using Newtonsoft.Json;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using BlutTruck.Domain_Layer.Entities;
using BlutTruck.Application_Layer.Models;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Firebase.Auth;
using Firebase.Database;
using FirebaseAdmin.Auth;
using Microsoft.Extensions.Options;
using Firebase.Auth.Providers;
using Firebase.Database.Query;
using Api.Controllers;
using static BlutTruck.Application_Layer.Models.PersonalDataModel;
using Microsoft.AspNetCore.Mvc;
using iTextSharp.text;
using iTextSharp.text.pdf;
using BlutTruck.Application_Layer.Models.InputDTO;
using BlutTruck.Application_Layer.Models.OutputDTO;
using Newtonsoft.Json.Linq;
using System.Net;
using static Google.Rpc.Context.AttributeContext.Types;
using BlutTruck.Application_Layer.Enums;
using System.Runtime.InteropServices.WindowsRuntime;
using Org.BouncyCastle.Asn1.Ocsp;


namespace BlutTruck.Data_Access_Layer.Repositories
{


    public class HealthDataRepository : IHealthDataRepository
    {
        private const string API_KEY = "AIzaSyB03cPKHoZJ05WaIT_D-Vsmsy7bkzH4zIc"; // Tu API_KEY
        private const string AUTH_DOMAIN = "proyectocsharp-tfg.firebaseapp.com";  // Tu dominio de Firebase
        private readonly IOptions<ApiSettings> _apiSettings;
        private readonly string _databaseUrl = "https://proyectocsharp-tfg-default-rtdb.europe-west1.firebasedatabase.app/";
        private readonly FirebaseAuthClient _authClient;
        

        public HealthDataRepository(IOptions<ApiSettings> apiSettings)
        {
            _apiSettings = apiSettings ?? throw new ArgumentNullException(nameof(apiSettings));
            var config = new FirebaseAuthConfig
            {
                ApiKey = API_KEY,
                AuthDomain = AUTH_DOMAIN,
                Providers = new FirebaseAuthProvider[] { new EmailProvider() }
            };
            _authClient = new FirebaseAuthClient(config);
        }

       

        public async Task<string> GetTokenAsync()
        {
            var adminUid = "server-admin-uid";
            var additionalClaims = new Dictionary<string, object>() { { "is_admin", true } };
            string customToken = await FirebaseAuth.DefaultInstance.CreateCustomTokenAsync(adminUid, additionalClaims);
            string adminIdToken = await ExchangeCustomTokenForIdTokenAsync(customToken); // Usa el método auxiliar que ya creamos
            return adminIdToken;
        }

        // No olvides el método auxiliar para canjear el token
        private async Task<string> ExchangeCustomTokenForIdTokenAsync(string customToken)
        {
            using (var httpClient = new HttpClient())
            {
                var requestUri = $"https://identitytoolkit.googleapis.com/v1/accounts:signInWithCustomToken?key={API_KEY}";
                var requestPayload = new { token = customToken, returnSecureToken = true };
                var jsonPayload = JsonConvert.SerializeObject(requestPayload);
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");
                var response = await httpClient.PostAsync(requestUri, content);
                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var responseData = JsonConvert.DeserializeObject<dynamic>(responseBody);
                    return responseData.idToken;
                }
                return null;
            }
        }

        public async Task<string> VerifyIdTokenAsync(string idToken)
        {
            var auth = FirebaseAuth.DefaultInstance;
            var decodedToken = await auth.VerifyIdTokenAsync(idToken);
            return decodedToken.Uid;
        }

        public async Task WriteDataWebAsync(WriteDataInputDTO request)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
            bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;

            FirebaseClient client;

            // Si es admin, creamos un cliente autenticado con su propio token de admin.
            if (isTokenFromAdmin)
            {
                Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
            }
            // Si es un usuario normal, verificamos seguridad y usamos su token.
            else
            {
                if (decodedToken.Uid != request.Credentials.UserId)
                {
                    throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                }

                // Comprobación de conexión que ya tenías
                var status = await GetConnectionStatusAsync(new GetConnectionStatusInputDTO { Credentials = request.Credentials });
                if (status.ConnectionStatus != 1)
                {
                    Console.WriteLine("La conexión del usuario no está activa. No se escriben los datos.");
                    return; // No se escriben los datos si el estado no es 1
                }

                client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
            }

            // La lógica de escritura es la misma para ambos, ya que el 'client' está correctamente autenticado.
            var currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
            await client
                .Child("healthData")
                .Child(request.Credentials.UserId)
                .Child("dias")
                .Child(currentDate)
                .PutAsync(request.HealthData);

            Console.WriteLine($"Datos guardados correctamente para {request.Credentials.UserId}.");
        }

        public async Task WriteDataAsync(WriteDataInputDTO request)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
            bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;

            FirebaseClient client;

            // Si es admin, creamos un cliente autenticado con su propio token de admin.
            if (isTokenFromAdmin)
            {
                Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
            }
            // Si es un usuario normal, verificamos seguridad y usamos su token.
            else
            {
                if (decodedToken.Uid != request.Credentials.UserId)
                {
                    throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                }

                // Comprobación de conexión que ya tenías
                var status = await GetConnectionStatusAsync(new GetConnectionStatusInputDTO { Credentials = request.Credentials });
                if (status.ConnectionStatus != 1)
                {
                    Console.WriteLine("La conexión del usuario no está activa. No se escriben los datos.");
                    return; // No se escriben los datos si el estado no es 1
                }

                client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
            }

            var currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
         
                await client
              .Child("healthData")
              .Child(request.Credentials.UserId)
              .Child("dias")
              .Child(currentDate)
              .PutAsync(request.HealthData);
            Console.WriteLine($"Datos guardados correctamente para {request.Credentials.UserId}.");

    }

        public async Task writePredictionAsync(PredictionInputDTO request)
        {
            if (request == null || request.Credentials == null || string.IsNullOrEmpty(request.Credentials.UserId) || string.IsNullOrEmpty(request.Credentials.IdToken))
            {
                Console.WriteLine("Error: Datos de entrada inválidos para writePredictionAsync.");
                return;
            }

            var currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");

            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;

                FirebaseClient client;

                // Si es admin, creamos un cliente autenticado con su propio token de admin.
                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    // Comprobación de conexión que ya tenías
                    var status = await GetConnectionStatusAsync(new GetConnectionStatusInputDTO { Credentials = request.Credentials });
                    if (status.ConnectionStatus != 1)
                    {
                        Console.WriteLine("La conexión del usuario no está activa. No se escriben los datos.");
                        return; // No se escriben los datos si el estado no es 1
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }

                var predictionDataToSave = new
                {
                    Resultado = request.Prediction,
                    Alertas = request.SpecificAlerts
                };

                await client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("Prediccion")
                    .PutAsync(predictionDataToSave);

                Console.WriteLine($"Predicción y alertas guardadas para el usuario: {request.Credentials.UserId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar la predicción para el usuario {request.Credentials.UserId}: {ex.Message}");
            }
        }

        public async Task writeAdminAsync(AdminInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId) || string.IsNullOrEmpty(request.Credentials.IdToken))
            {
                Console.WriteLine("Error: Datos de entrada inválidos para writePredictionAsync.");
                return;
            }
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }

                var pathConnection = $"healthData/{request.UserExtractId}/conexiones/{request.Credentials.UserId}/isAdmin";
                await client.Child(pathConnection).PutAsync<string>(request.AdminId);

                // Guardar el valor en el nodo isAdmin del usuario conectado
                var pathMonitor = $"healthData/{request.Credentials.UserId}/monitores/{request.UserExtractId}/isAdmin";
                await client.Child(pathMonitor).PutAsync<string>(request.AdminId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar variable isAdmin: {ex.Message}");
            }
        }


        public async Task<IEnumerable<PredictionDataDTO>> GetListPredictionAsync(UserCredentials credentials)
        {
            var predictionList = new List<PredictionDataDTO>();

            try
            {
                var response = new GetMonitoringUsersOutputDTO();
                response.MonitoringUsers = new List<MonitorUserModel>();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(credentials.IdToken) });
                }

                var monitoringResponse = await client
                    .Child("healthData")
                    .Child(credentials.UserId)
                    .Child("conexiones")
                    .OnceAsync<object>();

                if (monitoringResponse == null || !monitoringResponse.Any())
                {
                    response.Success = true;
                }

                foreach (var monitorFirebaseObject in monitoringResponse)
                {
                    var monitoringUserId = monitorFirebaseObject.Key;

                    if (string.IsNullOrEmpty(monitoringUserId)) continue;

                    string isAdmin = "False";
                    try
                    {
                        var storedData = monitorFirebaseObject.Object as Newtonsoft.Json.Linq.JObject;
                        MonitorUserModel m = storedData.ToObject<MonitorUserModel>();
                        if (storedData != null)
                        {
                            isAdmin = m.isAdmin;
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deserializing monitor data for {monitoringUserId}: {ex.Message}");
                    }

                    PersonalDataModel personalData = null;
                    try
                    {
                        personalData = await client
                            .Child("healthData")
                            .Child(monitoringUserId)
                            .Child("datos_personales")
                            .OnceSingleAsync<PersonalDataModel>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching personal data for {monitoringUserId}: {ex.Message}");
                    }


                    var monitoringUser = new MonitorUserModel
                    {
                        MonitoringUserId = monitoringUserId,
                        Name = personalData?.Name ?? "No Name",
                    };

                    response.MonitoringUsers.Add(monitoringUser);
                }

                response.Success = true;
                if (!response.Success || response.MonitoringUsers == null || !response.MonitoringUsers.Any())
                {
                    Console.WriteLine($"No monitoring users found or error occurred for user {credentials.UserId}. Returning empty prediction list.");
                    return predictionList;
                }
             
                foreach (var monitorUser in response.MonitoringUsers)
                {
                    try
                    {
                        var monitoredUserId = monitorUser.MonitoringUserId; 

                        var predictionPath = client
                            .Child("healthData")
                            .Child(monitoredUserId) 
                            .Child("Prediccion");

                        var namePath = client
                .Child("healthData")
                .Child(monitorUser.MonitoringUserId)
                .Child("datos_personales").Child("Name");

                        PredictionDataDTO predictionData = await predictionPath
                                                              .OnceSingleAsync<PredictionDataDTO>();
                        string nameData = await namePath
                                        .OnceSingleAsync<string>();
                        if (predictionData != null) {
                            predictionData.Nombre = nameData;

                        }

                        if (predictionData != null)
                        {
                            Console.WriteLine($"Prediction data successfully retrieved for monitored user: {monitoredUserId}");
                            predictionList.Add(predictionData);
                        }
                        else
                        {
                            Console.WriteLine($"No prediction data found for monitored user: {monitoredUserId}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Generic error fetching prediction for monitored user {monitorUser.MonitoringUserId}: {ex.Message}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetListPredictionAsync for user {credentials.UserId}: {ex.ToString()}");
            }

            Console.WriteLine($"Returning {predictionList.Count} predictions for users monitored by {credentials.UserId}.");
            return predictionList;
        }


        public async Task<PredictionDataDTO> GetPredictionAsync(UserCredentials credentials)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(credentials.IdToken);
            bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
            FirebaseClient client;

            if (isTokenFromAdmin)
            {
                Console.WriteLine($"Acción de administrador para usuario: {credentials.UserId}");
                client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(credentials.IdToken) });
            }
            // Si es un usuario normal, verificamos seguridad y usamos su token.
            else
            {
                if (decodedToken.Uid != credentials.UserId)
                {
                    throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                }

                client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(credentials.IdToken) });
            }

            // Construct the path to the user's prediction data
            var predictionPath = client
                .Child("healthData")
                .Child(credentials.UserId)
                .Child("Prediccion");

            

            // Attempt to retrieve the data and deserialize it into our DTO
            PredictionDataDTO predictionData = await predictionPath
                                         .OnceSingleAsync<PredictionDataDTO>();
            
            if (predictionData == null)
            {
                Console.WriteLine($"No prediction data found for user: {credentials.UserId}");
            }

            Console.WriteLine($"Prediction data successfully retrieved for user: {credentials.UserId}");
            return predictionData;
        }

        public async Task<ReadDataOutputDTO> ReadDataAsync(ReadDataInputDTO request)
        {
            var response = new ReadDataOutputDTO();
            string dateKey = DateTime.UtcNow.ToString("yyyy-MM-dd");

            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }


                // Intentar obtener los datos del día actual
                var todayData = await client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("dias")
                    .Child(dateKey)
                    .OnceSingleAsync<HealthDataOutputModel>();

                if (todayData != null)
                {
                    // Si hay datos para la fecha actual, devolverlos
                    response.Success = true;
                    response.SelectedDate = dateKey;
                    response.Data = todayData;
                    return response;
                }

                // Obtener todos los días disponibles si no hay datos del día actual
                var allDaysData = await client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("dias")
                    .OnceAsync<HealthDataOutputModel>();

                if (allDaysData == null || allDaysData.Count == 0)
                {
                    response.Success = false;
                    response.ErrorMessage = "Error: No hay datos disponibles en la base de datos.";
                    return response;
                }

                // Buscar la fecha más reciente
                var latestDay = allDaysData
                    .OrderByDescending(entry => entry.Key) // Ordenar las claves por fecha descendente
                    .FirstOrDefault();

                if (latestDay == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "Error: No se encontró una fecha válida en los datos.";
                    return response;
                }

                response.Success = true;
                response.SelectedDate = latestDay.Key;
                response.Data = latestDay.Object;
                return response;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error en Firebase: {ex.Message}";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error inesperado: {ex.Message}";
                return response;
            }
        }

        public async Task<HealthDataOutputModel> GetSelectDateHealthDataAsync(SelectDateHealthDataInputDTO request)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }


                // Construir la ruta y obtener los datos
                var healthData = await client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("dias")
                    .Child(request.DateKey)
                    .OnceSingleAsync<HealthDataOutputModel>();

                if (healthData == null)
                {
                    throw new Exception($"Los datos para la fecha {request.DateKey} no existen o están mal formateados.");
                }

                return healthData;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                throw new Exception($"Error en Firebase: {ex.Message}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error inesperado: {ex.Message}");
            }
        }

        public async Task<FullDataOutputDTO> GetFullHealthDataAsync(UserCredentials credentials)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(credentials.IdToken) });
                }


                // Obtener datos personales
                var personalData = await client
                    .Child("healthData")
                    .Child(credentials.UserId)
                    .Child("datos_personales")
                    .OnceSingleAsync<PersonalDataModel>();

                if (personalData == null)
                {
                    return null;
                }

                // Obtener datos de días
                var daysData = await client
                    .Child("healthData")
                    .Child(credentials.UserId)
                    .Child("dias")
                    .OnceAsync<HealthDataOutputModel>();

                if (daysData == null || daysData.Count == 0)
                {
                    return null;
                }

                // Construir el objeto final utilizando el DTO
                var result = new FullDataOutputDTO
                {
                    DatosPersonales = personalData,
                    Dias = daysData.ToDictionary(entry => entry.Key, entry => entry.Object)
                };

                return result;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                // Aquí puedes registrar el error y/o lanzar la excepción según convenga
                return null;
            }
            catch (Exception ex)
            {
                // Manejo de errores inesperados
                return null;
            }
        }

        public async Task<FullDataOutputDTO> GetFullHealthDataAndConnectAsync(FullAndMonitoringInputDTO request)
        {
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }


                // Obtener datos personales
                var personalData = await client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("datos_personales")
                    .OnceSingleAsync<PersonalDataModel>();

                if (personalData == null)
                {
                    return null;
                }

                // Obtener datos de días
                var daysData = await client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("dias")
                    .OnceAsync<HealthDataOutputModel>();

                if (daysData == null || daysData.Count == 0)
                {
                    return null;
                }

                // Construir el objeto final utilizando el DTO


                string isAdmin = await client
                 .Child("healthData")
                 .Child(request.Credentials.UserId)
                 .Child("monitores")
                 .Child(request.UserMonitoringID)
                 .Child("isAdmin")
                 .OnceSingleAsync<string>();

                var result = new FullDataOutputDTO
                {
                    DatosPersonales = personalData,
                    Dias = daysData.ToDictionary(entry => entry.Key, entry => entry.Object),
                    CurrentUserIsAdmin = isAdmin
                };

                return result;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        public async Task<PdfOutputDTO> GeneratePdfByDataTypeAsync(PdfInputByDataTypeDTO request)
        {
            FullDataOutputDTO fullData = await this.GetFullHealthDataAsync(request.Credentials);

            if (fullData == null || fullData.DatosPersonales == null)
            {
                throw new Exception("No se encontraron datos para el usuario especificado.");
            }

            using (var ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);
                var subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                var smallBoldFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 10);
                var listItemFont = FontFactory.GetFont(FontFactory.HELVETICA, 9);

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                string rutaImagen = Path.Combine(baseDir, "Recursos", "logo.png");
                if (System.IO.File.Exists(rutaImagen))
                {
                    Image logo = Image.GetInstance(rutaImagen);
                    logo.ScaleAbsolute(50, 50);
                    PdfPTable headerTable = new PdfPTable(2) { WidthPercentage = 50, HorizontalAlignment = Element.ALIGN_LEFT };
                    headerTable.SetWidths(new float[] { 1, 3 });
                    headerTable.AddCell(new PdfPCell(logo) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_CENTER, VerticalAlignment = Element.ALIGN_MIDDLE, Padding = 0 });
                    headerTable.AddCell(new PdfPCell(new Paragraph("BlutTruck", headerFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_LEFT, VerticalAlignment = Element.ALIGN_MIDDLE, PaddingLeft = 5, Padding = 0 });
                    document.Add(headerTable);
                }
                else
                {
                    Paragraph appTitle = new Paragraph("BlutTruck", headerFont) { Alignment = Element.ALIGN_CENTER };
                    document.Add(appTitle);
                    document.Add(new Paragraph("Advertencia: Logo no encontrado en " + rutaImagen, FontFactory.GetFont(FontFactory.HELVETICA, 8, BaseColor.RED)));
                }
                document.Add(new Paragraph(" "));

                document.Add(new Paragraph("Datos Personales", headerFont));
                document.Add(new Paragraph($"Nombre: {fullData.DatosPersonales.Name}", normalFont));
                // Formatear fecha de nacimiento si es DateTime
                // document.Add(new Paragraph($"Fecha de nacimiento: {fullData.DatosPersonales.DateOfBirth:dd/MM/yyyy}", normalFont));
                document.Add(new Paragraph($"Fecha de nacimiento: {fullData.DatosPersonales.DateOfBirth}", normalFont)); // Mantener como string por ahora
                document.Add(new Paragraph($"Altura: {fullData.DatosPersonales.Height}", normalFont));
                document.Add(new Paragraph($"Peso: {fullData.DatosPersonales.Weight}", normalFont));
                document.Add(new Paragraph($"Género: {fullData.DatosPersonales.Gender}", normalFont));
                document.Add(new Paragraph(" "));

                string dataTypeFriendlyName = GetDataTypeFriendlyName(request.DataType);
                document.Add(new Paragraph($"Historial de {dataTypeFriendlyName}", headerFont));
                document.Add(new Paragraph(" "));

                if (fullData.Dias == null || !fullData.Dias.Any())
                {
                    document.Add(new Paragraph($"No hay datos diarios disponibles para {dataTypeFriendlyName}.", normalFont));
                }
                else
                {
                    bool anyDataFoundForTypeAcrossAllDays = false;
                    foreach (var diaEntry in fullData.Dias.OrderBy(d => d.Key)) // Ordenar por fecha (string YYYY-MM-DD)
                    {
                        string fechaString = diaEntry.Key;
                        // Intentar parsear la fecha para mostrarla en formato local, si es necesario.
                        // DateTime fechaDT = DateTime.ParseExact(fechaString, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                        // Paragraph dateParagraph = new Paragraph($"Fecha: {fechaDT:dd/MM/yyyy}", subHeaderFont);
                        Paragraph dateParagraph = new Paragraph($"Fecha: {fechaString}", subHeaderFont); // O mantener YYYY-MM-DD

                        HealthDataOutputModel diaData = diaEntry.Value;
                        bool dataFoundForDay = false;
                        List<IElement> elementsForDay = new List<IElement>();

                        switch (request.DataType)
                        {
                            case HealthDataType.Steps:
                                if (diaData.Steps.HasValue)
                                {
                                    elementsForDay.Add(new Paragraph($"Pasos: {diaData.Steps.Value}", normalFont));
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.HeartRate:
                                if (diaData.HeartRateData != null && diaData.HeartRateData.Any())
                                {
                                    elementsForDay.Add(new Paragraph("Ritmo Cardíaco:", smallBoldFont));
                                    foreach (var hr in diaData.HeartRateData)
                                    {
                                        elementsForDay.Add(new Paragraph($"  Hora: {hr.Time:HH:mm:ss}, BPM: {hr.BPM}", listItemFont));
                                    }
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.Temperature:
                                if (diaData.TemperatureData != null && diaData.TemperatureData.Any())
                                {
                                    elementsForDay.Add(new Paragraph("Temperatura (mediciones):", smallBoldFont));
                                    foreach (var temp in diaData.TemperatureData)
                                    {
                                        elementsForDay.Add(new Paragraph($"  Hora: {temp.Time:HH:mm:ss}, Temp: {temp.Temperature:F1}°C", listItemFont));
                                    }
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.ActiveCalories:
                                if (diaData.ActiveCalories.HasValue)
                                {
                                    elementsForDay.Add(new Paragraph($"Calorías Activas: {diaData.ActiveCalories.Value:F0} kcal", normalFont));
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.BloodGlucose:
                                if (diaData.BloodGlucoseData != null && diaData.BloodGlucoseData.Any())
                                {
                                    elementsForDay.Add(new Paragraph("Glucosa en Sangre:", smallBoldFont));
                                    foreach (var bg in diaData.BloodGlucoseData)
                                    {
                                        elementsForDay.Add(new Paragraph($"  Hora: {bg.Time:HH:mm:ss}, Glucosa: {bg.BloodGlucose:F1} mg/dL", listItemFont));
                                    }
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.BloodPressure:
                                if (diaData.BloodPressureData != null && diaData.BloodPressureData.Any())
                                {
                                    elementsForDay.Add(new Paragraph("Presión Arterial:", smallBoldFont));
                                    foreach (var bp in diaData.BloodPressureData)
                                    {
                                        elementsForDay.Add(new Paragraph($"  Hora: {bp.Time:HH:mm:ss}, Sistólica: {bp.Systolic:F0}, Diastólica: {bp.Diastolic:F0} mmHg", listItemFont));
                                    }
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.OxygenSaturation:
                                if (diaData.OxygenSaturationData != null && diaData.OxygenSaturationData.Any())
                                {
                                    elementsForDay.Add(new Paragraph("Saturación de Oxígeno (SpO2):", smallBoldFont));
                                    foreach (var oxy in diaData.OxygenSaturationData)
                                    {
                                        elementsForDay.Add(new Paragraph($"  Hora: {oxy.Time:HH:mm:ss}, Porcentaje: {oxy.Percentage:F0}%", listItemFont));
                                    }
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.RespiratoryRate:
                                if (diaData.RespiratoryRateData != null && diaData.RespiratoryRateData.Any())
                                {
                                    elementsForDay.Add(new Paragraph("Frecuencia Respiratoria:", smallBoldFont));
                                    foreach (var rr in diaData.RespiratoryRateData)
                                    {
                                        elementsForDay.Add(new Paragraph($"  Hora: {rr.Time:HH:mm:ss}, Tasa: {rr.Rate:F0} rpm", listItemFont));
                                    }
                                    dataFoundForDay = true;
                                }
                                break;
                         
                            case HealthDataType.AverageHeartRate:
                                if (diaData.AvgHeartRate.HasValue)
                                {
                                    elementsForDay.Add(new Paragraph($"Ritmo Cardíaco Promedio: {diaData.AvgHeartRate.Value:F0} BPM", normalFont));
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.BodyTemperature:
                                if (diaData.BodyTemperature.HasValue)
                                {
                                    elementsForDay.Add(new Paragraph($"Temperatura Corporal (General del día): {diaData.BodyTemperature.Value:F1}°C", normalFont));
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.MaxHeartRate:
                                if (diaData.MaxHeartRate.HasValue)
                                {
                                    elementsForDay.Add(new Paragraph($"Ritmo Cardíaco Máximo: {diaData.MaxHeartRate.Value:F0} BPM", normalFont));
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.MinHeartRate:
                                if (diaData.MinHeartRate.HasValue)
                                {
                                    elementsForDay.Add(new Paragraph($"Ritmo Cardíaco Mínimo: {diaData.MinHeartRate.Value:F0} BPM", normalFont));
                                    dataFoundForDay = true;
                                }
                                break;
                            case HealthDataType.RestingHeartRate:
                                if (diaData.RestingHeartRate.HasValue)
                                {
                                    elementsForDay.Add(new Paragraph($"Ritmo Cardíaco en Reposo: {diaData.RestingHeartRate.Value:F0} BPM", normalFont));
                                    dataFoundForDay = true;
                                }
                                break;
                            default:
                                elementsForDay.Add(new Paragraph("Tipo de dato no soportado para la visualización detallada.", normalFont));
                                dataFoundForDay = true;
                                break;
                        }

                        if (dataFoundForDay)
                        {
                            anyDataFoundForTypeAcrossAllDays = true;
                            document.Add(dateParagraph);
                            foreach (var element in elementsForDay)
                            {
                                document.Add(element);
                            }
                            document.Add(new Paragraph(" "));
                        }
                    }

                    if (!anyDataFoundForTypeAcrossAllDays)
                    {
                        document.Add(new Paragraph($"No se encontraron datos de '{dataTypeFriendlyName}' para ninguna fecha.", normalFont));
                    }
                }

                document.Close();
                writer.Close();

                return new PdfOutputDTO { PdfBytes = ms.ToArray() };
            }
        }
        private bool HasDataForType(HealthDataOutputModel diaData, HealthDataType dataType)
        {
            switch (dataType)
            {
                case HealthDataType.Steps: return diaData.Steps.HasValue;
                case HealthDataType.HeartRate: return diaData.HeartRateData != null && diaData.HeartRateData.Any();
                case HealthDataType.Temperature: return diaData.TemperatureData != null && diaData.TemperatureData.Any();
                case HealthDataType.ActiveCalories: return diaData.ActiveCalories.HasValue;
                case HealthDataType.BloodGlucose: return diaData.BloodGlucoseData != null && diaData.BloodGlucoseData.Any();
                case HealthDataType.BloodPressure: return diaData.BloodPressureData != null && diaData.BloodPressureData.Any();
                case HealthDataType.OxygenSaturation: return diaData.OxygenSaturationData != null && diaData.OxygenSaturationData.Any();
                case HealthDataType.RespiratoryRate: return diaData.RespiratoryRateData != null && diaData.RespiratoryRateData.Any();
                case HealthDataType.Sleep: return diaData.SleepData != null;
                case HealthDataType.AverageHeartRate: return diaData.AvgHeartRate.HasValue;
                case HealthDataType.BodyTemperature: return diaData.BodyTemperature.HasValue;
                case HealthDataType.MaxHeartRate: return diaData.MaxHeartRate.HasValue;
                case HealthDataType.MinHeartRate: return diaData.MinHeartRate.HasValue;
                case HealthDataType.RestingHeartRate: return diaData.RestingHeartRate.HasValue;
                default: return false;
            }
        }


        // Método auxiliar para obtener un nombre legible del tipo de dato
        private string GetDataTypeFriendlyName(HealthDataType dataType)
        {
            switch (dataType)
            {
                case HealthDataType.Steps: return "Pasos";
                case HealthDataType.HeartRate: return "Ritmo Cardíaco";
                case HealthDataType.Temperature: return "Temperatura";
                case HealthDataType.ActiveCalories: return "Calorías Activas";
                case HealthDataType.BloodGlucose: return "Glucosa en Sangre";
                case HealthDataType.BloodPressure: return "Presión Arterial";
                case HealthDataType.OxygenSaturation: return "Saturación de Oxígeno";
                case HealthDataType.RespiratoryRate: return "Frecuencia Respiratoria";
                case HealthDataType.Sleep: return "Sueño";
                case HealthDataType.AverageHeartRate: return "Ritmo Cardíaco Promedio";
                case HealthDataType.BodyTemperature: return "Temperatura Corporal General";
                case HealthDataType.MaxHeartRate: return "Ritmo Cardíaco Máximo";
                case HealthDataType.MinHeartRate: return "Ritmo Cardíaco Mínimo";
                case HealthDataType.RestingHeartRate: return "Ritmo Cardíaco en Reposo";
                default: return "Dato Desconocido";
            }
        }

    public async Task<PdfOutputDTO> GeneratePdfAsync(PdfInputDTO request)
        {
            // Se obtiene el objeto con los datos completos.
            FullDataOutputDTO fullData = await this.GetFullHealthDataAsync(request.Credentials);

            if (fullData == null)
            {
                throw new Exception("No se encontraron datos para el usuario especificado.");
            }

            using (var ms = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 50, 50, 25, 25);
                PdfWriter writer = PdfWriter.GetInstance(document, ms);
                document.Open();

                // ------------------------
                // Sección: Encabezado
                // ------------------------

                // Define la fuente para el encabezado
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 16);

                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // Crea la ruta a la imagen DENTRO de la carpeta del programa
                string rutaImagen = Path.Combine(baseDir, "Recursos", "logo.png");
                Image logo = Image.GetInstance(rutaImagen);
                logo.ScaleAbsolute(50, 50);
                // Se omite asignar la alineación en la imagen, ya que se define en la celda

                // Crea el título de la app
                Paragraph appTitle = new Paragraph("BlutTruck", headerFont);

                // Crear una tabla de 2 columnas con ancho reducido y centrado en la página
                PdfPTable headerTable = new PdfPTable(2);
                headerTable.WidthPercentage = 50; // Reduce el ancho de la tabla para que se centre mejor
                headerTable.HorizontalAlignment = Element.ALIGN_LEFT;
                headerTable.SetWidths(new float[] { 1, 3 }); // Ajusta el ancho de cada columna según lo necesites

                // Celda para la imagen (centrada)
                PdfPCell cellLogo = new PdfPCell(logo)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    Padding = 0
                };
                headerTable.AddCell(cellLogo);

                // Celda para el título (con margen a la izquierda para separarlo un poco)
                PdfPCell cellTitle = new PdfPCell(appTitle)
                {
                    Border = Rectangle.NO_BORDER,
                    HorizontalAlignment = Element.ALIGN_LEFT,
                    VerticalAlignment = Element.ALIGN_MIDDLE,
                    PaddingLeft = 5,
                    Padding = 0
                };
                headerTable.AddCell(cellTitle);

                // Agregar la tabla de encabezado al documento
                document.Add(headerTable);
                // Salto de línea para separar el encabezado del contenido
                document.Add(new Paragraph(" "));

                // ------------------------
                // Sección: Datos Personales
                // ------------------------
                document.Add(new Paragraph("Datos Personales", headerFont));
                document.Add(new Paragraph($"Nombre: {fullData.DatosPersonales.Name}"));
                document.Add(new Paragraph($"Fecha de nacimiento: {fullData.DatosPersonales.DateOfBirth}"));
                document.Add(new Paragraph($"Altura: {fullData.DatosPersonales.Height}"));
                document.Add(new Paragraph($"Peso: {fullData.DatosPersonales.Weight}"));
                document.Add(new Paragraph($"Género: {fullData.DatosPersonales.Gender}"));
                document.Add(new Paragraph(" "));

                // ------------------------
                // Sección: Datos de Días
                // ------------------------
                document.Add(new Paragraph("Datos de Días", headerFont));

                foreach (var dia in fullData.Dias)
                {
                    string fecha = dia.Key;
                    HealthDataOutputModel diaData = dia.Value;

                    var subHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 14);
                    document.Add(new Paragraph($"Fecha: {fecha}", subHeaderFont));
                    document.Add(new Paragraph($"Pasos: {diaData.Steps}"));

                    if (diaData.HeartRateData != null && diaData.HeartRateData.Any())
                    {
                        document.Add(new Paragraph("Datos de Ritmo Cardíaco:"));
                        foreach (var hr in diaData.HeartRateData)
                        {
                            document.Add(new Paragraph($"Hora: {hr.Time} - BPM: {hr.BPM}"));
                        }
                    }

                    if (diaData.TemperatureData != null && diaData.TemperatureData.Any())
                    {
                        document.Add(new Paragraph("Datos de Temperatura:"));
                        foreach (var temp in diaData.TemperatureData)
                        {
                            document.Add(new Paragraph($"Hora: {temp.Time} - Temperatura: {temp.Temperature}"));
                        }
                    }

                    document.Add(new Paragraph(" ")); // Espacio entre días
                }

                document.Close();
                writer.Close();

                return new PdfOutputDTO { PdfBytes = ms.ToArray() };
            }
        }


        public async Task<SaveUserProfileOutputDTO> SaveUserProfileAsync(SaveUserProfileInputDTO request)
        {
            var response = new SaveUserProfileOutputDTO();
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }


                var profileRef = client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("datos_personales");

                await profileRef.PutAsync(request.Profile);

                response.Success = true;
                return response;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error en Firebase: {ex.Message}";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error inesperado: {ex.Message}";
                return response;
            }
        }

        public async Task<UpdateConnectionStatusOutputDTO> UpdateConnectionStatusAsync(UpdateConnectionStatusInputDTO request)
        {
            var response = new UpdateConnectionStatusOutputDTO();
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }


                var connectionRef = client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("datos_personales")
                    .Child("Conexion")
                    .Child("ConnectionStatus");

                await connectionRef.PutAsync(request.ConnectionStatus.ConnectionStatus);
                response.Success = true;
                return response;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error en Firebase: {ex.Message}";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error inesperado: {ex.Message}";
                return response;
            }
        }

        public async Task<PersonalAndLatestDayDataOutputDTO> GetPersonalAndLatestDayDataAsync(GetPersonalAndLatestDayDataInputDTO request)
        {
            var response = new PersonalAndLatestDayDataOutputDTO();
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }


                // Obtener datos personales
                var personalData = await client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("datos_personales")
                    .OnceSingleAsync<PersonalDataModel>();

                if (personalData == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "Error: Los datos personales son nulos o no existen.";
                    return response;
                }

                // Obtener datos de días
                var daysData = await client
                .Child("healthData")
                .Child(request.Credentials.UserId)
                .Child("dias")
                .OnceAsync<HealthDataOutputModel>();


                if (daysData == null || daysData.Count == 0)
                {
                    response.Success = false;
                    response.ErrorMessage = "Error: No se encontraron datos de días.";
                    return response;
                }

                // Buscar el día más reciente
                var latestDayKey = daysData
                    .OrderByDescending(entry => entry.Key)
                    .FirstOrDefault()?.Key;

                if (string.IsNullOrEmpty(latestDayKey))
                {
                    response.Success = false;
                    response.ErrorMessage = "Error: No se encontró un día válido más reciente.";
                    return response;
                }

                var latestDayData = daysData.FirstOrDefault(entry => entry.Key == latestDayKey)?.Object;
                if (latestDayData == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "Error: No se encontraron datos para el día más reciente.";
                    return response;
                }

                response.PersonalData = personalData;
                response.DiaMasReciente = new LatestDayData
                {
                    Fecha = latestDayKey,
                    Datos = latestDayData
                };
                response.Success = true;
                return response;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error en Firebase: {ex.Message}";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error inesperado: {ex.Message}";
                return response;
            }
        }

        public async Task<GetPersonalDataOutputDTO> GetPersonalDataAsync(GetPersonalDataInputDTO request)
        {
            var response = new GetPersonalDataOutputDTO();
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }


                var personalData = await client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("datos_personales")
                    .OnceSingleAsync<PersonalDataModel>();

                if (personalData == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "No se encontraron datos personales para este usuario.";
                    return response;
                }
                response.PersonalData = personalData;
                response.Success = true;
                return response;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error en Firebase: {ex.Message}";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error inesperado: {ex.Message}";
                return response;
            }
        }

        public async Task<GetConnectionStatusOutputDTO> GetConnectionStatusAsync(GetConnectionStatusInputDTO request)
        {
            var response = new GetConnectionStatusOutputDTO();
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }


                var connectionRef = client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("datos_personales")
                    .Child("Conexion")
                    .Child("ConnectionStatus");

                var connectionStatus = await connectionRef.OnceSingleAsync<int?>();
                response.ConnectionStatus = connectionStatus;
                response.Success = true;
                return response;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error en Firebase: {ex.Message}";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error inesperado: {ex.Message}";
                return response;
            }
        }

        public async Task<GetFavoritesOutputDTO> GetFavoritesAsync(UserCredentials request)
        {
            var response = new GetFavoritesOutputDTO();
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.IdToken) });
                }

                var connectionRef = client
                    .Child("healthData")
                    .Child(request.UserId)
                    .Child("datos_personales")
                    .Child("Favoritos");

                var connectionStatus = await connectionRef.OnceSingleAsync<List<string>>();
                response.Favorites = connectionStatus ?? new List<string>();
                return response;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error en Firebase: {ex.Message}";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error inesperado: {ex.Message}";
                return response;
            }
        }

        /// <summary>
        /// Escribe o sobrescribe la lista de tarjetas favoritas de un usuario en Firebase.
        /// </summary>
        /// <param name="request">Contiene las credenciales y la nueva lista de favoritos.</param>
        /// <returns>Un DTO que indica si la operación fue exitosa.</returns>
        public async Task<BaseOutputDTO> SetFavoritesAsync(SetFavoritesInputDTO request)
        {
            var response = new BaseOutputDTO();
            try
            {
                // Validación básica de la entrada
                if (request.Favorites == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "La lista de favoritos no puede ser nula.";
                    return response;
                }

                // La configuración del cliente de Firebase es idéntica a tu método GET
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }


                // La referencia al nodo de Firebase también es la misma
                var favoritesRef = client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("datos_personales")
                    .Child("Favoritos");

                await favoritesRef.PutAsync(request.Favorites);

                // Si llegamos aquí, la operación fue exitosa.
                response.Success = true;
                return response;
            }
            catch (Firebase.Database.FirebaseException ex)
            {
                // Manejo de errores específico de Firebase
                response.Success = false;
                response.ErrorMessage = $"Error en Firebase: {ex.Message}";
                return response;
            }
            catch (Exception ex)
            {
                // Manejo de errores genéricos
                response.Success = false;
                response.ErrorMessage = $"Error inesperado: {ex.Message}";
                return response;
            }
        }

        public async Task<RegisterConnectionOutputDTO> RegisterConnectionAsync(RegisterConnectionInputDTO request)
        {
            var response = new RegisterConnectionOutputDTO();
            try
            {
                var firebaseClient = new FirebaseClient(
                    _databaseUrl,
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.IdToken) });

                // Separar el ID real y el flag de admin
                var parts = request.ConnectedUserId.Split(";admin:");
                var extractedUserId = parts[0];
                var isAdmin = parts.Length > 1 && bool.TryParse(parts[1], out bool adminValue) ? adminValue : false;

                var connectionData = new Dictionary<string, string>
        {
            { "connectedAt", DateTime.UtcNow.ToString("o") },
            { "connectedUserId", extractedUserId },
            { "isAdmin", isAdmin.ToString() }
        };

                var monitorData = new Dictionary<string, string>
        {
            { "monitoredAt", DateTime.UtcNow.ToString("o") },
            { "monitoringUserId", request.CurrentUserId },
            { "isAdmin", isAdmin.ToString() }
        };

                // Guardar la conexión en el usuario actual
                var pathConnection = $"healthData/{request.CurrentUserId}/conexiones/{extractedUserId}";
                await firebaseClient.Child(pathConnection).PutAsync(connectionData);

                // Guardar la referencia en el usuario conectado
                var pathMonitor = $"healthData/{extractedUserId}/monitores/{request.CurrentUserId}";
                await firebaseClient.Child(pathMonitor).PutAsync(monitorData);

                response.Success = true;
                response.Message = "Conexión registrada exitosamente y referencia inversa guardada";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error al registrar la conexión: {ex.Message}";
                return response;
            }
        }

        public async Task<RegisterConnectionOutputDTO> RegisterCodeConnectionAsync(RegisterCodeConnectionInputDTO request)
        {
            var response = new RegisterConnectionOutputDTO();
            try
            {
                var firebaseClient = new FirebaseClient(
                    _databaseUrl,
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.IdToken) });

                var monitorData = new Dictionary<string, string>
        {
            { "Code", request.Code },
            { "UserId", request.CurrentUserId }
        };

                // Guardar la conexión en el usuario actual
                var pathConnection = $"/Codes/{request.Code}";
                await firebaseClient.Child(pathConnection).PutAsync(monitorData);

                response.Success = true;
                response.Message = "Conexión registrada exitosamente y referencia inversa guardada";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error al registrar la conexión: {ex.Message}";
                return response;
            }
        }

        public async Task<DeleteConnectionOutputDTO> DeleteCodeConnectionAsync(DeleteCodeConnectionInputDTO request)
        {
            var response = new DeleteConnectionOutputDTO();
            try
            {
                var firebaseClient = new FirebaseClient(
                    _databaseUrl,
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.IdToken) });


                var pathConnection = $"/Codes/{request.Code}";
                await firebaseClient.Child(pathConnection).DeleteAsync();

                response.Success = true;
                response.Message = "Codigoborrado correctamente";
                return response;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error al registrar la conexión: {ex.Message}";
                return response;
            }
        }

        public async Task<GetConnectionOutputDTO> GetCodeConnectionAsync(DeleteCodeConnectionInputDTO request)
        {
            var response = new GetConnectionOutputDTO();
            try
            {
                var firebaseClient = new FirebaseClient(
                    _databaseUrl,
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.IdToken) });

                var id = await firebaseClient
                    .Child("Codes")
                    .Child(request.Code)
                    .OnceAsync<object>();
                response.Id = id.ToArray()[1].Object.ToString();
                return response;
            }
            catch (Exception ex)
            {
                response.ErrorMessage = $"Error al leer el codigo";
                return response;
            }
        }

        public async Task<DeleteConnectionOutputDTO> DeleteConnectionAsync(DeleteConnectionInputDTO request)
        {
            var response = new DeleteConnectionOutputDTO();
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }

                var path = $"healthData/{request.Credentials.UserId}/conexiones/{request.ConnectedUserId}";
                await client.Child(path).DeleteAsync();

                var pathmonitoring = $"healthData/{request.ConnectedUserId}/monitores/{request.Credentials.UserId}";
                await client.Child(pathmonitoring).DeleteAsync();

                response.Success = true;
                response.Message = "Conexión eliminada correctamente";
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error al eliminar la conexión: {ex.Message}";
            }
            return response;
        }

        public async Task<RegisterUserOutputDTO> RegisterUserAsync(RegisterUserInputDTO request)
        {
            var response = new RegisterUserOutputDTO();
            try
            {
                var userCredential = await _authClient.CreateUserWithEmailAndPasswordAsync(request.Email, request.Password);
                var user = userCredential.User;
                response.Token = await user.GetIdTokenAsync();
                response.Success = true;
                var userId = userCredential.User.Uid;


                string token = await GetTokenAsync();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token) });
                }
                #region profile
                var profile = new PersonalDataModel
                {
                    Conexion = new PersonalDataModel.ConnectionModel
                    {
                        ConnectionStatus = 0
                    },
                    DateOfBirth = null, 
                    HasPredisposition = false,
                    Height = 0,
                    Weight = 0,
                    Gender = 0,
                    Smoke = 0,
                    Alcohol = 0,
                    Choresterol = 0,
                    PhotoURL = null,
                    Name = request.Name,
                    Active = false
                };
                #endregion profile

                var profileRef = client
                   .Child("healthData")
                   .Child(userId)
                   .Child("datos_personales");
                await profileRef.PutAsync(profile);
                #region healthData
                var healthData = new HealthDataInputModel
                {
                    UserId = null,
                    Steps = 0,
                    ActiveCalories = 0.0,
                    // Puedes inicializar la lista con un valor 0 o dejarla vacía según tus necesidades
                    HeartRates = new List<int?> { 0 },
                    // Para las colecciones de data, se puede crear un único objeto con valores por defecto
                    HeartRateData = new List<HeartRateDataPoint>
    {
        new HeartRateDataPoint { Time = DateTime.MinValue, BPM = 0 }
    },
                    RestingHeartRate = 0.0,
                    Weight = 0.0,
                    Height = 0.0,
                    BloodPressureData = new List<BloodPressureDataPoint>
    {
        new BloodPressureDataPoint { Time = DateTime.MinValue, Systolic = 0.0, Diastolic = 0.0 }
    },
                    OxygenSaturationData = new List<OxygenSaturationDataPoint>
    {
        new OxygenSaturationDataPoint { Time = DateTime.MinValue, Percentage = 0.0 }
    },
                    BloodGlucoseData = new List<BloodGlucoseDataPoint>
    {
        new BloodGlucoseDataPoint { Time = DateTime.MinValue, BloodGlucose = 0.0 }
    },
                    BodyTemperature = 0.0,
                    TemperatureData = new List<TemperatureDataPoint>
    {
        new TemperatureDataPoint { Time = DateTime.MinValue, Temperature = 0.0 }
    },
                    RespiratoryRateData = new List<RespiratoryRateDataPoint>
    {
        new RespiratoryRateDataPoint { Time = DateTime.MinValue, Rate = 0.0 }
    },
                    SleepData = new List<SleepSessionDataPoint>
    {
        new SleepSessionDataPoint
        {
            StartTime = DateTime.MinValue,
            EndTime = DateTime.MinValue,
            Stages = new List<SleepStageDataPoint>
            {
                new SleepStageDataPoint
                {
                    Type = null, // o string.Empty, si prefieres que sea cadena vacía
                    StartTime = DateTime.MinValue,
                    EndTime = DateTime.MinValue
                }
            }
        }
    }
                };
                #endregion healthData
                var currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                await client
              .Child("healthData")
              .Child(userId)
              .Child("dias")
              .Child(currentDate)
              .PutAsync(healthData);

            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                response.Success = false;
                response.ErrorMessage = "Error al registrar el usuario: " + ex.Reason;
            }
            return response;
        }

        public async Task<RegisterUserOutputDTO> RegisterGoogleUserAsync(RegisterGoogleUserInputDTO request)
        {
            var response = new RegisterUserOutputDTO();
            try
            {
                response.Success = true;
                var userId = request.UserId;


                string token = await GetTokenAsync();
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(token);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(token) });
                }
                #region profile
                var profile = new PersonalDataModel
                {
                    Conexion = new PersonalDataModel.ConnectionModel
                    {
                        ConnectionStatus = 0
                    },
                    DateOfBirth = null, // Puedes asignar una fecha en formato string o dejarla en null
                    HasPredisposition = false,
                    Height = 0,
                    Weight = 0,
                    Gender = 0,
                    Smoke = 0,
                    Alcohol = 0,
                    Choresterol = 0,
                    PhotoURL = null,
                    Name = request.Name,
                    Active = false
                };
                #endregion profile

                var profileRef = client
                   .Child("healthData")
                   .Child(userId)
                   .Child("datos_personales");
                await profileRef.PutAsync(profile);
                #region healthData
                var healthData = new HealthDataInputModel
                {
                    UserId = null,
                    Steps = 0,
                    ActiveCalories = 0.0,
                    // Puedes inicializar la lista con un valor 0 o dejarla vacía según tus necesidades
                    HeartRates = new List<int?> { 0 },
                    // Para las colecciones de data, se puede crear un único objeto con valores por defecto
                    HeartRateData = new List<HeartRateDataPoint>
    {
        new HeartRateDataPoint { Time = DateTime.MinValue, BPM = 0 }
    },
                    RestingHeartRate = 0.0,
                    Weight = 0.0,
                    Height = 0.0,
                    BloodPressureData = new List<BloodPressureDataPoint>
    {
        new BloodPressureDataPoint { Time = DateTime.MinValue, Systolic = 0.0, Diastolic = 0.0 }
    },
                    OxygenSaturationData = new List<OxygenSaturationDataPoint>
    {
        new OxygenSaturationDataPoint { Time = DateTime.MinValue, Percentage = 0.0 }
    },
                    BloodGlucoseData = new List<BloodGlucoseDataPoint>
    {
        new BloodGlucoseDataPoint { Time = DateTime.MinValue, BloodGlucose = 0.0 }
    },
                    BodyTemperature = 0.0,
                    TemperatureData = new List<TemperatureDataPoint>
    {
        new TemperatureDataPoint { Time = DateTime.MinValue, Temperature = 0.0 }
    },
                    RespiratoryRateData = new List<RespiratoryRateDataPoint>
    {
        new RespiratoryRateDataPoint { Time = DateTime.MinValue, Rate = 0.0 }
    },
                    SleepData = new List<SleepSessionDataPoint>
    {
        new SleepSessionDataPoint
        {
            StartTime = DateTime.MinValue,
            EndTime = DateTime.MinValue,
            Stages = new List<SleepStageDataPoint>
            {
                new SleepStageDataPoint
                {
                    Type = null, // o string.Empty, si prefieres que sea cadena vacía
                    StartTime = DateTime.MinValue,
                    EndTime = DateTime.MinValue
                }
            }
        }
    }
                };
                #endregion healthData
                var currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                await client
              .Child("healthData")
              .Child(userId)
              .Child("dias")
              .Child(currentDate)
              .PutAsync(healthData);

            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                response.Success = false;
                response.ErrorMessage = "Error al registrar el usuario: " + ex.Reason;
            }
            return response;
        }

        public async Task<LoginUserOutputDTO> LoginUserAsync(LoginUserInputDTO request)
        {
            var response = new LoginUserOutputDTO();
            try
            {
                var userCredential = await _authClient.SignInWithEmailAndPasswordAsync(request.Email, request.Password);
                var token = await userCredential.User.GetIdTokenAsync();
                var userId = userCredential.User.Uid; // Obtienes el UID del usuario
                response.Token = token;
                response.UserId = userId;
                response.Success = true;
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                response.Success = false;
                response.ErrorMessage = "Credenciales incorrectas";
            }
            return response;
        }

        public async Task<DeleteUserOutputDTO> DeleteDataUserAsync(DeleteUserInputDTO request)
        {
            var response = new DeleteUserOutputDTO();
            FirebaseClient firebaseClient = null; // Declare outside try for wider scope

            try
            {

                firebaseClient = new FirebaseClient(
                    _databaseUrl,
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Token) } // Use the fetched token
                );

                // --- Step 1: Retrieve all data for the user ---
                var userHealthDataPath = $"healthData/{request.UserId}";
                var dataToArchive = await firebaseClient
                    .Child("healthData")
                    .Child(request.UserId)
                    .OnceAsJsonAsync(); // Get all data under the user's healthData node

                if (dataToArchive == null)
                {
                    response.Success = false;
                    response.ErrorMessage = "No data found for the specified user to delete.";
                    return response;
                }

                // --- Step 2: Store the retrieved data in the "deletedHealthData" path ---
                var deletedDataPath = $"deletedHealthData/{request.UserId}_{DateTime.UtcNow:yyyyMMddHHmmss}"; // Unique identifier for archived data
                await firebaseClient
                   .Child("healthData")
                   .Child(request.UserId)
                   .Child("datos_personales")
                   .DeleteAsync();
                response.Success = true;
                await firebaseClient
                .Child("healthData")
                .Child(request.UserId)
                .Child("dias")
                .DeleteAsync();
                response.Success = true;
                await firebaseClient
                .Child("healthData")
                .Child(request.UserId)
                .Child("Prediccion")
                .DeleteAsync();

                #region profile
                var profile = new PersonalDataModel
                {
                    Conexion = new PersonalDataModel.ConnectionModel
                    {
                        ConnectionStatus = 0
                    },
                    DateOfBirth = null, // Puedes asignar una fecha en formato string o dejarla en null
                    HasPredisposition = false,
                    Height = 0,
                    Weight = 0,
                    Gender = 0,
                    Smoke = 0,
                    Alcohol = 0,
                    Choresterol = 0,
                    PhotoURL = null,
                    Name = null,
                    Active = false
                };
                #endregion profile

                var profileRef = firebaseClient
                   .Child("healthData")
                   .Child(request.UserId)
                   .Child("datos_personales");
                await profileRef.PutAsync(profile);
                #region healthData
                var healthData = new HealthDataInputModel
                {
                    UserId = null,
                    Steps = 0,
                    ActiveCalories = 0.0,
                    // Puedes inicializar la lista con un valor 0 o dejarla vacía según tus necesidades
                    HeartRates = new List<int?> { 0 },
                    // Para las colecciones de data, se puede crear un único objeto con valores por defecto
                    HeartRateData = new List<HeartRateDataPoint>
    {
        new HeartRateDataPoint { Time = DateTime.MinValue, BPM = 0 }
    },
                    RestingHeartRate = 0.0,
                    Weight = 0.0,
                    Height = 0.0,
                    BloodPressureData = new List<BloodPressureDataPoint>
    {
        new BloodPressureDataPoint { Time = DateTime.MinValue, Systolic = 0.0, Diastolic = 0.0 }
    },
                    OxygenSaturationData = new List<OxygenSaturationDataPoint>
    {
        new OxygenSaturationDataPoint { Time = DateTime.MinValue, Percentage = 0.0 }
    },
                    BloodGlucoseData = new List<BloodGlucoseDataPoint>
    {
        new BloodGlucoseDataPoint { Time = DateTime.MinValue, BloodGlucose = 0.0 }
    },
                    BodyTemperature = 0.0,
                    TemperatureData = new List<TemperatureDataPoint>
    {
        new TemperatureDataPoint { Time = DateTime.MinValue, Temperature = 0.0 }
    },
                    RespiratoryRateData = new List<RespiratoryRateDataPoint>
    {
        new RespiratoryRateDataPoint { Time = DateTime.MinValue, Rate = 0.0 }
    },
                    SleepData = new List<SleepSessionDataPoint>
    {
        new SleepSessionDataPoint
        {
            StartTime = DateTime.MinValue,
            EndTime = DateTime.MinValue,
            Stages = new List<SleepStageDataPoint>
            {
                new SleepStageDataPoint
                {
                    Type = null, // o string.Empty, si prefieres que sea cadena vacía
                    StartTime = DateTime.MinValue,
                    EndTime = DateTime.MinValue
                }
            }
        }
    }
                };
                #endregion healthData
                var currentDate = DateTime.UtcNow.ToString("yyyy-MM-dd");
                await firebaseClient
              .Child("healthData")
              .Child(request.UserId)
              .Child("dias")
              .Child(currentDate)
              .PutAsync(healthData);
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                response.Success = false;
                response.ErrorMessage = "Error al eliminar el usuario de la autenticación: " + ex.Reason;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = "Error al eliminar la información del usuario: " + ex.Message;
            }

            return response;
        }
        public async Task<DeleteUserOutputDTO> DeleteUserAsync(DeleteUserInputDTO request)
        {
            var response = new DeleteUserOutputDTO();
            try
            {
                // --- Inicialización del Firebase Admin SDK (si no está ya inicializado globalmente) ---
                string baseDir = AppDomain.CurrentDomain.BaseDirectory;
                // Crea la ruta a la credencial DENTRO de la carpeta del programa
                string rutajson = Path.Combine(baseDir, "Recursos", "proyectocsharp-tfg-firebase-adminsdk-fbsvc-a393e8de19.json"); // Asegúrate que el nombre/ruta es correcto
                if (FirebaseApp.DefaultInstance == null)
                {
                    FirebaseApp.Create(new AppOptions()
                    {
                        Credential = GoogleCredential.FromFile(rutajson),
                    });
                }
                var firebaseClient = new FirebaseClient(
                    _databaseUrl,
                    new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Token) }
                );

                var userCredentials = new UserCredentials { UserId = request.UserId, IdToken = request.Token };
                var getMonitoringInput = new GetMonitoringUsersInputDTO { Credentials = userCredentials };
                var monitoringUsersResponse = await GetMonitoringUsersAsync(getMonitoringInput);
                List<MonitorUserModel> monitors = new List<MonitorUserModel>();

                if (monitoringUsersResponse.Success && monitoringUsersResponse.MonitoringUsers != null)
                {
                    monitors = monitoringUsersResponse.MonitoringUsers;
                }

                var getConnectedInput = new ConnectedUsersInputDTO { Credentials = userCredentials };
                List<ConnectedUserModel> connectedUsers = new List<ConnectedUserModel>();
                connectedUsers = await GetConnectedUsersAsync(getConnectedInput) ?? new List<ConnectedUserModel>();


                foreach (var monitor in monitors)
                {
                        await firebaseClient
                            .Child("healthData")
                            .Child(monitor.MonitoringUserId)
                            .Child("conexiones")
                            .Child(request.UserId)
                            .DeleteAsync();
                }

                foreach (var connectedUser in connectedUsers)
                {
                        await firebaseClient
                            .Child("healthData")
                            .Child(connectedUser.ConnectedUserId) 
                            .Child("monitores")
                            .Child(request.UserId) 
                            .DeleteAsync();
                }

                await FirebaseAuth.DefaultInstance.DeleteUserAsync(request.UserId);

                await firebaseClient
                    .Child("healthData")
                    .Child(request.UserId)
                    .DeleteAsync();

                response.Success = true;
            }
            catch (FirebaseAdmin.Auth.FirebaseAuthException ex) // Error específico de Auth
            {
                response.Success = false;
                response.ErrorMessage = $"Error al eliminar el usuario de la autenticación ({ex.AuthErrorCode}): {ex.Message}";
            }
            catch (Firebase.Database.FirebaseException ex) // Error específico de Database
            {
                response.Success = false;
                response.ErrorMessage = "Error al interactuar con la base de datos durante la eliminación: " + ex.Message;
            }
            catch (Exception ex) // Otros errores generales
            {
                response.Success = false;
                response.ErrorMessage = "Error general durante la eliminación del usuario: " + ex.ToString(); // Usar ToString para más detalle si es necesario
            }

            return response;
        }
        public async Task<GetMonitoringUsersOutputDTO> GetMonitoringUsersAsync(GetMonitoringUsersInputDTO request)
        {
            var response = new GetMonitoringUsersOutputDTO();
            response.MonitoringUsers = new List<MonitorUserModel>();
            try
            {
                var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
                bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
                FirebaseClient client;

                if (isTokenFromAdmin)
                {
                    Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }
                // Si es un usuario normal, verificamos seguridad y usamos su token.
                else
                {
                    if (decodedToken.Uid != request.Credentials.UserId)
                    {
                        throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                    }

                    client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
                }

                var monitorsData = await client
                    .Child("healthData")
                    .Child(request.Credentials.UserId)
                    .Child("monitores")
                    .OnceAsync<object>(); 

                if (monitorsData == null || !monitorsData.Any())
                {
                    response.Success = true;
                    return response;
                }

                foreach (var monitorFirebaseObject in monitorsData)
                {
                    var monitoringUserId = monitorFirebaseObject.Key;

                    if (string.IsNullOrEmpty(monitoringUserId)) continue;

                    string isAdmin="False";
                    try
                    {
                        var storedData = monitorFirebaseObject.Object as Newtonsoft.Json.Linq.JObject;
                        MonitorUserModel m = storedData.ToObject<MonitorUserModel>();
                        if (storedData != null)
                        {
                            isAdmin = m.isAdmin; 
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error deserializing monitor data for {monitoringUserId}: {ex.Message}");
                    }

                    var emailResponse = await GetUserEmailByIdAsync(new GetUserEmailByIdInputDTO { UserId = monitoringUserId });
                    string email = emailResponse?.Email ?? "No Email"; // Añadir manejo de posible null en emailResponse

                    // Obtener datos personales del usuario monitor
                    PersonalDataModel personalData = null;
                    try
                    {
                        personalData = await client
                            .Child("healthData")
                            .Child(monitoringUserId)
                            .Child("datos_personales")
                            .OnceSingleAsync<PersonalDataModel>();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error fetching personal data for {monitoringUserId}: {ex.Message}");
                    }


                    var monitoringUser = new MonitorUserModel
                    {
                        MonitoringUserId = monitoringUserId,
                        Name = personalData?.Name ?? "No Name",
                        PhotoURL = personalData?.PhotoURL ?? "No photo",
                        Email = email,
                        isAdmin = isAdmin 
                    };

                    response.MonitoringUsers.Add(monitoringUser);
                }

                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error al obtener usuarios que monitorean: {ex.ToString()}";
            }
            return response;
        }

        public async Task<GetUserEmailByIdOutputDTO> GetUserEmailByIdAsync(GetUserEmailByIdInputDTO request)
        {
            var response = new GetUserEmailByIdOutputDTO();
            try
            {
                UserRecord user = await FirebaseAuth.DefaultInstance.GetUserAsync(request.UserId);
                response.Email = user.Email;
                response.Success = true;
            }
            catch (Exception ex)
            {
                response.Success = false;
                response.ErrorMessage = $"Error al obtener email del usuario: {ex.Message}";
            }
            return response;
        }

        public async Task ChangePasswordAsync(ChangePasswordRequestInputDTO input)
        {
            try
            {
                await _authClient.ResetEmailPasswordAsync(input.email);
            }
            catch (Firebase.Auth.FirebaseAuthException ex)
            {
                throw new Exception("Error al cambiar la contraseña: " + ex.Reason);
            }
        }

        public async Task<List<ConnectedUserModel>> GetConnectedUsersAsync(ConnectedUsersInputDTO request)
        {
            var decodedToken = await FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(request.Credentials.IdToken);
            bool isTokenFromAdmin = decodedToken.Claims.TryGetValue("is_admin", out var isAdminClaim) && (bool)isAdminClaim;
            FirebaseClient client;

            if (isTokenFromAdmin)
            {
                Console.WriteLine($"Acción de administrador para usuario: {request.Credentials.UserId}");
                client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
            }
            // Si es un usuario normal, verificamos seguridad y usamos su token.
            else
            {
                if (decodedToken.Uid != request.Credentials.UserId)
                {
                    throw new Exception($"Error de seguridad: Un usuario intentó escribir en la cuenta de otro.");
                }

                client = new FirebaseClient(_databaseUrl, new FirebaseOptions { AuthTokenAsyncFactory = () => Task.FromResult(request.Credentials.IdToken) });
            }


            // Obtener lista de conexiones del usuario actual
            var connections = await client
                .Child("healthData")
                .Child(request.Credentials.UserId)
                .Child("conexiones")
                .OnceAsync<object>();

            if (connections == null || !connections.Any())
                return new List<ConnectedUserModel>();

            var connectedUsers = new List<ConnectedUserModel>();

            foreach (var connection in connections)
            {
                var connectedUserId = connection.Key;

                // Obtener datos personales del usuario conectado
                var personalData = await client
                    .Child("healthData")
                    .Child(connectedUserId)
                    .Child("datos_personales")
                    .OnceSingleAsync<PersonalDataModel>();

                // Usar ReadDataAsync (versión con DTO) para obtener la última fecha con datos
                var healthDataResult = await this.ReadDataAsync(
                    new ReadDataInputDTO
                    {
                        Credentials = new UserCredentials
                        {
                            UserId = connectedUserId,
                            IdToken = request.Credentials.IdToken
                        }
                    });

                // Extraer los datos de la respuesta
                string latestDay = "No days available";
                HealthDataOutputModel healthData = null;

                if (healthDataResult != null && healthDataResult.Success)
                {
                    latestDay = healthDataResult.SelectedDate;
                    healthData = healthDataResult.Data;
                }

                var connectedUser = new ConnectedUserModel
                {
                    ConnectedUserId = connectedUserId,
                    Name = personalData?.Name ?? "No Name",
                    PhotoURL = personalData?.PhotoURL ?? "No photo",
                    LatestDay = latestDay,
                    MaxHeartRate = healthData?.MaxHeartRate?.ToString() ?? "N/A",
                    MinHeartRate = healthData?.MinHeartRate?.ToString() ?? "N/A",
                    AvgHeartRate = healthData?.AvgHeartRate?.ToString() ?? "N/A"
                };

                connectedUsers.Add(connectedUser);
            }

            return connectedUsers;
        }
    }
}







