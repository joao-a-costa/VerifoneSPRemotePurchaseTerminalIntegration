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

        private const string _okMessage = "00";
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

        #region "Private Methods"

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
                    byte[] response = new byte[20148];
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
        /// Calculates the length of the receipt.
        /// </summary>
        /// <param name="binaryPart">The binary part of the receipt.</param>
        /// <returns>The length of the receipt.</returns>
        private static int CalculateReceiptLenght(string binaryPart)
        {
            // Step 2: Convert each character to its ASCII hex representation
            byte[] asciiBytes = Encoding.ASCII.GetBytes(binaryPart);
            string asciiHex = BitConverter.ToString(asciiBytes).Replace("-", ""); // Get hex as a string
            Console.WriteLine($"ASCII Hex: {asciiHex}"); // Output will be "0178" instead of "7801"

            // Step 3: Swap endianness (change byte order)
            //string swappedHex = asciiHex.Substring(2, 2) + asciiHex.Substring(0, 2); // Swap bytes: "0178" -> "7801"
            string swappedHex = asciiHex;
            Console.WriteLine($"Swapped Endianness: {swappedHex}"); // Output: "0178"

            // Step 4: Convert to integer
            int receiptLength = Convert.ToInt32(swappedHex, 16); // Convert from hex to integer
            Console.WriteLine($"Receipt Length: {receiptLength} bytes"); // Output: 376

            return receiptLength;
        }

        #endregion

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
            var purchaseResult = new PurchaseResult();
            var message = SendCommand(new OpenPeriod().ToString());
            SendCommand(new IdleState().ToString());
            var messageStatusCode = StatusCode.Error;

            // Extract the status code part from the message and convert it to an integer from hex
            var statusCodeHex = message.Substring(2, 2);
            if (int.TryParse(statusCodeHex, NumberStyles.HexNumber, null, out int statusCodeInt) && Enum.IsDefined(typeof(StatusCode), statusCodeInt))
                messageStatusCode = (StatusCode)statusCodeInt;

            if (messageStatusCode == StatusCode.OKCommand)
            {
                //purchaseResult.TransactionId = transactionId;
                //purchaseResult.Amount = amount;

                //DateTime.TryParseExact(
                //    $"{message.Substring(18, 8)} {message.Substring(107, 6)}",
                //    _dateTimeFormatOnPOS,
                //    CultureInfo.InvariantCulture,
                //    DateTimeStyles.None,
                //    out DateTime receiptDataParsed
                //);

                //var receiptPosIdentification = message.Substring(26, 8);

                var merchantReceiptBinaryPart = message.Substring(21).Substring(0, 2);
                var merchantReceiptLenght = CalculateReceiptLenght(merchantReceiptBinaryPart);
                var merchantReceipt = Encoding.UTF8.GetString(Convert.FromBase64String(message.Substring(23, merchantReceiptLenght)));

                //purchaseResult.OriginalPosIdentification = receiptPosIdentification;
                //purchaseResult.OriginalReceiptData = receiptDataParsed;
                purchaseResult.ReceiptData = new PurchaseResultReceipt
                {
                    MerchantCopy = merchantReceipt,
                    //ClientCopy = clientReceipt
                };
            }

            return new Result
            {
                Success = message.Substring(2, 2).StartsWith(_okMessage),
                Message = message,
                StatusCode = messageStatusCode,
                StatusCodeDescription = Utilities.GetEnumDescription(messageStatusCode),
                ExtraData = purchaseResult
            };
        }

        /// <summary>
        /// Closes the period.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        public Result ClosePeriod()
        {
            var purchaseResult = new PurchaseResult();
            var message = SendCommand(new ClosePeriod().ToString());
            SendCommand(new IdleState().ToString());
            var messageStatusCode = StatusCode.Error;

            // Extract the status code part from the message and convert it to an integer from hex
            var statusCodeHex = message.Substring(2, 2);
            if (int.TryParse(statusCodeHex, NumberStyles.HexNumber, null, out int statusCodeInt) && Enum.IsDefined(typeof(StatusCode), statusCodeInt))
                messageStatusCode = (StatusCode)statusCodeInt;

            if (messageStatusCode == StatusCode.OKCommand)
            {
                //purchaseResult.TransactionId = transactionId;
                //purchaseResult.Amount = amount;

                //DateTime.TryParseExact(
                //    $"{message.Substring(18, 8)} {message.Substring(107, 6)}",
                //    _dateTimeFormatOnPOS,
                //    CultureInfo.InvariantCulture,
                //    DateTimeStyles.None,
                //    out DateTime receiptDataParsed
                //);

                //var receiptPosIdentification = message.Substring(26, 8);

                var merchantReceiptBinaryPart = message.Substring(71).Substring(0, 2);
                var merchantReceiptLenght = CalculateReceiptLenght(merchantReceiptBinaryPart);
                var merchantReceipt = Encoding.UTF8.GetString(Convert.FromBase64String(message.Substring(73, merchantReceiptLenght)));

                //purchaseResult.OriginalPosIdentification = receiptPosIdentification;
                //purchaseResult.OriginalReceiptData = receiptDataParsed;
                purchaseResult.ReceiptData = new PurchaseResultReceipt
                {
                    MerchantCopy = merchantReceipt,
                    //ClientCopy = clientReceipt
                };
            }

            return new Result
            {
                Success = message.Substring(2, 2).StartsWith(_okMessage),
                Message = message,
                StatusCode = messageStatusCode,
                StatusCodeDescription = Utilities.GetEnumDescription(messageStatusCode),
                ExtraData = purchaseResult
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

                var merchantReceiptBinaryPart = message.Substring(195).Substring(0, 2);
                var merchantReceiptLenght = CalculateReceiptLenght(merchantReceiptBinaryPart);
                var merchantReceipt = Encoding.UTF8.GetString(Convert.FromBase64String(message.Substring(198, merchantReceiptLenght)));

                var clientReceiptBinaryPart = message.Substring(198 + merchantReceiptLenght).Substring(0, 2);
                //var clientReceiptLenght = CalculateReceiptLenght(clientReceiptBinaryPart);
                var clientReceipt = Encoding.UTF8.GetString(Convert.FromBase64String(message.Substring(198 + merchantReceiptLenght + 2)));

                purchaseResult.OriginalPosIdentification = receiptPosIdentification;
                purchaseResult.OriginalReceiptData = receiptDataParsed;
                purchaseResult.ReceiptData = new PurchaseResultReceipt
                {
                    MerchantCopy = merchantReceipt,
                    ClientCopy = clientReceipt
                };
            }

            return new Result
            {
                Success = message.Substring(2, 2).StartsWith(_okMessage),
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

            if (messageStatusCode == StatusCode.OKCommand)
            {
                //purchaseResult.TransactionId = transactionId;
                //purchaseResult.Amount = amount;

                DateTime.TryParseExact(
                    $"{message.Substring(18, 8)} {message.Substring(107, 6)}",
                    _dateTimeFormatOnPOS,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime receiptDataParsed
                );

                var receiptPosIdentification = message.Substring(26, 8);

                var merchantReceiptBinaryPart = message.Substring(195).Substring(0, 2);
                var merchantReceiptLenght = CalculateReceiptLenght(merchantReceiptBinaryPart);
                var merchantReceipt = Encoding.UTF8.GetString(Convert.FromBase64String(message.Substring(198, merchantReceiptLenght)));

                var clientReceiptBinaryPart = message.Substring(198 + merchantReceiptLenght).Substring(0, 2);
                //var clientReceiptLenght = CalculateReceiptLenght(clientReceiptBinaryPart);
                var clientReceipt = Encoding.UTF8.GetString(Convert.FromBase64String(message.Substring(198 + merchantReceiptLenght + 2)));

                purchaseResult.OriginalPosIdentification = receiptPosIdentification;
                purchaseResult.OriginalReceiptData = receiptDataParsed;
                purchaseResult.ReceiptData = new PurchaseResultReceipt
                {
                    MerchantCopy = merchantReceipt,
                    ClientCopy = clientReceipt
                };
            }

            return new Result
            {
                Success = message.Substring(2, 2).StartsWith(_okMessage),
                Message = message,
                StatusCode = messageStatusCode,
                StatusCodeDescription = Utilities.GetEnumDescription(messageStatusCode),
                ExtraData = purchaseResult
            };
        }
    }
}