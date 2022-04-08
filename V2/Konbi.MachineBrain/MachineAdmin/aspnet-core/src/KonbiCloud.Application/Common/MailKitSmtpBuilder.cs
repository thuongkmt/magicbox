using System;
using System.Collections.Generic;
using System.Text;
using Abp.MailKit;
using Abp.Net.Mail.Smtp;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace KonbiCloud.Common
{
    public class MyMailKitSmtpBuilder : DefaultMailKitSmtpBuilder
    {
        private readonly ISmtpEmailSenderConfiguration _smtpEmailSenderConfiguration;
        public MyMailKitSmtpBuilder(ISmtpEmailSenderConfiguration smtpEmailSenderConfiguration, IAbpMailKitConfiguration abpMailKitConfiguration)
            : base(smtpEmailSenderConfiguration,abpMailKitConfiguration)
        {
            _smtpEmailSenderConfiguration = smtpEmailSenderConfiguration;
        }

        public virtual SmtpClient Build()
        {
            var client = new SmtpClient();

            try
            {
                ConfigureClient(client);
                return client;
            }
            catch
            {
                client.Dispose();
                throw;
            }
        }

        protected override void ConfigureClient(SmtpClient client)
        {
            client.ServerCertificateValidationCallback = (sender, certificate, chain, errors) => true;

            //base.ConfigureClient(client);
            client.Connect(
                _smtpEmailSenderConfiguration.Host,
                _smtpEmailSenderConfiguration.Port,
                SecureSocketOptions.Auto
            //_smtpEmailSenderConfiguration.EnableSsl
            //SecureSocketOptions.SslOnConnect
            );

            if (_smtpEmailSenderConfiguration.UseDefaultCredentials)
            {
                return;
            }

            client.Authenticate(
                _smtpEmailSenderConfiguration.UserName,
                _smtpEmailSenderConfiguration.Password
            );
        }
    }
}
