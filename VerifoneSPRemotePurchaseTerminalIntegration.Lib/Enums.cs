﻿using System.ComponentModel;

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
    }
}
