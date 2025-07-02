using System.Net.Mail; // Agregar esta referencia
using System.Net; // Para credenciales y cliente de red
using System.Threading.Tasks;
using BlutTruck.Application_Layer.IServices;
using Microsoft.AspNetCore.Mvc;
using Firebase.Database;
using Firebase.Database.Query;
using Firebase.Auth;
using Google.Api;
using Firebase.Auth.Providers;
using BlutTruck.Application_Layer.Models;
using System.Linq;
using System;
using BlutTruck.Application_Layer.Models.InputDTO;



namespace BlutTruck.API_Layer.Controllers
{
    public class PredictionRequestDTO

    {
        public string UserId { get; set; } = string.Empty;
    }

    [ApiController]
    [Route("api/[controller]")]
    public class PrediccionController : ControllerBase
    {
        private readonly IPrediccionService _prediccionService;
        private readonly IHealthDataService _healthDataService;
        public PrediccionController(IPrediccionService prediccionService, IHealthDataService healthDataService)
        {
            _prediccionService = prediccionService;
            _healthDataService = healthDataService;
        }

        [HttpPost("PredictHealthRisk")]
    public async Task<IActionResult> PredictHealthRisk([FromBody] PredictionRequestDTO requestDto)
    {
        try
        {
            string userId = requestDto.UserId;

            // --- Autenticación y obtención de datos (como lo tenías) ---
            var token = await _healthDataService.AuthenticateAndGetTokenAsync();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { Message = "No se pudo generar el token de autenticación." });
            }

            var uid = await _healthDataService.VerifyUserTokenAsync(token);
            if (uid == null)
            {
                return Unauthorized(new { Message = "El token generado no es válido." });
            }

            var request = new GetPersonalAndLatestDayDataInputDTO
            {
                Credentials = new UserCredentials { UserId = userId, IdToken = token }
            };
            var personalAndLatestDayData = await _healthDataService.GetPersonalAndLatestDayDataAsync(request);
            if (personalAndLatestDayData == null)
            {
                // Considera devolver un NotFound si el usuario o los datos no existen
                return BadRequest(personalAndLatestDayData?.ErrorMessage ?? "No se pudieron obtener los datos del usuario.");
            }

            PersonalDataModel personalData = personalAndLatestDayData.PersonalData;
            HealthDataOutputModel latestDayData = personalAndLatestDayData.DiaMasReciente?.Datos; // Null check

            if (latestDayData == null)
            {
                return BadRequest("No se encontraron datos de salud recientes para la predicción.");
            }
            if (!DateTime.TryParse(personalData.DateOfBirth, out DateTime dateOfBirth))
            {
                return BadRequest("Fecha de nacimiento inválida.");
            }

            // --- Cálculo de edad (como lo tenías) ---
            int edad = DateTime.Now.Year - dateOfBirth.Year;
            if (DateTime.Now.Month < dateOfBirth.Month ||
                (DateTime.Now.Month == dateOfBirth.Month && DateTime.Now.Day < dateOfBirth.Day))
            {
                edad--;
            }

            // --- Mapeo y Predicción (como lo tenías) ---
            var predictionRequest = MapToPredictionRequest(personalData, latestDayData, edad);
            var predictionResult = await _prediccionService.PredecirAsync(predictionRequest); 

            // --- Definición de thresholds y obtención de valores (como lo tenías) ---
            const double thresholdGlucosa = 140;
            const double thresholdPresionSistolica = 140; 
            const double thresholdPresionDiastolica = 80;
            const double thresholdColesterol = 240;

            var latestGlucose = latestDayData.BloodGlucoseData?.LastOrDefault();
            var latestBloodPressure = latestDayData.BloodPressureData?.LastOrDefault();

            // Usar valores por defecto o manejar la ausencia de datos de forma más robusta si es necesario
            double bloodGlucose = latestGlucose?.BloodGlucose ?? 0;
            double systolicPressure = latestBloodPressure?.Systolic ?? 120; // Valor típico si no hay datos
            double diastolicPressure = latestBloodPressure?.Diastolic ?? 80; // Valor típico si no hay datos
            double cholesterol = 0;
            double.TryParse(personalData.Choresterol?.ToString(), out cholesterol); // Intentar convertir de forma segura

            // --- Evaluar parámetros y acumular alertas (como lo tenías) ---
            var riskAlerts = new List<(string parameter, double value, string message)>();

            if (bloodGlucose > thresholdGlucosa)
            {
                riskAlerts.Add(("Glucosa", bloodGlucose, $"Nivel de glucosa elevado: {bloodGlucose} mg/dL."));
            }
            if (systolicPressure > thresholdPresionSistolica)
            {
                riskAlerts.Add(("Presión sistólica", systolicPressure, $"Presión sistólica elevada: {systolicPressure} mmHg."));
            }
            if (diastolicPressure > thresholdPresionDiastolica)
            {
                riskAlerts.Add(("Presión diastólica", diastolicPressure, $"Presión diastólica elevada: {diastolicPressure} mmHg."));
            }
            if (cholesterol > thresholdColesterol)
            {
                riskAlerts.Add(("Colesterol", cholesterol, $"Nivel de colesterol elevado: {cholesterol} mg/dL."));
            }

