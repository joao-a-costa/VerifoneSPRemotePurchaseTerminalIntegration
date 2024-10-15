namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models
{
    public class Result
    {
        public bool Success { get; set; }
        public Enums.StatusCode StatusCode { get; set; } = Enums.StatusCode.Error;
        public string StatusCodeDescription { get; set; }
        public string Message { get; set; }
        public object ExtraData { get; set; }

        public Result()
        {
            StatusCodeDescription = Utilities.GetEnumDescription(Enums.StatusCode.Error);
        }
    }
}
