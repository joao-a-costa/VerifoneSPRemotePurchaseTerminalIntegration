using System;

namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models
{
    internal class Purchase
    {
        private const string _commandPurchase = "1D004753303610#TRANSACTIONID##PRINTRECEIPTONPOS#01#AMOUNT#30303000";

        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public bool PrintReceiptOnPOS { get; set; } = false;

        override public string ToString()
        {
            string command = _commandPurchase
                .Replace("#TRANSACTIONID#", Utilities.ConvertToHexString(TransactionId.PadLeft(10, '0')).Replace(" ", string.Empty))
                .Replace("#AMOUNT#", Utilities.ConvertToHexString(Amount.PadLeft(8, '0')).Replace(" ", string.Empty))
                .Replace("#PRINTRECEIPTONPOS#", (Convert.ToByte(PrintReceiptOnPOS) + 1).ToString().PadLeft(2, '0').Replace(" ", string.Empty));

            return command;
        }
    }
}
