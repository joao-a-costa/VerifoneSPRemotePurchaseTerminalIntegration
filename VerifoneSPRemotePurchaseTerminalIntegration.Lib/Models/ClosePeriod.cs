using System;

namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models
{
    internal class ClosePeriod
    {
        private const string _commandClosePeriod = "080047533035050101#PRINTRECEIPTONPOS#";

        public bool UseSupervisorCard { get; set; } = false;
        public bool PrintReceiptOnPOS { get; set; } = false;

        override public string ToString()
        {
            string command = _commandClosePeriod
                .Replace("#USESUPERVISORCARD#", Convert.ToByte(!UseSupervisorCard).ToString())
                .Replace("#PRINTRECEIPTONPOS#", (Convert.ToByte(PrintReceiptOnPOS) + 1).ToString().PadLeft(2, '0').Replace(" ", string.Empty));

            return command;
        }
    }
}
