using System;
using System.Net.Sockets;
using VerifoneSPRemotePurchaseTerminalIntegration.Lib;
using VerifoneSPRemotePurchaseTerminalIntegration.Lib.Models;
using static VerifoneSPRemotePurchaseTerminalIntegration.Lib.Enums;

namespace VerifoneSPRemotePurchaseTerminalIntegration.Console
{
    internal static class Program
    {
        #region "Constants"

        private const string _MessageTheFollowingCommandsAreAvailable = "The following commands are available:";
        private const string _MessageInvalidInput = "Invalid input";

        #endregion

        #region "Members"

        private static readonly string serverIp = "192.168.40.108";
        private static readonly int port = 5005;

        private static VerifoneSPRemote VerifoneSPRemote = null;

        #endregion

        static void Main()
        {
            try
            {
                var logger = NLog.LogManager.GetCurrentClassLogger();
                logger.Info("Application started.");

                VerifoneSPRemote = new VerifoneSPRemote(serverIp, port, logger);

                ListenForUserInput();
            }
            catch (Exception e) when (e is ArgumentNullException || e is SocketException)
            {
                System.Console.WriteLine($"{e.GetType().Name}: {e.Message}");
            }
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
        }        

        #region "Private Methods"

        /// <summary>
        /// Listens for user input and sends the input to the WebSocket server.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        private static void ListenForUserInput()
        {
            var serverIsRunning = true;
            Result lastProcessPaymentResult = null;

            while (serverIsRunning)
            {
                ShowListOfCommands();
                var input = System.Console.ReadLine()?.ToLower();

                if (int.TryParse(input, out int commandValue) && Enum.IsDefined(typeof(TerminalCommandOptions), commandValue))
                {
                    var command = (TerminalCommandOptions)commandValue;
                    switch (command)
                    {
                        case TerminalCommandOptions.SendTerminalStatusRequest:
                            VerifoneSPRemote.TerminalStatus();
                            break;
                        case TerminalCommandOptions.SendTerminalOpenPeriod:
                            VerifoneSPRemote.OpenPeriod();
                            break;
                        case TerminalCommandOptions.SendTerminalClosePeriod:
                            VerifoneSPRemote.ClosePeriod();
                            break;
                        case TerminalCommandOptions.SendProcessPaymentRequest:
                            lastProcessPaymentResult = VerifoneSPRemote.Purchase(new Random().Next(1000, 10000).ToString(), Convert.ToInt32((Math.Round(new Random().NextDouble() * (1.99 - 0.01) + 0.01, 2)) * 100).ToString().PadLeft(8, '0'),
                                false);
                            break;
                        case TerminalCommandOptions.SendProcessRefundRequest:

                            if (lastProcessPaymentResult == null)
                                System.Console.WriteLine("Please process a payment first.");
                            else
                                VerifoneSPRemote.Refund((PurchaseResult)lastProcessPaymentResult.ExtraData);
                            break;
                        case TerminalCommandOptions.ParsePurchaseResponse:
                            ParsePurchaseResponse();
                            break;
                        case TerminalCommandOptions.ShowListOfCommands:
                            ShowListOfCommands();
                            break;
                        case TerminalCommandOptions.StopTheServer:
                            serverIsRunning = false;
                            break;
                    }
                }
                else
                {
                    System.Console.WriteLine(_MessageInvalidInput);
                    ShowListOfCommands();
                }
            }
        }

        /// <summary>
        /// Shows the list of commands.
        /// </summary>
        private static void ShowListOfCommands()
        {
            System.Console.WriteLine($"\n\n{_MessageTheFollowingCommandsAreAvailable}");
            foreach (TerminalCommandOptions command in Enum.GetValues(typeof(TerminalCommandOptions)))
            {
                System.Console.WriteLine($"   {(int)command} - {Utilities.GetEnumDescription(command)}");
            }
            System.Console.WriteLine();
        }

        private static void ParsePurchaseResponse()
        {
            System.Console.WriteLine("Insert a valid message...");

            var input = System.Console.ReadLine()?.ToLower();

            var res = VerifoneSPRemote.ParsePurchaseResponse(false, new PurchaseResult(), input);

            System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(res, Newtonsoft.Json.Formatting.Indented));
        }

        #endregion
    }
}