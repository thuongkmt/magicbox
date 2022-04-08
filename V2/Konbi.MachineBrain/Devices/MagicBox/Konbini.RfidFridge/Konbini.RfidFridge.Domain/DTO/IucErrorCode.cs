using System.Collections.Generic;

namespace Konbini.RfidFridge.Domain
{
    public class IucErrorCode
    {
        public static readonly Dictionary<long, string> ComportError = new Dictionary<long, string>
        {
            [0] = "Successful",
            [2] = "COM Port not found",
            [5] = "COM Port in use by another process",
            [5] = "COM Port in use by another process",
            [13] = "Invalid data from Terminal (Bad checksum)",
            [24] = "Command length is too long (Maximum = 1024 bytes)",
            [258] = "Terminal cannot be reached thorugh this port",
            [592] = "Data is not accepted by Terminal (Bad checksum)",
            [1460] = "Timeout",
            [-1] = "Port error",
        };


        public static string GetComportError(long code)
        {
            return ComportError.ContainsKey(code) ? $"{ComportError[code]}" : "Unknow response code";
        }

        public static string CResponseCode(string cmd)
        {
            return CxxxResponseTable.ContainsKey(cmd) ? $"{CxxxResponseTable[cmd]}" : $"[{cmd}] Unknow response code";
        }

        private static readonly Dictionary<string, string> CxxxResponseTable = new Dictionary<string, string>
        {
            ["00"] = "APPROVED",
            ["01"] = "REFER TO CARD ISSUER",
            ["02"] = "REFER TO CARD ISSUER’S SPECIAL CONDITION",
            ["03"] = "ERROR CALL HELP SN",
            ["05"] = "DO NOT HONOUR",
            ["12"] = "INVALID TRANSACTION (HELP TR)",
            ["13"] = "INVALID AMOUNT (HELP AM)",
            ["14"] = "INVALID CARD NUMBER (HELP RE)",
            ["19"] = "RE-ENTER TRANSACTION",
            ["25"] = "UNABLE TO LOCATE RECORD ON FILE (HELP NT)",
            ["30"] = "FORMAT ERROR (HELP FE)",
            ["31"] = "BANK NOT SUPPORTED BY SWITCH (HELP NS)",
            ["41"] = "LOST CARD",
            ["43"] = "STOLEN CARD PICK UP",
            ["51"] = "TRANSACTION DECLINED",
            ["54"] = "EXPIRED CARD",
            ["55"] = "INCORRECT PIN",
            ["58"] = "TRANSACTION NOT PERMITTED IN TERMINAL",
            ["76"] = "INVALID PRODUCT CODES (HELP DC)",
            ["77"] = "RECONCILE ERROR",
            ["78"] = "TRACE# NOT FOUND",
            ["89"] = "BAD TERMINAL ID",
            ["91"] = "ISSUER/SWITCH INOPERATIVE",
            ["94"] = "DUPLICATE TRANSMISSION",
            ["96"] = "SYSTEM MALFUNCTION",
            ["SE"] = "TERMINAL FULL",
            ["PE"] = "PIN ENTRY ERROR",
            ["IC"] = "INVALID CARD",
            ["EC"] = "CARD IS EXPIRED",
            ["CE"] = "CONNECTION ERROR",
            ["RE"] = "RECORD NOT FOUND",
            ["HE"] = "WRONG HOST NUMBER PROVIDED",
            ["LE"] = "LINE ERROR",
            ["VB"] = "TRANSACTION ALREADY VOIDED",
            ["FE"] = "FILE EMPTY / NO TRANSACTION TO VOID",
            ["WC"] = "CARD NUMBER DOES NOT MATCH",
            ["TA"] = "TRANSACTION ABORTED BY USER",
            ["AE"] = "AMOUNT DID NOT MATCH",
            ["XX"] = "TERMINAL NOT PROPERLY SETUP",
            ["DL"] = "LOGON NOT DONE",
            ["BT"] = "BAD TLV COMMAND FORMAT",
            ["IS"] = "TRANSACTION NOT FOUND, INQUIRY SUCCESSFUL",
            ["CD"] = "CARD DECLINED TRANSACTION",
            ["LH"] = "LOYALTY HOST IS TEMPORARY OFFLINE.",
            ["IN"] = "INVALID CARD",
            ["CO"] = "CARD NOT READ PROPERLY",
            ["TL"] = "TOP UP LIMIT EXCEEDED",
            ["PL"] = "PAYMENT LIMIT EXCEEDED",
            ["CA"] = "NOT ACCEPTED",
            ["CT"] = "TIMED OUT"
        };
    }
}