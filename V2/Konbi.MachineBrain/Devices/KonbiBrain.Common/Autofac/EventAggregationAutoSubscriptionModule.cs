using Autofac;
using Autofac.Core;
using Caliburn.Micro;
using System;

namespace Caliburn.Micro.Autofac
{
    public class EventAggregationAutoSubscriptionModule : Module
    {
        public EventAggregationAutoSubscriptionModule()
        {
        }

        protected override void AttachToComponentRegistration(IComponentRegistry registry, IComponentRegistration registration)
        {
            registration.Activated += new EventHandler<ActivatedEventArgs<object>>(EventAggregationAutoSubscriptionModule.OnComponentActivated);
        }

        private static void OnComponentActivated(object sender, ActivatedEventArgs<object> e)
        {
            if (e == null)
            {
                return;
            }
            IHandle instance = e.Instance as IHandle;
            if (instance != null)
            {
                e.Context.Resolve<IEventAggregator>().Subscribe(instance);
            }
        }
    }
}