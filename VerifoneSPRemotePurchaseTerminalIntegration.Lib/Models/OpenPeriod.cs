using System;

namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models
{
    internal class OpenPeriod
    {
        private const string _commandOpenPeriod = "0700475330340300#PRINTRECEIPTONPOS#";

        public bool UseSupervisorCard { get; set; } = false;
        public bool PrintReceiptOnPOS { get; set; } = false;

        override public string ToString()
        {
            string command = _commandOpenPeriod
                .Replace("#USESUPERVISORCARD#", Convert.ToByte(!UseSupervisorCard).ToString())
                .Replace("#PRINTRECEIPTONPOS#", (Convert.ToByte(PrintReceiptOnPOS) + 1).ToString().PadLeft(2, '0').Replace(" ", string.Empty));

            return command;
        }
    }
}