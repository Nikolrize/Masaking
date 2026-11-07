using System;
using System.Configuration;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Net;
using System.Net.Mail;
using System.Net.Sockets;

namespace Masaking.Infrastructure
{
    public static class RecoveryDelivery
    {
        public static void ReportFailure(Exception error)
        {
            string category = FailureCategory(error);
            Trace.TraceError("Password recovery failed: {0}", category);
        }

        public static string FailureCategory(Exception error)
        {
            for (Exception current = error; current != null; current = current.InnerException)
            {
                if (current is SqlException sql)
                {
                    return sql.Number == 207 || sql.Number == 208
                        ? "Database schema is missing recovery tables or OTP columns. Run the manual setup SQL."
                        : "Database operation failed. Check database availability and permissions.";
                }

                if (current is SmtpException smtp)
                {
                    int status = (int)smtp.StatusCode;
                    bool isAuthFailure = status == 535
                        || status == 534
                        || status == 530
                        || smtp.Message.IndexOf("5.7.8", StringComparison.OrdinalIgnoreCase) >= 0;

                    if (isAuthFailure)
                    {
                        return "SMTP authentication failed. Check the sender account and generate a fresh Google App Password.";
                    }

                    // A connection failure can be nested inside a generic SMTP exception.
                    if (smtp.InnerException == null)
                    {
                        return "SMTP rejected the message. Check the provider's sending permissions and limits.";
                    }
                }

                if (current is SocketException || current is WebException || current is TimeoutException)
                {
                    return "SMTP network or TLS connection failed. Check connectivity and port 587.";
                }

                if (current is ConfigurationErrorsException)
                {
                    return "Email configuration is missing or invalid. Check the sender and SMTP settings.";
                }
            }

            if (error is InvalidOperationException)
            {
                return "Recovery setup failed. Check OWIN startup and email configuration.";
            }

            return "Password recovery processing failed.";
        }
    }
}