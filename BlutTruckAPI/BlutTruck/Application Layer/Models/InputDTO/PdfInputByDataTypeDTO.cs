using BlutTruck.Application_Layer.Enums;

namespace BlutTruck.Application_Layer.Models.InputDTO
{
    public class PdfInputByDataTypeDTO
    {
        public UserCredentials Credentials { get; set; }
        public HealthDataType DataType { get; set; }
    }
}
