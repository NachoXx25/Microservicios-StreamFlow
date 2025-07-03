using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailMicroservice.src.Infrastructure.MessageBroker.Models;
using EmailMicroservice.src.Interfaces;
using System.Net.Mail;
using Serilog;
using DotNetEnv;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace EmailMicroservice.src.Implements
{
    public class BillEventHandler : IBillEventHandler
    {
        public async Task HandleBillUpdatedEvent(BillUpdated billEvent)
        {
            Log.Information("📧 Enviando email de factura actualizada a: {UserEmail}", billEvent.UserEmail);
            try
            {
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress("StreamFlow", Env.GetString("FROM_EMAIL")));
                message.To.Add(new MailboxAddress(billEvent.UserName, billEvent.UserEmail));
                message.Subject = "Factura Actualizada";

                var bodyBuilder = new BodyBuilder();
                bodyBuilder.HtmlBody = GenerateEmailTemplate(billEvent);
                message.Body = bodyBuilder.ToMessageBody();

                using (var smtpClient = new MailKit.Net.Smtp.SmtpClient())
                {
                   
                    smtpClient.ServerCertificateValidationCallback = (s, certificate, chain, sslPolicyErrors) => true;
                    
                    await smtpClient.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
                    await smtpClient.AuthenticateAsync(
                        Env.GetString("FROM_EMAIL"), 
                        Env.GetString("FROM_EMAIL_PASSWORD")
                    );
                    await smtpClient.SendAsync(message);
                    await smtpClient.DisconnectAsync(true);
                }
                
                Log.Information("✅ Email enviado exitosamente a: {UserEmail}", billEvent.UserEmail);
            }
            catch (Exception ex)
            {
                Log.Error("❌ Error al enviar email: {Error}", ex.Message);
                throw;
            }
        }

        private string GenerateEmailTemplate(BillUpdated billEvent)
        {
            return $@"
            <html>
            <head>
                <style>
                    body {{ font-family: Arial, sans-serif; }}
                    .container {{ max-width: 600px; margin: 0 auto; }}
                    .header {{ background-color: #4CAF50; color: white; padding: 20px; }}
                    .content {{ padding: 20px; }}
                    .status {{ color: #2196F3; font-weight: bold; }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>StreamFlow - Actualización de Factura</h1>
                    </div>
                    <div class='content'>
                        <p>Estimado/a <strong>{billEvent.UserName}</strong>,</p>
                        <p>Le informamos que su factura ha sido actualizada al estado:</p>
                        <p class='status'>{billEvent.BillStatus}, </p>
                        <p class='status'>con el monto:{billEvent.BillAmount}</p>
                        <p><small>Fecha: {DateTime.Now:dd/MM/yyyy HH:mm}</small></p>
                        <hr>
                        <p><em>Este es un email automático, no responder.</em></p>
                    </div>
                </div>
            </body>
            </html>";
        }
    }
}