using System.ComponentModel;

namespace VerifoneSPRemotePurchaseTerminalIntegration.Lib
{
    public class Enums
    {
        public enum TerminalCommandOptions
        {
            [Description("Send terminal status request")]
            SendTerminalStatusRequest = 1,
            [Description("Send terminal open period request")]
            SendTerminalOpenPeriod = 2,
            [Description("Send terminal close period request")]
            SendTerminalClosePeriod = 3,
            [Description("Send terminal purchase request")]
            SendProcessPaymentRequest = 4,
            [Description("Send terminal refund request")]
            SendProcessRefundRequest = 5,
            [Description("Show list of commands")]
            ShowListOfCommands = 9998,
            [Description("Stop listening")]
            StopTheServer = 9999
        }

        public enum VerifoneNegativeResponses
        {
            [Description("Comprimento Inválido")]
            COMPRIMINVALIDO = 001,
            [Description("COMANDO INVÁLIDO")]
            COMANDOINVALIDO = 002,
            [Description("VERSÃO INVALIDA")]
            VERSÃOINVALIDA = 003,
            [Description("FORA DE CONTEXTO")]
            FORADECONTEXTO = 004,
            [Description("OPERAÇÃO ANULADA")]
            OPERACAOANULADA = 005,
            [Description("FORA DE SERVIÇO")]
            FORADESERVICO = 006,
            [Description("MATRICULAR TPA")]
            MATRICULARTPA = 007,
            [Description("MODELO ECR INV.")]
            MODELOECRINV = 008,
            [Description("ERRO")]
            ERRO = 012,
        }

        public enum StatusCode
        {
            [Description("OK Command")]
            OKCommand = 0x00,
            [Description("Operation cancelled by the operator")]
            OperationCancelled = 0x71,
            [Description("Communications Anomaly")]
            CommunicationsAnomaly = 0x72,
            [Description("SIBS refused operation")]
            SIBSRefusedOperation = 0x75,
            [Description("Not Configured Terminal")]
            NotConfiguredTerminal = 0x76,
            [Description("Invalid Period")]
            InvalidPeriod = 0x77,
            [Description("Card Detected in the Reader")]
            CardDetectedInReader = 0x4350,
            [Description("Timeout in the Supervisors’ Features")]
            TimeoutInSupervisorsFeatures = 0x80,
            [Description("Wrong Command Size")]
            WrongCommandSize = 0x82,
            [Description("Invalid Central operation")]
            InvalidCentralOperation = 0x92,
            [Description("Error: Unable to Parse")]
            Error = 9999
        }
    }
}
