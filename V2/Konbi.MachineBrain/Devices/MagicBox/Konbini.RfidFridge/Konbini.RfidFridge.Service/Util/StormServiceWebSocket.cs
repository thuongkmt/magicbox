
using Konbini.RfidFridge.Domain.DTO;
using Newtonsoft.Json;
using StompSharp;
using StompSharp.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Konbini.RfidFridge.Service.Util
{
    using Konbini.RfidFridge.Service.Core;
    using StompHelper;
    using System.Threading.Tasks;
    using WebSocketSharp;

    public class StormServiceWebSocket
    {

        private LogService LogService;
        WebSocket ws = new WebSocket("ws://localhost:15674/ws");
        StompMessageSerializer serializer = new StompMessageSerializer();
        String clientId = string.Empty;

        public StormServiceWebSocket(LogService logService)
        {
            LogService = logService;
        }

        public void Connect()
        {
           
            ws.OnMessage += ws_OnMessage;
            ws.OnClose += ws_OnClose;
            ws.OnOpen += ws_OnOpen;
            ws.OnError += ws_OnError;
            ws.Connect();

            SubscribeStomp();
        }

        public void PublishInventory(List<ProductDto> products)
        {
            Task.Run(() =>
            {
                try
                {
                    var broad = new StompMessage(StompFrame.SEND, JsonConvert.SerializeObject(products));
                    broad["content-type"] = "application/json";
                    broad["destination"] = "/topic/inventory";
                    ws.Send(serializer.Serialize(broad));
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            });

        }

        public void PublishTagId(List<TagIdDto> tags)
        {
            Task.Run(() =>
            {
                try
                {
                    var broad = new StompMessage(StompFrame.SEND, JsonConvert.SerializeObject(tags));
                    broad["content-type"] = "application/json";
                    broad["destination"] = "/topic/tagid";
                    ws.Send(serializer.Serialize(broad));
                }
                catch (Exception ex)
                {
                    LogService.LogError(ex);
                }
            });

        }

        private Action<string> _onCommandRev;

        public void SubCommand(Action<string> onCommandRev)
        {
            try
            {
                _onCommandRev = onCommandRev;

            }
            catch (Exception ex)
            {
                LogService.LogError(ex);
            }
        }


        private void ConnectStomp()
        {
            var connect = new StompMessage(StompFrame.CONNECT);
            connect["accept-version"] = "1.2";
            connect["heart-beat"] = "0,5000";
            connect["login"] = "admin";
            connect["passcode"] = "imsuper";
            Console.WriteLine($"Is Alive: {this.ws.IsAlive}");
            if (!ws.IsAlive)
            {
                ws.Connect();
            }
            ws.Send(serializer.Serialize(connect));
        }

        private void SubscribeStomp()
        {
            if (!ws.IsAlive)
            {
                ws.Connect();
            }
            var sub = new StompMessage(StompFrame.SUBSCRIBE);
            sub["id"] = "sub-0";
            sub["destination"] = "/topic/command";
            ws.Send(serializer.Serialize(sub));

            var sub1 = new StompMessage(StompFrame.SUBSCRIBE);
            sub["id"] = "sub-1";
            sub["destination"] = "/topic/inventory";
            ws.Send(serializer.Serialize(sub));
        }


        void ws_OnOpen(object sender, EventArgs e)
        {
            LogService.LogInfo("Stomp OPEN");
            ConnectStomp();
        }

        void ws_OnMessage(object sender, MessageEventArgs e)
        {
            StompMessage msg = serializer.Deserialize(e.Data);
            if (msg.Command == StompFrame.CONNECTED)
            {
                LogService.LogInfo("Stomp CONNECTED");
               
            }
            else if (msg.Command == StompFrame.MESSAGE)
            {
                LogService.LogInfo("Stomp MESSAGE: " + msg.Body);
                _onCommandRev?.Invoke(msg.Body);
            }
        }

        void ws_OnClose(object sender, CloseEventArgs e)
        {
            ConnectStomp();
        }


        void ws_OnError(object sender, ErrorEventArgs e)
        {
            this.LogService.LogInfo(e.ToString());
        }
    }
}
