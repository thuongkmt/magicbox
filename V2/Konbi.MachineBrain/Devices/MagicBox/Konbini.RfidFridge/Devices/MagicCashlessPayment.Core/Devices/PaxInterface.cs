using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MagicCashlessPayment.Core.Devices
{
    public class PaxInterface
    {
        TransactionResult CurrentTransactionResult = new TransactionResult();

        [DllImport("madaapi_v1_7.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int api_RequestCOMTrxn(int bPort, int dwBaudRate, int bParity, int bDataBits, int bStopBits, byte[] inReqBuff, byte[] inReqLen, int txtype,
                        ref byte[] panNo, ref byte[] purAmount, ref byte[] stanNo, ref byte[] dataTime, ref byte[] expDate, ref byte[] trxRrn, ref byte[]
                        authCode, ref byte[] rspCode, ref byte[] terminalId, ref byte[] schemeId, ref byte[] merchantId, ref byte[]
                        addtlAmount, ref byte[] ecrrefno, ref byte[] version, [MarshalAs(UnmanagedType.LPWStr)] StringBuilder outresp, ref int outRspLen);

        [DllImport("madaapi_v1_7.dll", CallingConvention = CallingConvention.StdCall)]
        public static extern int api_CommTest(int bPort, int dwBaudRate, int bParity, int bDataBits, int bStopBits, byte[] inReqBuff, byte[] inReqLen);


        public int RequestCOMTxn(int bPort, int dwBaudRate, int bParity, int bDataBits, int bStopBits, string command, int txtype)
        {
            var cmd = command.StringToCommand();
            var result = api_RequestCOMTrxn(bPort, dwBaudRate, bParity, bDataBits, bStopBits, cmd.Item1, cmd.Item2, txtype,
                        ref CurrentTransactionResult.panNo, ref CurrentTransactionResult.purAmount, ref CurrentTransactionResult.stanNo, ref CurrentTransactionResult.dataTime, ref CurrentTransactionResult.expDate, ref CurrentTransactionResult.trxRrn,
                        ref CurrentTransactionResult.authCode, ref CurrentTransactionResult.rspCode, ref CurrentTransactionResult.terminalId, ref CurrentTransactionResult.schemeId, ref CurrentTransactionResult.merchantId,
                        ref CurrentTransactionResult.addtlAmount, ref CurrentTransactionResult.ecrrefno, ref CurrentTransactionResult.version, CurrentTransactionResult.outresp, ref CurrentTransactionResult.outRspLen);
            return result;
        }

        public int Port { get; set; }
        const int BAURATE = 34800;
        const int PARITY = 0;
        const int DATABITS = 8;
        const int STOPBIT = 0;

        public Action<string> LogAction { get; set; }

        private int EcrReferenceNumber = 0;

        public bool CommTest(string comport)
        {
            Log($"Connecting to terminal by comport: {comport}");
            var result = false;
            int.TryParse(comport.ToLower().Replace("com", string.Empty), out int intPort);
            Port = intPort;

            var cmd = "04!".StringToCommand();

            var r = api_CommTest(Port, BAURATE, PARITY, DATABITS, STOPBIT, cmd.Item1, cmd.Item2);

            if (r == 0)
            {
                Log("Connected to PAX terminal!");
                result = true;
            }
            else
            {
                Log($"Failed to connect to PAX terminal: {GetApiResponseCode(r)}");
            }
            return result;
        }

        private int AuthTransaction(int amount)
        {
            var cmd = new SendCommand.AuthCommand(amount, EcrReferenceNumber++);
            return Transaction(TransactionType.Authorization, cmd);
        }

        private int AdviceTransaction(int amount, string txnNumber)
        {
            var cmd = new SendCommand.AdviceCommand(amount, txnNumber, EcrReferenceNumber++);
            return Transaction(TransactionType.Advice, cmd);
        }

        private int Transaction(TransactionType type, SendCommand.BaseCommand command)
        {
            Log($"Start Transaction: {type} | Command: {command}");
            var r = RequestCOMTxn(Port, BAURATE, PARITY, DATABITS, STOPBIT, command.ToString(), (int)type);
            Log($"Send Command Result: {GetApiResponseCode(r)}");
            return r;
        }

        private void Log(string message)
        {
            LogAction?.Invoke(message);
        }

        public string GetApiResponseCode(int code)
        {
            if (ApiResponseCode.ContainsKey(code))
            {
                return $"[{code}] {ApiResponseCode[code]}";
            }
            else
            {
                return $"[{code}] UNKNOWN RESPONSE CODE";
            }
        }



        public class SendCommand
        {
            public class BaseCommand
            {
                public int EcrReferenceNumber;

            }

            public class TransactionCommand : BaseCommand
            {
                public int Amount;
                public bool EnablePrinter;
            }

            public class AuthCommand : TransactionCommand
            {
                public AuthCommand(int amount, int identity)
                {
                    Amount = amount;
                    EcrReferenceNumber = identity;
                }

                public override string ToString()
                {
                    var printer = EnablePrinter ? "1" : "0";
                    return $"{Amount};{printer};{EcrReferenceNumber}!";
                }
            }

            public class AdviceCommand : TransactionCommand
            {
                public string TransactionApprovalNumber;
                public AdviceCommand(int amount, string transactionApprovalNumber, int identity)
                {
                    Amount = amount;
                    EcrReferenceNumber = identity;
                    TransactionApprovalNumber = transactionApprovalNumber;
                }

                public override string ToString()
                {
                    var printer = EnablePrinter ? "1" : "0";
                    return $"{Amount};{TransactionApprovalNumber};{printer};{EcrReferenceNumber}!";
                }
            }
        }

        public enum TransactionType
        {
            Purchase = 0,
            PurchaseWithCashback = 1,
            Refund = 2,
            Reversal = 3,
            Authorization = 12,
            Advice = 13,
            CashAdvance = 14
        }

        private Dictionary<int, string> ApiResponseCode = new Dictionary<int, string>()
        {
            {0,"Success"},
            {-1,"Library Failed"},
            {-2,"No Response Received"},
            {-3,"Not able to open port"},
            {-4,"Acknowledgement Failed"},
            {1,"Terminal TMS not Loaded"},
            {2,"Blocked Card"},
            {3,"No Active Application Found"},
            {4,"Card Read Error"},
            {5,"Insert Card Only"},
            {6,"Maximum Amount Limit Exceeded"},
            {7,"PIN Quit"},
            {8,"User Cancelled or Timeout"},
            {9,"Card Scheme Not Supported"},
            {10,"Terminal Busy"},
            {11,"Paper Out"},
            {12,"No Reconciliation Record found"},
            {13,"Transaction Cancelled"},
            {14,"De-SAF Processing"},
        };

        public class TransactionResult
        {
            public byte[] panNo;
            public byte[] purAmount;
            public byte[] stanNo;
            public byte[] dataTime;
            public byte[] expDate;
            public byte[] trxRrn;
            public byte[] authCode;
            public byte[] rspCode;
            public byte[] terminalId;
            public byte[] schemeId;
            public byte[] merchantId;
            public byte[] addtlAmount;
            public byte[] ecrrefno;
            public byte[] version;
            public StringBuilder outresp;
            public int outRspLen;
        }


    }

    public static class PaxExt
    {
        public static Tuple<byte[], byte[]> StringToCommand(this String command)
        {
            var cmd = Encoding.ASCII.GetBytes(command);
            var length = new byte[1];
            length[0] = (byte)cmd.Length;
            return new Tuple<byte[], byte[]>(cmd, length);
        }
    }
}
