using Microsoft.AspNetCore.Mvc;
using BlutTruck.Application_Layer.IServices;
using System.Threading.Tasks;
using BlutTruck.Application_Layer.Models;
using Application.Services;
using BlutTruck.Application_Layer.Services;
using static BlutTruck.Application_Layer.Models.PersonalDataModel;
using Firebase.Database;
using Firebase.Auth.Repository;
using Microsoft.AspNetCore.Authorization;
using BlutTruck.Application_Layer.Models.InputDTO;
using BlutTruck.Application_Layer.Models.OutputDTO;

namespace Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WriteDataController : ControllerBase
    {
        private readonly IHealthDataService _healthDataService;

        public WriteDataController(IHealthDataService healthDataService)
        {
            _healthDataService = healthDataService;
        }

        [HttpPost("write")]
        public async Task<IActionResult> WriteData([FromBody] WriteDataInputDTO request)
        {
            if (request == null || request.Credentials == null || string.IsNullOrEmpty(request.Credentials.UserId))
            {
                return BadRequest(new { Message = "El cuerpo de la solicitud es inválido o falta el UserId." });
            }

            try
            {
                if (string.IsNullOrEmpty(request.Credentials.IdToken) || request.Credentials.IdToken == "string")
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación de administrador." });
                    }
                }
                await _healthDataService.WriteDataAsync(request);

                return Ok(new { Message = "Datos procesados correctamente." });
            }
            catch (Exception ex)
            {
                // El método del repositorio lanzará una excepción si hay problemas de seguridad o de otro tipo.
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("writeWeb")]
        public async Task<IActionResult> WriteDataWeb([FromBody] WriteDataInputDTO request)
        {
            if (request == null || request.Credentials == null || string.IsNullOrEmpty(request.Credentials.UserId))
            {
                return BadRequest(new { Message = "El cuerpo de la solicitud es inválido o falta el UserId." });
            }

            try
            {
                if (string.IsNullOrEmpty(request.Credentials.IdToken) || request.Credentials.IdToken == "string")
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación de administrador." });
                    }
                }
                await _healthDataService.WriteDataWebAsync(request);

                return Ok(new { Message = "Datos procesados correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("writePrediction")]
        public async Task<IActionResult> writePrediction([FromBody] PredictionInputDTO request)
        {
            if (request == null || request.Credentials == null || string.IsNullOrEmpty(request.Credentials.UserId))
            {
                return BadRequest(new { Message = "El cuerpo de la solicitud es inválido o falta el UserId." });
            }

            try
            {
                if (string.IsNullOrEmpty(request.Credentials.IdToken) || request.Credentials.IdToken == "string")
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación de administrador." });
                    }
                }
                await _healthDataService.writePredictionAsync(request);
                return Ok(new { Message = "Datos procesados correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("writeAdmin")]
        public async Task<IActionResult> writeAdmin([FromBody] AdminInputDTO request)
        {
            if (request == null || request.Credentials == null || string.IsNullOrEmpty(request.Credentials.UserId))
            {
                return BadRequest(new { Message = "El cuerpo de la solicitud es inválido o falta el UserId." });
            }

            try
            {
                if (string.IsNullOrEmpty(request.Credentials.IdToken) || request.Credentials.IdToken == "string")
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación de administrador." });
                    }
                }
                await _healthDataService.writeAdminAsync(request);

                return Ok(new { Message = "Datos procesados correctamente." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("registerConnection")]
        public async Task<IActionResult> RegisterConnection([FromBody] RegisterConnectionInputDTO request)
        {
            if (request == null ||
                request.ConnectedUserId == null ||
                string.IsNullOrEmpty(request.CurrentUserId) ||
                string.IsNullOrEmpty(request.ConnectedUserId))
            {
                return BadRequest(new { Message = "El cuerpo de la solicitud es inválido o falta información necesaria." });
            }

            try
            {
                if (request.IdToken == "string" || request.IdToken == null)
                {
                    request.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                RegisterConnectionOutputDTO response = await _healthDataService.RegisterConnectionAsync(request);
                return Ok(new { Message = "Conexión registrada exitosamente", Result = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("registerCodeConnection")]
        public async Task<IActionResult> registerCodeConnection([FromBody] RegisterCodeConnectionInputDTO request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.CurrentUserId) ||
                string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new { Message = "El cuerpo de la solicitud es inválido o falta información necesaria." });
            }

            try
            {
                if (request.IdToken == "string" || request.IdToken == null)
                {
                    request.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                RegisterConnectionOutputDTO response = await _healthDataService.RegisterCodeConnectionAsync(request);
                return Ok(new { Message = "Conexión registrada exitosamente", Result = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("GetCodeConnection")]
        public async Task<IActionResult> GetCodeConnection([FromBody] DeleteCodeConnectionInputDTO request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new { Message = "El cuerpo de la solicitud es inválido o falta información necesaria." });
            }

            try
            {
                if (request.IdToken == "string" || request.IdToken == null)
                {
                    request.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                GetConnectionOutputDTO response = await _healthDataService.GetCodeConnectionAsync(request);
                return Ok(new { Message = "Conexión registrada exitosamente", Result = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("deleteCodeConnection")]
        public async Task<IActionResult> DeleteCodeConnection([FromBody] DeleteCodeConnectionInputDTO request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Code))
            {
                return BadRequest(new { Message = "El cuerpo de la solicitud es inválido o falta información necesaria." });
            }

            try
            {
                if (request.IdToken == "string" || request.IdToken == null)
                {
                    request.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                DeleteConnectionOutputDTO response = await _healthDataService.DeleteCodeConnectionAsync(request);
                return Ok(new { Message = "Conexión registrada exitosamente", Result = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }
        // Se espera que el cliente envíe el currentUserId y el token como parte del DTO.
        [HttpPost("getMonitoringUsers")]
        public async Task<IActionResult> GetMonitoringUsers([FromBody] GetMonitoringUsersInputDTO request)
        {
            if (request == null ||
                request.Credentials == null ||
                string.IsNullOrEmpty(request.Credentials.UserId))
            {
                return BadRequest(new { Message = "El UserId y el IdToken son obligatorios." });
            }

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                GetMonitoringUsersOutputDTO monitoringUsers = await _healthDataService.GetMonitoringUsersAsync(request);
                return Ok(monitoringUsers.MonitoringUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpDelete("deleteConnection")]
        public async Task<IActionResult> DeleteConnection([FromBody] DeleteConnectionInputDTO request)
        {
            if (request == null ||
                request.Credentials == null ||
                string.IsNullOrEmpty(request.Credentials.UserId) ||
                string.IsNullOrEmpty(request.ConnectedUserId))
            {
                return BadRequest(new { Message = "El cuerpo de la solicitud es inválido o falta información necesaria." });
            }

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                DeleteConnectionOutputDTO response = await _healthDataService.DeleteConnectionAsync(request);
                return Ok(new { Message = response.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserInputDTO request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Password) ||
                string.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new { Message = "Todos los campos son obligatorios." });
            }

            try
            {
               
                RegisterUserOutputDTO response = await _healthDataService.RegisterUserAsync(request);
                return Ok(new { Token = response.Token, Message = "Registro exitoso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [HttpPost("googleRegister")]
        public async Task<IActionResult> GoogleRegister([FromBody] RegisterGoogleUserInputDTO request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Name))
            {
                return BadRequest(new { Message = "Todos los campos son obligatorios." });
            }

            try
            {

                RegisterUserOutputDTO response = await _healthDataService.RegisterGoogleUserAsync(request);
                return Ok(new { Token = response.Token, Message = "Registro exitoso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginUserInputDTO request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Email) ||
                string.IsNullOrEmpty(request.Password))
            {
                return BadRequest(new { Message = "Correo y contraseña son obligatorios." });
            }

            try
            {
                LoginUserOutputDTO response = await _healthDataService.LoginUserAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpDelete("deletedata")]
        public async Task<IActionResult> Delete([FromBody] DeleteUserInputDTO request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { Message = "Correo y contraseña son obligatorios." });
            }

            try
            {
                if (request.Token == "string" || request.Token == null)
                {
                    request.Token = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Token))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                DeleteUserOutputDTO response = await _healthDataService.DeleteDataUserAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpDelete("deleteuser")]
        public async Task<IActionResult> Deleteuser([FromBody] DeleteUserInputDTO request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.UserId))
            {
                return BadRequest(new { Message = "Correo y contraseña son obligatorios." });
            }

            try
            {
                if (request.Token == "string" || request.Token == null)
                {
                    request.Token = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Token))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                DeleteUserOutputDTO response = await _healthDataService.DeleteUserAsync(request);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestInputDTO request)
        {
            if (string.IsNullOrEmpty(request.email))
            {
                return BadRequest(new { Message = "El token y la nueva contraseña son obligatorios." });
            }

            try
            {
                await _healthDataService.ChangePasswordAsync(request);
                return Ok(new { Message = "Correo enviado exitosamente." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }


        [HttpPost("save-profile")]
        public async Task<IActionResult> SaveUserProfileAsync([FromBody] SaveUserProfileInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId) || request.Profile == null)
            {
                return BadRequest(new { Message = "El UserId y el perfil son obligatorios." });
            }

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                SaveUserProfileOutputDTO response = await _healthDataService.SaveUserProfileAsync(request);
                return Ok(new { Message = "Perfil guardado correctamente.", Data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al guardar el perfil: {ex.Message}" });
            }
        }

        [HttpPost("update-connection")]
        public async Task<IActionResult> UpdateConnectionStatusAsync([FromBody] UpdateConnectionStatusInputDTO request)
        {
            if (request == null ||
                string.IsNullOrEmpty(request.Credentials.UserId) ||
                request.ConnectionStatus == null ||
                request.ConnectionStatus.ConnectionStatus == null)
            {
                return BadRequest(new { Message = "El UserId y el estado de conexión son obligatorios." });
            }

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                UpdateConnectionStatusOutputDTO response = await _healthDataService.UpdateConnectionStatusAsync(request);
                return Ok(new { Message = "Estado de conexión actualizado correctamente.", Data = response });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al actualizar el estado de conexión: {ex.Message}" });
            }
        }
    }


    [ApiController]
    [Route("api/[controller]")]
    public class ReadDataController : ControllerBase
    {
        private readonly IHealthDataService _healthDataService;

        public ReadDataController(IHealthDataService healthDataService)
        {
            _healthDataService = healthDataService;
        }

        [HttpPost("recentday")]
        public async Task<IActionResult> ReadData([FromBody] ReadDataInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var data = await _healthDataService.ReadDataAsync(request);
                if (data == null)
                    return NotFound(new { Message = "No se encontraron datos para el usuario especificado." });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("selectday")]
        public async Task<IActionResult> GetSelectDateHealthDataAsync([FromBody] SelectDateHealthDataInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId) || string.IsNullOrEmpty(request.DateKey))
                return BadRequest(new { Message = "El UserId y la fecha son obligatorios." });

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var healthData = await _healthDataService.GetSelectDateHealthDataAsync(request);
                if (healthData == null)
                    return NotFound(new { Message = $"No se encontraron datos para la fecha {request.DateKey}." });

                return Ok(healthData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al obtener los datos: {ex.Message}" });
            }
        }

        [HttpPost("full")]
        public async Task<IActionResult> GetFullHealthDataAsync([FromBody] UserCredentials request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.IdToken == "string" || request.IdToken == null)
                {
                    request.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                FullDataOutputDTO healthData = await _healthDataService.GetFullHealthDataAsync(request);
                if (healthData == null)
                    return NotFound(new { Message = "Datos no encontrados." });

                return Ok(healthData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al obtener los datos: {ex.Message}" });
            }
        }

        [HttpPost("fullAndConnection")]
        public async Task<IActionResult> GetFullHealthDataAndConnectAsync([FromBody] FullAndMonitoringInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                FullDataOutputDTO healthData = await _healthDataService.GetFullHealthDataAndConnectAsync(request);
                if (healthData == null)
                    return NotFound(new { Message = "Datos no encontrados." });

                return Ok(healthData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al obtener los datos: {ex.Message}" });
            }
        }

        [HttpPost("connected-users")]
        public async Task<IActionResult> GetConnectedUsers([FromBody] ConnectedUsersInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId))
                return BadRequest(new { Message = "El Credentials.UserId es requerido." });

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var connectedUsers = await _healthDataService.GetConnectedUsersAsync(request);
                return Ok(connectedUsers);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Ocurrió un error: {ex.Message}" });
            }
        }

        [HttpPost("latest")]
        public async Task<IActionResult> GetPersonalAndLatestDayDataAsync([FromBody] GetPersonalAndLatestDayDataInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var healthData = await _healthDataService.GetPersonalAndLatestDayDataAsync(request);
                if (healthData == null)
                    return NotFound(new { Message = "Datos no encontrados." });

                return Ok(healthData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al obtener los datos: {ex.Message}" });
            }
        }

        [HttpPost("getprediction")]
        public async Task<IActionResult> GetPredictionDataAsync([FromBody] UserCredentials request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.IdToken == "string" || request.IdToken == null)
                {
                    request.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    // Si no se pudo obtener un token, retorna Unauthorized
                    if (string.IsNullOrEmpty(request.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var predictionData = await _healthDataService.GetPredictionAsync(request);
                if (predictionData == null)
                {
                    return NotFound(new { Message = $"No se encontraron datos de predicción para el usuario {request.UserId}." });
                }
                return Ok(predictionData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al obtener los datos de predicción: {ex.Message}" });
            }
        }


        [HttpPost("getlistprediction")]
        public async Task<IActionResult> GetPredictionListDataAsync([FromBody] UserCredentials request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.IdToken == "string" || request.IdToken == null)
                {
                    request.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    // Si no se pudo obtener un token, retorna Unauthorized
                    if (string.IsNullOrEmpty(request.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var predictionData = await _healthDataService.GetListPredictionAsync(request);
                if (predictionData == null)
                {
                    return NotFound(new { Message = $"No se encontraron datos de predicción para el usuario {request.UserId}." });
                }
                return Ok(predictionData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al obtener los datos de predicción: {ex.Message}" });
            }
        }

        [HttpPost("personal")]
        public async Task<IActionResult> GetPersonalDataAsync([FromBody] GetPersonalDataInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var personalData = await _healthDataService.GetPersonalDataAsync(request);
                if (personalData == null)
                    return NotFound(new { Message = "Datos personales no encontrados para el usuario." });

                return Ok(personalData);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al obtener los datos: {ex.Message}" });
            }
        }

        [HttpPost("get-pdf")]
        public async Task<IActionResult> DownloadPdf([FromBody] PdfInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var pdfOutput = await _healthDataService.GeneratePdfAsync(request);
                return File(pdfOutput.PdfBytes, "application/pdf", "datosSalud.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al generar el PDF: {ex.Message}" });
            }
        }

        [HttpPost("get-connection")]
        public async Task<IActionResult> GetConnectionStatusAsync([FromBody] GetConnectionStatusInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var connectionStatus = await _healthDataService.GetConnectionStatusAsync(request);
                if (connectionStatus == null)
                    return NotFound(new { Message = "No se encontró el estado de conexión." });

                return Ok(new { ConnectionStatus = connectionStatus.ConnectionStatus });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al obtener el estado de conexión: {ex.Message}" });
            }
        }

        [HttpPost("getfavorites")]
        public async Task<IActionResult> GetFavoritesAsync([FromBody] UserCredentials request)
        {
            if (request == null || string.IsNullOrEmpty(request.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.IdToken == "string" || request.IdToken == null)
                {
                    request.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var connectionStatus = await _healthDataService.GetFavoritesAsync(request);
                if (connectionStatus == null)
                    return NotFound(new { Message = "No se encontró el estado de conexión." });

                connectionStatus.Success = true;
                return Ok(connectionStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al obtener el estado de conexión: {ex.Message}" });
            }
        }

        [HttpPost("setfavorites")]
        public async Task<IActionResult> SetFavoritesAsync([FromBody] SetFavoritesInputDTO request)
        {
            if (request == null || string.IsNullOrEmpty(request.Credentials.UserId))
                return BadRequest(new { Message = "El UserId es requerido." });

            try
            {
                if (request.Credentials.IdToken == "string" || request.Credentials.IdToken == null)
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }
                var connectionStatus = await _healthDataService.SetFavoritesAsync(request);
                if (connectionStatus == null)
                    return NotFound(new { Message = "No se encontró el estado de conexión." });

                return Ok(connectionStatus);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al obtener el estado de conexión: {ex.Message}" });
            }
        }

        [HttpPost("get-pdf-by-type")]
        public async Task<IActionResult> DownloadPdfByDataTypeAsync([FromBody] PdfInputByDataTypeDTO request)
        {
            if (request == null || request.Credentials == null || string.IsNullOrEmpty(request.Credentials.UserId))
            {
                return BadRequest(new { Message = "El UserId en las credenciales es requerido." });
            }

            try
            {
                if (string.IsNullOrEmpty(request.Credentials.IdToken) || request.Credentials.IdToken == "string")
                {
                    request.Credentials.IdToken = await _healthDataService.AuthenticateAndGetTokenAsync();
                    if (string.IsNullOrEmpty(request.Credentials.IdToken))
                    {
                        return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
                    }
                }

                var pdfOutput = await _healthDataService.GeneratePdfByDataTypeAsync(request);

                if (pdfOutput == null || pdfOutput.PdfBytes == null || pdfOutput.PdfBytes.Length == 0)
                {
                    return NotFound(new { Message = $"No se pudo generar el PDF para el tipo de dato '{request.DataType}' o el contenido está vacío." });
                }

                // Devolver el archivo PDF
                string fileName = $"datosSalud_{request.Credentials.UserId}_{request.DataType}.pdf";
                return File(pdfOutput.PdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = $"Error al generar el PDF para el tipo de dato '{request.DataType}': {ex.Message}" });
            }
        }
    }
}


