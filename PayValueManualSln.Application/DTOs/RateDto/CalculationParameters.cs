namespace PayValueManualSln.Application.DTOs.RateDto
{
    public class CalculationParameters
    {
        public long? RateId { get; set; }
        public decimal? Value { get; set; }
        public string RateAmount { get; set; }
        public long? ZoneId { get; set; }
        public long? LocationId { get; set; }
        public long? ServiceId { get; set; }
        public double? LandSize { get; set; }
        public long? TypeId { get; set; }
        public long? ServiceMethodId { get; set; }
        public long? InputDefinitionId { get; set; }
        public decimal? AmountForTemplateValue { get; set; }
        public long? Pages { get; set; }
        public bool? IsServiceForAllLocation { get; set; }
        public decimal? CalculatedRateAmount { get; set; } //this is for use in rateservice to be used for calculation
    }
}
