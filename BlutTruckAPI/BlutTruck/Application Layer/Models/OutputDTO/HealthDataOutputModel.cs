namespace BlutTruck.Application_Layer.Models
{
    public class HealthDataOutputModel
    {
        public required string UserId { get; set; }
        public int? Steps { get; set; }
        public double? ActiveCalories { get; set; }

        public List<int?> HeartRates { get; set; } = new List<int?>();
        public List<HeartRateDataPoint> HeartRateData { get; set; } = new List<HeartRateDataPoint>();

        public double? AvgHeartRate
        {
            get
            {
                var validBpmValues = HeartRateData?
                    .Where(hrdp => hrdp != null) // Filtra HeartRateDataPoint nulos (si es posible)
                    .Select(hrdp => hrdp.BPM);    // Asume que 'BPM' en HeartRateDataPoint es int

                // .Average() sobre una colección de int devuelve double, así que está bien.
                return validBpmValues?.Any() == true ? validBpmValues.Average() : null;
            }
        }

        // CORREGIDO: Cambiado de int? a double?
        public double? MinHeartRate
        {
            get
            {
                var validBpmValues = HeartRateData?
                    .Where(hrdp => hrdp != null)
                    .Select(hrdp => hrdp.BPM);

                // .Min() sobre una colección de int devuelve int. Se hace cast a double?.
                return validBpmValues?.Any() == true ? (double?)validBpmValues.Min() : null;
            }
        }

        // CORREGIDO: Cambiado de int? a double?
        public double? MaxHeartRate
        {
            get
            {
                var validBpmValues = HeartRateData?
                    .Where(hrdp => hrdp != null)
                    .Select(hrdp => hrdp.BPM);

                // .Max() sobre una colección de int devuelve int. Se hace cast a double?.
                return validBpmValues?.Any() == true ? (double?)validBpmValues.Max() : null;
            }
        }


        public double? RestingHeartRate { get; set; }
        public double? Weight { get; set; }
        public double? Height { get; set; }

        public List<BloodPressureDataPoint> BloodPressureData { get; set; } = new List<BloodPressureDataPoint>();
        public List<OxygenSaturationDataPoint> OxygenSaturationData { get; set; } = new List<OxygenSaturationDataPoint>();
        public List<BloodGlucoseDataPoint> BloodGlucoseData { get; set; } = new List<BloodGlucoseDataPoint>();

        public double? BodyTemperature { get; set; }
        public List<TemperatureDataPoint> TemperatureData { get; set; } = new List<TemperatureDataPoint>();

        public List<RespiratoryRateDataPoint> RespiratoryRateData { get; set; } = new List<RespiratoryRateDataPoint>();
        public List<SleepSessionDataPoint> SleepData { get; set; } = new List<SleepSessionDataPoint>();
    }
}
