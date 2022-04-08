using System;
using System.Threading.Tasks;
using Konbini.Messages;

namespace KonbiCloud.Messaging
{
    public interface IMessageHandler 
    {
        Task<Boolean> Handle(KeyValueMessage keyValueMessage);
    }

    public interface IProductMessageHandler : IMessageHandler
    {

    }

    public interface ITransactionMessageHandler : IMessageHandler
    {

    }

    public interface IInventoryMessageHandler : IMessageHandler
    {

    }

    public interface ITopupMessageHandler : IMessageHandler
    {

    }

    public interface IInventoryRestockMessageHandler : IMessageHandler
    {

    }

    public interface IUpdateInventoryListMessageHandler : IMessageHandler
    {

    }

    public interface IMachineStatusMessageHandler : IMessageHandler
    {

    }

    public interface ITemperatureLogsMessageHandler : IMessageHandler
    {

    }

    public interface IProductTagsMessageHandler : IMessageHandler
    {

    }

    public interface IChangeTagStateMessageHandler:IMessageHandler
    {

    }

    public interface IProductCategoryMessageHandler : IMessageHandler
    {

    }

    public interface IProductTagsRealtimeMessageHandler : IMessageHandler
    {

    }

    public interface IManuallySyncProductsMessageHandler : IMessageHandler
    {

    }

    public interface IManuallySyncProductCategoriesMessageHandler : IMessageHandler
    {

    }

    public interface ISyncInventoriesForActiveTopupMessageHandler : IMessageHandler
    {

    }
}
