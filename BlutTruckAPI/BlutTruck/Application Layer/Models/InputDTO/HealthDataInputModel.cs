namespace BlutTruck.Application_Layer.Models
{
    public class HealthDataInputModel
    {
        public string? UserId { get; set; }
        public int? Steps { get; set; }
        public double? ActiveCalories { get; set; }

        public List<int?>? HeartRates { get; set; } = new List<int?>();

        public List<HeartRateDataPoint>? HeartRateData { get; set; } = new List<HeartRateDataPoint>();


        public double? AvgHeartRate
        {
            get
            {
                // Usar hrdp.BPM (o el nombre correcto de la propiedad en HeartRateDataPoint)
                var validBpmValues = HeartRateData?
                    .Where(hrdp => hrdp != null) // Filtra HeartRateDataPoint nulos (si es posible que existan)
                    .Select(hrdp => hrdp.BPM);    // Asume que 'BPM' es int

                // Average sobre una colección de int devuelve double.
                return validBpmValues?.Any() == true ? validBpmValues.Average() : null;
            }
        }

        // Cambiado a double? para coincidir con el tipo de dato en Firebase JSON
        public double? MinHeartRate
        {
            get
            {
                var validBpmValues = HeartRateData?
                    .Where(hrdp => hrdp != null)
                    .Select(hrdp => hrdp.BPM);

                // Min() sobre una colección de int devuelve int. Hacemos cast a double?.
                return validBpmValues?.Any() == true ? (double?)validBpmValues.Min() : null;
            }
        }

        // Cambiado a double? para coincidir con el tipo de dato en Firebase JSON
        public double? MaxHeartRate
        {
            get
            {
                var validBpmValues = HeartRateData?
                    .Where(hrdp => hrdp != null)
                    .Select(hrdp => hrdp.BPM);

                // Max() sobre una colección de int devuelve int. Hacemos cast a double?.
                return validBpmValues?.Any() == true ? (double?)validBpmValues.Max() : null;
            }
        }


        public double? RestingHeartRate { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }

        public List<BloodPressureDataPoint>? BloodPressureData { get; set; } = new List<BloodPressureDataPoint>();

        public List<OxygenSaturationDataPoint>? OxygenSaturationData { get; set; } = new List<OxygenSaturationDataPoint>();

        public List<BloodGlucoseDataPoint>? BloodGlucoseData { get; set; } = new List<BloodGlucoseDataPoint>();

        public double? BodyTemperature { get; set; }
        public List<TemperatureDataPoint>? TemperatureData { get; set; } = new List<TemperatureDataPoint>();

        public List<RespiratoryRateDataPoint>? RespiratoryRateData { get; set; } = new List<RespiratoryRateDataPoint>();

        public List<SleepSessionDataPoint>? SleepData { get; set; } = new List<SleepSessionDataPoint>();
    }


public class HeartRateDataPoint
    {
        public DateTime Time { get; set; }
        public int BPM { get; set; }
    }

    public class TemperatureDataPoint
    {
        public DateTime Time { get; set; }
        public double Temperature { get; set; }
    }

    public class BloodPressureDataPoint
    {
        public DateTime Time { get; set; }
        public double Systolic { get; set; }
        public double Diastolic { get; set; }
    }

    public class OxygenSaturationDataPoint
    {
        public DateTime Time { get; set; }
        public double Percentage { get; set; }
    }

    public class BloodGlucoseDataPoint
    {
        public DateTime Time { get; set; }
        public double BloodGlucose { get; set; }
    }

    public class RespiratoryRateDataPoint
    {
        public DateTime Time { get; set; }
        public double Rate { get; set; }
    }

    public class SleepSessionDataPoint
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<SleepStageDataPoint>? Stages { get; set; }
    }

    public class SleepStageDataPoint
    {
        public string Type { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
