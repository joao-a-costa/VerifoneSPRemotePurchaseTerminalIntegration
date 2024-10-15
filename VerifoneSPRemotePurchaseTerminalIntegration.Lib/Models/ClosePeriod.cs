namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models
{
    internal class ClosePeriod
    {
        private const string _commandClosePeriod = "08004753303505010102";

        override public string ToString()
        {
            return $"{_commandClosePeriod}";
        }
    }
}
