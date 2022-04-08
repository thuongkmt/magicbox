using System.Threading.Tasks;
using Abp.Dependency;
using Konbini.Messages;

namespace KonbiCloud.Messaging
{
    public interface IMessageHandler : ITransientDependency
    {
        Task<bool> Handle(KeyValueMessage message);
    }

    public interface IProductMessageHandler : IMessageHandler
    {
         
    }

    public interface IProductTagsMessageHandler : IMessageHandler
    {

    }

    public interface IProductCategoryMessageHandler : IMessageHandler
    {

    }

    public interface IProductCategoryRelationMessageHandler : IMessageHandler
    {

    }

    public interface IProductMachinePriceMessageHandler : IMessageHandler
    {

    }

    public interface IManuallySyncProductsMessageHandler : IMessageHandler
    {

    }

    public interface IManuallySyncProductCategoriesMessageHandler : IMessageHandler
    {

    }

    public interface IAlertConfigurationsMessageHandler : IMessageHandler
    {

    }

    public interface ILinePayMessageHandler : IMessageHandler
    {

    }

    public interface ISyncInventoryToCloudMessageHandler : IMessageHandler
    {

    }
}
