﻿using System;

namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models
{
    internal class Refund
    {
        private const string _commandRefund = "C00210#TRANSACTIONID##AMOUNT##ORIGINALPOSIDENTIFICATION##ORIGINALRECEIPTDATA##ORIGINALRECEIPTTIME##PRINTRECEIPTONPOS#0";

        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string OriginalPosIdentification { get; set; }
        public DateTime OriginalReceiptData { get; set; }
        public DateTime OriginalReceiptTime { get; set; }
        public bool PrintReceiptOnPOS { get; set; } = false;

        override public string ToString()
        {
            return _commandRefund
                .Replace("#TRANSACTIONID#", TransactionId.PadLeft(4, '0'))
                .Replace("#AMOUNT#", Amount.PadLeft(8, '0'))
                .Replace("#ORIGINALPOSIDENTIFICATION#", OriginalPosIdentification?.PadLeft(8, '0'))
                .Replace("#ORIGINALRECEIPTDATA#",
                    OriginalReceiptData.Year.ToString().PadLeft(4, '0') +
                    OriginalReceiptData.Month.ToString().PadLeft(2, '0') +
                    OriginalReceiptData.Day.ToString().PadLeft(2, '0'))
                .Replace("#ORIGINALRECEIPTTIME#",
                    OriginalReceiptTime.Hour.ToString().PadLeft(2, '0') +
                    OriginalReceiptTime.Minute.ToString().PadLeft(2, '0') +
                    OriginalReceiptTime.Second.ToString().PadLeft(2, '0'))
                .Replace("#PRINTRECEIPTONPOS#", Convert.ToByte(PrintReceiptOnPOS).ToString());
        }
    }
}
