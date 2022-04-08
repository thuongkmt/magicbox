using System;
using System.Collections.Generic;
using System.Text;

namespace Konbini.Messages
{
    public class RabbitMqConstants
    {
        public const string DEFAULT_QUEUE = "KonbiDefaultQueue";
        public const string CLIENT_TO_SERVER_QUEUE = "KonbiMachine2CloudQueue";
        //public const string CLIENT_TO_SERVER_QUEUE = "KonbiMachine2CloudQueue";

        public const string DEFAULT_ROUTING_KEY = "KonbiDefaultRoutingKey";
        public const string EXCHANGE_M2CLOUD_NOQUEUE = "KonbiMachine2CloudExchangeNoQueue";
        public const string EXCHANGE_CLOUD_TO_MACHINE_QUEUED = "KonbiCloud2MachineExchangeQueued";
        public const string EXCHANGE_CLOUD_TO_MACHINE_QUEUED_LINE = "KonbiCloud2MachineExchangeQueuedLinePay";
        public const string EXCHANGE_M2CLOUD_QUEUED = "KonbiMachine2CloudExchangeQueued";


        //Routing keys
        public const string ROUTING_KEY_MACHINES = "KonbiAllMachinesRoutingKey";
        public const string ROUTING_KEY_MACHINE_ID = "KonbiMachineIdRoutingKey";
        public const string ROUTING_KEY_MACHINE_TENANT = "KonbiMachineTenantRoutingKey";



        //public const string DEFAULT_QUEUE = "KonbiDefaultQueue_Ha20190504";
        //public const string CLIENT_TO_SERVER_QUEUE = "KonbiMachine2CloudQueue_Ha20190504";
        ////public const string CLIENT_TO_SERVER_QUEUE = "KonbiMachine2CloudQueue";

        //public const string DEFAULT_ROUTING_KEY = "KonbiDefaultRoutingKey_Ha20190504";
        //public const string EXCHANGE_M2CLOUD_NOQUEUE = "KonbiMachine2CloudExchangeNoQueue_Ha20190504";
        //public const string EXCHANGE_CLOUD_TO_MACHINE_QUEUED = "KonbiCloud2MachineExchangeQueued_Ha20190504";
        //public const string EXCHANGE_M2CLOUD_QUEUED = "KonbiMachine2CloudExchangeQueued_Ha20190504";


        ////Routing keys
        //public const string ROUTING_KEY_MACHINES = "KonbiAllMachinesRoutingKey_Ha20190504";
        //public const string ROUTING_KEY_MACHINE_ID = "KonbiMachineIdRoutingKey_Ha20190504";
        //public const string ROUTING_KEY_MACHINE_TENANT = "KonbiMachineTenantRoutingKey_Ha20190504";


    }
}