            // --- Crear el DTO para guardar la predicción DESPUÉS de calcular las alertas ---
            var requestdbprediction = new PredictionInputDTO
            {
                Credentials = new UserCredentials
                {
                    UserId = userId,
                    IdToken = token 
                },
                Prediction = predictionResult, 
                SpecificAlerts = riskAlerts.Select(alert => alert.message).ToList()
            };

            // --- Guardar la predicción con las alertas ---
            await _healthDataService.writePredictionAsync(requestdbprediction);

            // --- Obtener usuarios monitores y enviar notificaciones (como lo tenías) ---
            var request2 = new GetMonitoringUsersInputDTO
            {
                Credentials = new UserCredentials { UserId = userId, IdToken = token }
            };
            var monitoringUsers = await _healthDataService.GetMonitoringUsersAsync(request2);

            if (monitoringUsers != null && monitoringUsers.MonitoringUsers != null) // Verificar nulidad
            {
                if (IsHighRisk(predictionResult))
                {
                    foreach (var monitor in monitoringUsers.MonitoringUsers)
                    {
                        if (!string.IsNullOrEmpty(monitor?.Email)) // Verificar email
                        {
                            await SendHighRiskNotificationAsync(monitor.Email, predictionResult);
                        }
                    }
                }

                if (riskAlerts.Any())
                {
                    string riskDetails = string.Join("<br/>", riskAlerts.Select(r => r.message));
                    foreach (var monitor in monitoringUsers.MonitoringUsers)
                    {
                        if (!string.IsNullOrEmpty(monitor?.Email)) // Verificar email
                        {
                            await SendConsolidatedRiskNotificationAsync(monitor.Email, riskDetails);
                        }
                    }
                }
            }


