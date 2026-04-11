using System.Net;
using System.Net.Mail;

namespace IdentityApp.Models
{
    public class SmtpEmailSender : IEmailSender
    {
        private string? _host;
        private int _port;
        private bool _enabledSSL;
        private string? _userName;
        private string? _password;

        public SmtpEmailSender( string? host,int port ,bool enabledSSL,string? userName,string? password)
        {
            _host = host;
            _port = port;
            _enabledSSL = enabledSSL;
            _userName = userName;
            _password = password;
        }

        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient(_host, _port)
            {
                // Önce varsayılanları temizle, sonra kendi bilgilerini ver
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_userName, _password),
                EnableSsl = _enabledSSL
            };

            return client.SendMailAsync(new MailMessage(_userName ?? "", email, subject, message) {IsBodyHtml = true});
            
            
        }
    }
    
}
       
        
    
