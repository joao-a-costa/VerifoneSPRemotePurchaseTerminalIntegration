using System;
using System.Net.Sockets;
using VerifoneSPRemotePurchaseTerminalIntegration.Lib;
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

        //private static readonly string serverIp = "192.168.40.175";
        //private static readonly string serverIp = "192.168.1.252";
        //private static readonly int port = 15200;
        private static readonly string serverIp = "192.168.40.175";
        private static readonly int port = 5005;

        private static readonly VerifoneSPRemote VerifoneSPRemote = new VerifoneSPRemote(serverIp, port);

        #endregion

        static void Main()
        {
            try
            {
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
                            var teste = VerifoneSPRemote.OpenPeriod("0001");
                            break;
                        case TerminalCommandOptions.SendTerminalClosePeriod:
                            VerifoneSPRemote.ClosePeriod("0001");
                            break;
                        case TerminalCommandOptions.SendProcessPaymentRequest:
                            VerifoneSPRemote.Purchase("0001", Convert.ToInt32((Math.Round(new Random().NextDouble() * (1.99 - 0.01) + 0.01, 2)) * 100).ToString().PadLeft(8, '0'),
                                false);
                            break;
                        case TerminalCommandOptions.SendProcessRefundRequest:
                            VerifoneSPRemote.Refund(new Lib.Models.PurchaseResult());
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

        #endregion
    }
}