            // --- Devolver resultado (como lo tenías, puedes incluir las alertas aquí también si quieres) ---
            return Ok(new
            {
                Prediccion = predictionResult,
                Fecha = personalAndLatestDayData.DiaMasReciente?.Fecha,
                DatosUtilizados = predictionRequest,
                AlertasEspecificasGeneradas = riskAlerts.Select(a => a.message).ToList() // Devuelve los mensajes de alerta en la respuesta API
                                                                                         // O si usaste la estructura detallada:
                                                                                         // AlertasEspecificasGeneradas = riskAlerts.Select(a => new { Parametro = a.parameter, Valor = a.value, Mensaje = a.message }).ToList()
            });
        }
        catch (Exception ex)
        {
            // Loggear el error sería una buena práctica aquí
            // _logger.LogError(ex, "Error en PredictHealthRisk para UserId {UserId}", requestDto?.UserId);
            return StatusCode(500, new { error = $"Se produjo un error interno en el servidor: {ex.Message}" });
        }
    }

        // ... (MapToPredictionRequest, IsHighRisk, SendHighRiskNotificationAsync, SendConsolidatedRiskNotificationAsync como los tenías) ...

        private Dictionary<string, object> MapToPredictionRequest(PersonalDataModel personalData, HealthDataOutputModel latestDayData, int edad)
        {
            var latestBloodPressure = latestDayData.BloodPressureData?.LastOrDefault();
            var latestGlucose = latestDayData.BloodGlucoseData?.LastOrDefault();
            double.TryParse(personalData?.Choresterol?.ToString(), out double cholesterol);
            double.TryParse(personalData?.Height?.ToString(), out double height);
            double.TryParse(personalData?.Weight?.ToString(), out double weight);
            int gender = 1; // Valor por defecto (ej. masculino)
            int.TryParse(personalData?.Gender?.ToString(), out gender);
            double glucosaReal = latestGlucose?.BloodGlucose ?? 0;

            int glucosaCategoria;
            if (glucosaReal < 100)
                glucosaCategoria = 1;
            else if (glucosaReal < 126)
                glucosaCategoria = 2;
            else
                glucosaCategoria = 3;

            var predictionRequest = new Dictionary<string, object>
    {
        { "edad", edad },
        { "genero", gender },
        { "altura_cm", latestDayData?.Height ?? height },
        { "peso_kg", latestDayData?.Weight ?? weight },
        { "presion_sistolica", latestBloodPressure?.Systolic ?? 120 },
        { "presion_diastolica", latestBloodPressure?.Diastolic ?? 80 },
        { "colesterol", cholesterol },
        { "glucosa", glucosaCategoria }, 
        { "fuma", personalData?.Smoke ?? 0 },
        { "bebe_alcohol", personalData?.Alcohol ?? 0 },
        { "activo", personalData?.Active ?? true },
        { "enfermedad_cardiaca", personalData?.HasPredisposition ?? false }
    };

            return predictionRequest;
        }

        private bool IsHighRisk(dynamic predictionResult)
    {
        if (predictionResult is string resultString && !string.IsNullOrEmpty(resultString))
        {
            // Intentar extraer un número del inicio del string
            string numericPart = new string(resultString.TakeWhile(c => char.IsDigit(c) || c == '.' || c == ',').ToArray()).Replace(',', '.');

            if (float.TryParse(numericPart, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float result))
            {
                // Definir el umbral de riesgo alto
                const float highRiskThreshold = 60.0f;
                return result >= highRiskThreshold;
            }
        }
        return false; // No se pudo determinar el riesgo o no es alto
    }

    private async Task SendHighRiskNotificationAsync(string email, string predictionResult)
        {
            string truncatedResult = predictionResult.Length > 4 ? predictionResult.Substring(0, 4) : predictionResult;
            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("empresa.bluttruck@gmail.com", "dgpr ajeq uakc wdbf"),
                    EnableSsl = true
                })
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("empresa.bluttruck@gmail.com", "BlutTruck Health Team"),
                        Subject = "⚠️ Alerta: Alto Riesgo de Salud Detectado",
                        Body = $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            background-color: #f4f4f9;
            margin: 0;
            padding: 0;
        }}
        .email-container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            padding: 20px;
        }}
        .header {{
            background-color: #007bff;
            color: #ffffff;
            padding: 10px 20px;
            border-radius: 8px 8px 0 0;
            text-align: center;
            font-size: 24px;
            font-weight: bold;
        }}
        .content {{
            padding: 20px;
            color: #333333;
        }}
        .footer {{
            text-align: center;
            margin-top: 20px;
            font-size: 12px;
            color: #777777;
        }}
        .btn {{
            display: inline-block;
            margin-top: 10px;
            padding: 10px 20px;
            color: #ffffff;
            background-color: #28a745;
            text-decoration: none;
            border-radius: 4px;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            Alerta de Salud de BlutTruck
        </div>
        <div class='content'>
            <p>Estimado usuario,</p>
            <p>Los resultados de nuestro análisis indican un <strong>alto riesgo global</strong> en su salud.</p>
            <ul>
                <li><strong>Riesgo Detectado:</strong> {truncatedResult}</li>
            </ul>
            <p>Por favor, contacte a un médico lo antes posible.</p>
            <a href='mailto:support@bluttruck.com' class='btn'>Contactar Soporte</a>
        </div>
        <div class='footer'>
            Este correo es generado automáticamente por nuestro sistema. Si tiene preguntas, contáctenos en support@bluttruck.com.
            <br />
            © 2025 BlutTruck Health Services
        </div>
    </div>
</body>
</html>",
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(email);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar el correo electrónico: {ex.Message}");
            }
        }

        // Método para enviar un correo consolidado con las alertas específicas (parámetros fuera de rango)
        private async Task SendConsolidatedRiskNotificationAsync(string email, string riskDetails)
        {
            try
            {
                using (var smtpClient = new SmtpClient("smtp.gmail.com")
                {
                    Port = 587,
                    Credentials = new NetworkCredential("empresa.bluttruck@gmail.com", "dgpr ajeq uakc wdbf"),
                    EnableSsl = true
                })
                {
                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress("empresa.bluttruck@gmail.com", "BlutTruck Health Team"),
                        Subject = "⚠️ Alerta: Riesgo Específico Detectado",
                        Body = $@"
<html>
<head>
    <style>
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            background-color: #f4f4f9;
            margin: 0;
            padding: 0;
        }}
        .email-container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            padding: 20px;
        }}
        .header {{
            background-color: #007bff;
            color: #ffffff;
            padding: 10px 20px;
            border-radius: 8px 8px 0 0;
            text-align: center;
            font-size: 24px;
            font-weight: bold;
        }}
        .content {{
            padding: 20px;
            color: #333333;
        }}
        .footer {{
            text-align: center;
            margin-top: 20px;
            font-size: 12px;
            color: #777777;
        }}
        .btn {{
            display: inline-block;
            margin-top: 10px;
            padding: 10px 20px;
            color: #ffffff;
            background-color: #28a745;
            text-decoration: none;
            border-radius: 4px;
            font-weight: bold;
        }}
    </style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            Alerta de Salud de BlutTruck
        </div>
        <div class='content'>
            <p>Estimado usuario,</p>
            <p>Se han detectado las siguientes anomalías en sus parámetros de salud:</p>
            <p>{riskDetails}</p>
            <p>Le recomendamos contactar a un médico o a nuestro equipo de soporte para mayor información.</p>
            <a href='mailto:support@bluttruck.com' class='btn'>Contactar Soporte</a>
        </div>
        <div class='footer'>
            Este correo es generado automáticamente por nuestro sistema. Si tiene preguntas, contáctenos en support@bluttruck.com.
            <br />
            © 2025 BlutTruck Health Services
        </div>
    </div>
</body>
</html>",
                        IsBodyHtml = true
                    };
                    mailMessage.To.Add(email);

                    await smtpClient.SendMailAsync(mailMessage);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al enviar la alerta específica: {ex.Message}");
            }
        }
    }
}
