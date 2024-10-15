﻿using System;
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

        private const string _infoReceived = "Received";
        private const string _infoUnknownError = "Erro no processamento. Consulte o terminal para mais detalhes";

        private const string _okTerminalStatus = "INIT OK";
        private const string _okOpenPeriod = "00";
        private const string _okClosePeriod = "PERÍODO FECHADO";
        private const string _okPurchase = "000";
        private const string _okRefund = "DEVOL. EFECTUADA";

        private const string _patternReceiptOnECRTerminalIDAndDate = @"Ident\. TPA:\s*(\d+)\s*(\d{2}-\d{2}-\d{2})\s*(\d{2}:\d{2}:\d{2})";
        private const string _dateTimeFormatOnECR = "yy-MM-dd HH:mm:ss";

        private const string _patternReceiptOnPOSTerminalID = @"[\u001c\b](\d{8})";
        private const string _patternReceiptOnPOSDate = @"(\d{8})";
        private const string _patternReceiptOnPOSTime = @"(\d{6})";
        private const string _dateTimeFormatOnPOS = "yyyyMMdd HHmmss";

        private const string _purchaseTags = "0B9F1C009A009F21009F4100";


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
        public string SendCommand(string command, string tags = "")
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
            }

            return message;
        }

        /// <summary>
        /// Terminal status.
        /// </summary>
        public Result TerminalStatus()
        {
            var message = SendCommand(new TerminalStatus().ToString());
            var success = message.Substring(9).StartsWith(_okTerminalStatus);
            var originalPosIdentification = success ? message.Substring(26) : string.Empty;

            return new Result { Success = success, Message = message, ExtraData = originalPosIdentification };
        }

        /// <summary>
        /// Opens the period.
        /// </summary>
        /// <param name="transactionId">The transaction identifier.</param>
        public Result OpenPeriod(string transactionId)
        {
            var message = SendCommand(new OpenPeriod().ToString());
            var messageIdle = SendCommand(new IdleState().ToString());
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
        public Result ClosePeriod(string transactionId)
        {
            var message = SendCommand(new ClosePeriod().ToString());
            var messageIdle = SendCommand(new IdleState().ToString());
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
                _purchaseTags);
            var success = message.Substring(6, 3).Equals(_okPurchase);

            var messageIdle = SendCommand(new IdleState().ToString());

            if (success)
            {
                var receiptPosIdentification = string.Empty;
                var receiptDataParsed = DateTime.Now;
                var receiptData = string.Empty;

                purchaseResult.TransactionId = transactionId;
                purchaseResult.Amount = amount;

                if (!printReceiptOnPOS)
                {
                    // Match Ident. TPA for terminal ID, date, and time:
                    var matchIdentTpa = Regex.Match(message, _patternReceiptOnECRTerminalIDAndDate);
                    if (matchIdentTpa.Success)
                    {
                        DateTime.TryParseExact(
                            matchIdentTpa.Groups[2].Value + " " + matchIdentTpa.Groups[3].Value,
                            _dateTimeFormatOnECR,
                            CultureInfo.InvariantCulture,
                            DateTimeStyles.None,
                            out receiptDataParsed
                        );

                        receiptPosIdentification = matchIdentTpa.Groups[1].Value;
                        receiptData = message.Substring(29);
                    }
                }
                else
                {
                    // Define regex patterns for terminal ID, date, and time
                    string terminalIdPattern = _patternReceiptOnPOSTerminalID;  // Matches 8 digits after a specific control character
                    string datePattern = _patternReceiptOnPOSDate;  // Matches 8 digits (YYYYMMDD) for date
                    string timePattern = _patternReceiptOnPOSTime;  // Matches 6 digits (HHMMSS) for time

                    // Find matches for terminal ID, date, and time
                    Match terminalIdMatch = Regex.Match(message, terminalIdPattern);

                    if (terminalIdMatch.Success)
                    {
                        receiptPosIdentification = terminalIdMatch.Groups[1].Value;

                        Match dateMatch = Regex.Match(message.Substring(terminalIdMatch.Index + terminalIdMatch.Length), datePattern);

                        if (dateMatch.Success)
                        {
                            Match timeMatch = Regex.Match(message.Substring(terminalIdMatch.Index + terminalIdMatch.Length + dateMatch.Index + dateMatch.Length), timePattern);

                            if (timeMatch.Success)
                            {
                                DateTime.TryParseExact(
                                    dateMatch.Groups[1].Value + " " + timeMatch.Groups[1].Value,
                                    _dateTimeFormatOnPOS,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out receiptDataParsed
                                );
                            }
                        }
                    }
                }

                purchaseResult.OriginalPosIdentification = receiptPosIdentification;
                purchaseResult.OriginalReceiptData = receiptDataParsed;
                purchaseResult.ReceiptData = receiptData;
            }

            var result = new Result
            {
                Success = success,
                ExtraData = purchaseResult
            };

            result.Message = result.Success ? message : ParseErrorResponse(message);

            return result;
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

            var messageIdle = SendCommand(new IdleState().ToString());

            var result = new Result
            {
                Success = message.Substring(9).StartsWith(_okRefund),
                ExtraData = purchaseResult
            };

            result.Message = result.Success ? message : ParseErrorResponse(message);

            return result;
        }

        /// <summary>
        /// Parses the error response.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns>The error response.</returns>
        private static string ParseErrorResponse(string message)
        {
            var response = message.Substring(6, 3);

            if (int.TryParse(response, out int intValue))
            {
                var enumValue = (VerifoneNegativeResponses)intValue;
                var fieldInfo = enumValue.GetType().GetField(enumValue.ToString());
                if (fieldInfo != null)
                {
                    var descriptionAttributes = (System.ComponentModel.DescriptionAttribute[])fieldInfo
                    .GetCustomAttributes(typeof(System.ComponentModel.DescriptionAttribute), false);

                    if (descriptionAttributes.Length > 0)
                    {
                        return descriptionAttributes[0].Description;
                    }
                }
                else
                {
                    return _infoUnknownError;
                }
            }

            return _infoUnknownError;
        }

    }
}