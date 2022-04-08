//using System;
//using System.Collections.Generic;
//using System.Text;
//using Abp.Dependency;
//using RabbitMQ.Client;

//namespace KonbiCloud.Messaging
//{
//    public interface IConnectToRabbitMqService:ISingletonDependency,IDisposable
//    {
//        void Connect();

//        IModel GetQueuedModel();
//        IModel GetNoQueuedModel();
//        IConnection GetConnection();
//    }
//}
