using RabbitMQ.Client;
using System;

namespace Konbini.Messages.Services
{
    public interface IConnectToRabbitMqMessageService :  IDisposable
    {
        void Connect(string hostName, string userName, string pwd,string clientName = "KonbiCloud-20190504");
        Action<Exception> ErrorAction { get; set; }
        IModel GetQueuedModel();
        IModel GetNoQueuedModel();
        IConnection GetConnection();
        int GetIdentity();

        void IncreaseIndentity();

        void Reconnect();
    }
}
