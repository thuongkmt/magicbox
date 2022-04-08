namespace Konbini.RfidFridge.Service.Data
{
    using System.Threading.Tasks;

    public interface ISettingService
    {
        Task<bool> GetAll();
    }
}
