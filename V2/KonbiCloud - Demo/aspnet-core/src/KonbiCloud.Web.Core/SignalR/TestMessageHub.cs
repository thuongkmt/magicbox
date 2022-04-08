using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Abp;
using Abp.AspNetCore.SignalR.Hubs;
using Abp.Auditing;
using Abp.Localization;
using Abp.RealTime;
using Abp.Runtime.Session;
using Abp.UI;
using Castle.Core.Logging;
using Castle.Windsor;
using KonbiCloud.SignalR;
using KonbiCloud.Web.Chat.SignalR;

namespace KonbiCloud.Web.SignalR
{
    public class TestMessageHub : OnlineClientHubBase
    {
        private readonly IMessageManager messageManager;
        private readonly ILocalizationManager _localizationManager;
        private readonly IWindsorContainer _windsorContainer;
        private bool _isCallByRelease;

        public TestMessageHub(
            IMessageManager messageManager,
            ILocalizationManager localizationManager,
            IWindsorContainer windsorContainer,
            IOnlineClientManager onlineClientManager, IClientInfoProvider clientInfoProvider) : base(onlineClientManager, clientInfoProvider)
        {
            this.messageManager = messageManager;
            _localizationManager = localizationManager;
            _windsorContainer = windsorContainer;

            Logger = NullLogger.Instance;
            AbpSession = NullAbpSession.Instance;
        }

        public async Task<string> SendMessage(SendMessageInput input)
        {
            var sender = AbpSession.ToUserIdentifier();
            var receiver = new UserIdentifier(input.TenantId, input.UserId);

            try
            {
                await messageManager.SendMessageAsync(sender, receiver, input.Message, input.TenancyName, input.UserName, input.ProfilePictureId);
                return string.Empty;
            }
            catch (UserFriendlyException ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return ex.Message;
            }
            catch (Exception ex)
            {
                Logger.Warn("Could not send chat message to user: " + receiver);
                Logger.Warn(ex.ToString(), ex);
                return _localizationManager.GetSource("AbpWeb").GetString("InternalServerError");
            }
        }

        public void Register()
        {
            Logger.Debug("A client is registered: " + Context.ConnectionId);
        }

        protected override void Dispose(bool disposing)
        {
            if (_isCallByRelease)
            {
                return;
            }
            base.Dispose(disposing);
            if (disposing)
            {
                _isCallByRelease = true;
                _windsorContainer.Release(this);
            }
        }
    }
}
