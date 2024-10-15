namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models
{
    internal class OpenPeriod
    {
        //private const string _commandOpenPeriod = "S00010#TRANSACTIONID#010";
        private const string _commandOpenPeriod = "070047533034030001";

        //public string TransactionId { get; set; }

        override public string ToString()
        {
           return $"{_commandOpenPeriod}";
        }
    }
}
