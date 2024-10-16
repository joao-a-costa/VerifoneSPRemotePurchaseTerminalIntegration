using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models;
using static VerifoneSPRemotePurchaseTerminalIntegration.Lib.Enums;

namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib
{
    public class VerifoneSPRemote
    {
        #region "Constants"

        private const string _okOpenPeriod = "00";
        private const string _dateTimeFormatOnPOS = "yyyyMMdd HHmmss";

        #endregion

        #region "Members"

        private readonly string serverIp;
        private readonly int port;

        #endregion

        #region "Properties"

        public string OriginalPosIdentification { get; }

        #endregion

        #region "Events"

        /// <summary>
        /// Define an event to be raised when a message is sent
        /// </summary>
        public event EventHandler<string> MessageSent;

        #endregion

        #region "Constructors"

        public VerifoneSPRemote(string serverIp, int port)
        {
            this.serverIp = serverIp;
            this.port = port;
        }

        #endregion

        /// <summary>
        /// Sends the command to the server.
        /// </summary>
        /// <param name="command">The command to send.</param>
        private string SendCommand(string command, string tags = "")
        {
            var message = string.Empty;

            MessageSent?.Invoke(this, command);

            using (var client = new TcpClient(serverIp, port))
            {
                using (var stream = client.GetStream())
                {
                    byte[] hexCommand = null;

                    if (string.IsNullOrEmpty(tags))
                        hexCommand = Utilities.ConvertHexStringToByteArray(command);
                    else
                        hexCommand = Utilities.ConvertHexStringToByteArray(string.Concat(Utilities.CalculateHexLength(command).Select(b => b.ToString("D2"))));
                    stream.Write(hexCommand, 0, hexCommand.Length);

                    var stringCommand = Encoding.ASCII.GetBytes(command);
                    stream.Write(stringCommand, 0, stringCommand.Length);

                    if (!string.IsNullOrEmpty(tags))
                    {
                        var hexCommandLast = Utilities.ConvertHexStringToByteArray(tags);
                        stream.Write(hexCommandLast, 0, hexCommandLast.Length);
                    }

                    // Receive the response
                    byte[] response = new byte[256];
                    int bytesRead = stream.Read(response, 0, response.Length);

                    // Process the response
                    message = Encoding.ASCII.GetString(response, 0, bytesRead);
                    Console.WriteLine("Response from terminal: " + message);

                    stream.Close();
                }

                client.Close();
            }

            return message;
        }

        /// <summary>
        /// Terminal status.
        /// </summary>
        public Result TerminalStatus()
        {
            using (var client = new TcpClient(serverIp, port))
            {
                using (var stream = client.GetStream())
                {
                    stream.Close();
                }

                client.Close();
            }

            return new Result { Success = true };
        }

        /// <summary>
        /// Opens the period.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        public Result OpenPeriod()
        {
            var message = SendCommand(new OpenPeriod().ToString());
            SendCommand(new IdleState().ToString());
            var messageStatusCode = StatusCode.Error;

            // Extract the status code part from the message and convert it to an integer from hex
            var statusCodeHex = message.Substring(2, 2);
            if (int.TryParse(statusCodeHex, NumberStyles.HexNumber, null, out int statusCodeInt) && Enum.IsDefined(typeof(StatusCode), statusCodeInt))
                messageStatusCode = (StatusCode)statusCodeInt;

            return new Result
            {
                Success = message.Substring(2, 2).StartsWith(_okOpenPeriod),
                Message = message,
                StatusCode = messageStatusCode,
                StatusCodeDescription = Utilities.GetEnumDescription(messageStatusCode)
            };
        }


        /// <summary>
        /// Closes the period.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        public Result ClosePeriod()
        {
            var message = SendCommand(new ClosePeriod().ToString());
            SendCommand(new IdleState().ToString());
            var messageStatusCode = StatusCode.Error;

            // Extract the status code part from the message and convert it to an integer from hex
            var statusCodeHex = message.Substring(2, 2);
            if (int.TryParse(statusCodeHex, NumberStyles.HexNumber, null, out int statusCodeInt) && Enum.IsDefined(typeof(StatusCode), statusCodeInt))
                messageStatusCode = (StatusCode)statusCodeInt;

            return new Result
            {
                Success = message.Substring(2, 2).StartsWith(_okOpenPeriod),
                Message = message,
                StatusCode = messageStatusCode,
                StatusCodeDescription = Utilities.GetEnumDescription(messageStatusCode)
            };
        }

        /// <summary>
        /// Purchases the specified transaction identifier and amount.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="amount">The amount.</param>
        /// <param name="originalPosIdentification">The original POS identification.</param>
        /// <param name="printReceiptOnPOS">if set to <c>true</c> [print receipt on POS].</param>
        /// <param name="originalReceiptData">The original receipt data.</param>
        public Result Purchase(string transactionId, string amount, bool printReceiptOnPOS = true)
        {
            var purchaseResult = new PurchaseResult();
            var message  = SendCommand(new Purchase {
                TransactionId = transactionId,
                Amount = amount,
                PrintReceiptOnPOS = printReceiptOnPOS }.ToString(),
                string.Empty);
            SendCommand(new IdleState().ToString());
            var messageStatusCode = StatusCode.Error;

            // Extract the status code part from the message and convert it to an integer from hex
            var statusCodeHex = message.Substring(2, 2);
            if (int.TryParse(statusCodeHex, NumberStyles.HexNumber, null, out int statusCodeInt) && Enum.IsDefined(typeof(StatusCode), statusCodeInt))
                messageStatusCode = (StatusCode)statusCodeInt;

            if (messageStatusCode == StatusCode.OKCommand)
            {
                purchaseResult.TransactionId = transactionId;
                purchaseResult.Amount = amount;

                DateTime.TryParseExact(
                    $"{message.Substring(18, 8)} {message.Substring(107, 6)}",
                    _dateTimeFormatOnPOS,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime receiptDataParsed
                );

                var receiptPosIdentification = message.Substring(26, 8);
                var receiptData = message;

                purchaseResult.OriginalPosIdentification = receiptPosIdentification;
                purchaseResult.OriginalReceiptData = receiptDataParsed;
                purchaseResult.ReceiptData = receiptData;
            }

            return new Result
            {
                Success = message.Substring(2, 2).StartsWith(_okOpenPeriod),
                Message = message,
                StatusCode = messageStatusCode,
                StatusCodeDescription = Utilities.GetEnumDescription(messageStatusCode),
                ExtraData = purchaseResult
            };
        }

        /// <summary>
        /// The refund.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        /// <param name="amount">The amount.</param>
        public Result Refund(PurchaseResult purchaseResult, bool printReceiptOnPOS = true)
        {
            var message = SendCommand(new Refund {
                TransactionId = purchaseResult.TransactionId,
                Amount = purchaseResult.Amount,
                OriginalPosIdentification = purchaseResult.OriginalPosIdentification,
                OriginalReceiptData = purchaseResult.OriginalReceiptData,
                OriginalReceiptTime = purchaseResult.OriginalReceiptData,
                PrintReceiptOnPOS = printReceiptOnPOS
            }.ToString());
            SendCommand(new IdleState().ToString());
            var messageStatusCode = StatusCode.Error;

            // Extract the status code part from the message and convert it to an integer from hex
            var statusCodeHex = message.Substring(2, 2);
            if (int.TryParse(statusCodeHex, NumberStyles.HexNumber, null, out int statusCodeInt) && Enum.IsDefined(typeof(StatusCode), statusCodeInt))
                messageStatusCode = (StatusCode)statusCodeInt;

            return new Result
            {
                Success = message.Substring(2, 2).StartsWith(_okOpenPeriod),
                Message = message,
                StatusCode = messageStatusCode,
                StatusCodeDescription = Utilities.GetEnumDescription(messageStatusCode)
            };
        }
    }
}