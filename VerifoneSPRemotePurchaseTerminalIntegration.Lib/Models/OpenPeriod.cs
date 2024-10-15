namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models
{
    internal class OpenPeriod
    {
        private const string _commandOpenPeriod = "070047533034030001";

        override public string ToString()
        {
           return $"{_commandOpenPeriod}";
        }
    }
}
