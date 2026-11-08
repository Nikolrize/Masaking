using System;
using System.Configuration;
using System.Net.Mail;

namespace Masaking.Infrastructure
{
    public static class RecoveryMail
    {
        public static void SendOtp(string recipient, string code)
        {
            if (!RecoveryOtp.HasValidCode(code))
            {
                throw new ArgumentException("Invalid OTP.");
            }

            string body = "Your Masaking password reset code is:\r\n\r\n" + code
                + "\r\n\r\nEnter this code in the same browser where you requested it, together with your new password. "
                + "The code expires after 10 minutes and can be used once. Never share this code. "
                + "If you did not request this, you can ignore this email. Your password has not changed.";

            Send(recipient, "Your Masaking password reset code", body);
        }

        public static void SendConfirmation(string recipient)
        {
            string body = "Your Masaking password has been reset and previous sign-ins have been invalidated. "
                + "If you did not make this change, contact the site administrator immediately.";

            Send(recipient, "Your Masaking password was changed", body);
        }

        private static void Send(string recipient, string subject, string body)
        {
            string from = ConfigurationManager.AppSettings["PasswordResetFromEmail"];
            if (string.IsNullOrWhiteSpace(from))
            {
                throw new ConfigurationErrorsException("Configure PasswordResetFromEmail and system.net/mailSettings.");
            }

            using (var message = new MailMessage(from, recipient, subject, body))
            using (var client = new SmtpClient())
            {
                bool isLocalHost = string.Equals(client.Host, "localhost", StringComparison.OrdinalIgnoreCase)
                    || client.Host == "127.0.0.1"
                    || client.Host == "::1";

                if (client.DeliveryMethod == SmtpDeliveryMethod.Network && !client.EnableSsl && !isLocalHost)
                {
                    throw new ConfigurationErrorsException("Remote SMTP delivery requires TLS (enableSsl=true).");
                }

                message.IsBodyHtml = false;
                client.Timeout = 10000;
                client.Send(message);
            }
        }
    }
}