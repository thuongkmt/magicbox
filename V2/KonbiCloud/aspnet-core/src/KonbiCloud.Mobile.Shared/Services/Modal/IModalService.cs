using System.Threading.Tasks;
using KonbiCloud.Views;
using Xamarin.Forms;

namespace KonbiCloud.Services.Modal
{
    public interface IModalService
    {
        Task ShowModalAsync(Page page);

        Task ShowModalAsync<TView>(object navigationParameter) where TView : IXamarinView;

        Task<Page> CloseModalAsync();
    }
}
