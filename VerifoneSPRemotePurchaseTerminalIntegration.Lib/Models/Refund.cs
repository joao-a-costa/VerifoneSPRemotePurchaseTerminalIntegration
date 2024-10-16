using System;

namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models
{
    internal class Refund
    {
        private const string _commandRefund = "2E004753303702#TRANSACTIONID#01#OriginalPosIdentification##OriginalReceiptData##OriginalReceiptTime##AMOUNT#";

        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string OriginalPosIdentification { get; set; }
        public DateTime OriginalReceiptData { get; set; }
        public DateTime OriginalReceiptTime { get; set; }
        public bool PrintReceiptOnPOS { get; set; } = false;

        override public string ToString()
        {
            return _commandRefund
                .Replace("#TRANSACTIONID#", Utilities.ConvertToHexString(TransactionId.PadLeft(10, '0')).Replace(" ", string.Empty))
                .Replace("#OriginalPosIdentification#", Utilities.ConvertToHexString(OriginalPosIdentification.PadLeft(8, '0')).Replace(" ", string.Empty))
                .Replace("#OriginalReceiptData#", Utilities.ConvertToHexString(OriginalReceiptData.ToString("yyyyMMdd")).Replace(" ", string.Empty))
                .Replace("#OriginalReceiptTime#", Utilities.ConvertToHexString(OriginalReceiptTime.ToString("HHmmss")).Replace(" ", string.Empty))
                .Replace("#AMOUNT#", Utilities.ConvertToHexString(Amount.PadLeft(8, '0')).Replace(" ", string.Empty))
                .Replace("#PRINTRECEIPTONPOS#", Convert.ToByte(PrintReceiptOnPOS).ToString().PadLeft(2, '0'))
            ;
        }
    }
}
