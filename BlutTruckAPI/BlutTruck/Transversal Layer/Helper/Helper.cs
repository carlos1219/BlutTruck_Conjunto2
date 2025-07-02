namespace BlutTruck.Transversal_Layer.Helper
{
    using BlutTruck.Transversal_Layer.IHelper;
    using BlutTruck.Application_Layer.Models;
    using System;
        using System.Collections.Generic;

        public class Helper : IHelper
        {
            public string GetCurrentDateKey()
            {
                return DateTime.Now.ToString("yyyy-MM-dd");
            }

        public Dictionary<string, object> FormatHealthData(HealthDataInputModel data)
        {
            var healthDataDict = new Dictionary<string, object>
    {
        { "UserId", data.UserId }
    };

            if (data.Steps.HasValue)
                healthDataDict["steps"] = data.Steps.Value;
            if (data.ActiveCalories.HasValue)
                healthDataDict["activeCalories"] = data.ActiveCalories.Value;
            if (data.AvgHeartRate.HasValue)
                healthDataDict["avgHeartRate"] = data.AvgHeartRate.Value;
            if (data.MinHeartRate.HasValue)
                healthDataDict["minHeartRate"] = data.MinHeartRate.Value;
            if (data.MaxHeartRate.HasValue)
                healthDataDict["maxHeartRate"] = data.MaxHeartRate.Value;
            if (data.RestingHeartRate.HasValue)
                healthDataDict["restingHeartRate"] = data.RestingHeartRate.Value;
            if (data.Weight.HasValue)
                healthDataDict["weight"] = data.Weight.Value;
            if (data.Height.HasValue)
                healthDataDict["height"] = data.Height.Value;
            if (data.BloodPressureData?.Any() == true)
            {
                healthDataDict["bloodPressureSystolic"] = data.BloodPressureData.First().Systolic;
                healthDataDict["bloodPressureDiastolic"] = data.BloodPressureData.First().Diastolic;
            }

            if (data.OxygenSaturationData?.Any() == true)
            {
                healthDataDict["oxygenSaturation"] = data.OxygenSaturationData.First().Percentage;
            }

            if (data.BloodGlucoseData?.Any() == true)
            {
                healthDataDict["bloodGlucose"] = data.BloodGlucoseData.First().BloodGlucose;
            }

            if (data.TemperatureData?.Any() == true)
            {
                healthDataDict["bodyTemperature"] = data.TemperatureData.First().Temperature;
            }

            if (data.RespiratoryRateData?.Any() == true)
            {
                healthDataDict["respiratoryRate"] = data.RespiratoryRateData.First().Rate;
            }

            if (data.HeartRates != null && data.HeartRates.Any())
                healthDataDict["heartRates"] = data.HeartRates;
            if (data.HeartRateData != null && data.HeartRateData.Any())
                healthDataDict["heartRateData"] = data.HeartRateData;
            if (data.TemperatureData != null && data.TemperatureData.Any())
                healthDataDict["temperatureData"] = data.TemperatureData;
            if (data.BloodPressureData != null && data.BloodPressureData.Any())
                healthDataDict["bloodPressureData"] = data.BloodPressureData;
            if (data.OxygenSaturationData != null && data.OxygenSaturationData.Any())
                healthDataDict["oxygenSaturationData"] = data.OxygenSaturationData;
            if (data.BloodGlucoseData != null && data.BloodGlucoseData.Any())
                healthDataDict["bloodGlucoseData"] = data.BloodGlucoseData;
            if (data.RespiratoryRateData != null && data.RespiratoryRateData.Any())
                healthDataDict["respiratoryRateData"] = data.RespiratoryRateData;
            if (data.SleepData != null && data.SleepData.Any())
                healthDataDict["sleepData"] = data.SleepData;

            return healthDataDict;
        }

    }
}
