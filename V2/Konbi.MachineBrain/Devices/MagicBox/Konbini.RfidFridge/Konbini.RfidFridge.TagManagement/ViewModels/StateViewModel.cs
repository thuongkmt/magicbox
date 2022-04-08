using System.Diagnostics;
using System.Windows.Input;
using Caliburn.Micro;
using Konbini.RfidFridge.TagManagement.Enums;
using Konbini.RfidFridge.TagManagement.Interface;
using Konbini.RfidFridge.TagManagement.Service;
using Screen = Konbini.RfidFridge.TagManagement.Enums.Screen;

namespace Konbini.RfidFridge.TagManagement.ViewModels
{
    public class StateViewModel : Conductor<object>, IHandle<AppMessage>
    {
        protected IEventAggregator EventAggregator;
        protected ShellViewModel ShellView;
        private MachineState _currentState;
        public MachineState CurrentState
        {
            get => _currentState;
            set
            {
                _currentState = value;
                OnStateChange(value);
            }
        }

        public StateViewModel(IEventAggregator events, ShellViewModel shellView = null)
        {
            EventAggregator = events;
            EventAggregator.Subscribe(this);

            ShellView = shellView;

        }

        public void CheckBeforeHandle(Screen currentScreen)
        {
            if (ShellView.CurrentScreen == currentScreen)
            {
                return;
            }
        }
        public virtual void Handle(AppMessage message)
        {

        }

        public virtual void OnStateChange(MachineState state)
        {

        }

    
    }
}