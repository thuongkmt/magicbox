using Xamarin.Forms.Internals;

namespace KonbiCloud.Behaviors
{
    [Preserve(AllMembers = true)]
    public interface IAction
    {
        bool Execute(object sender, object parameter);
    }
}