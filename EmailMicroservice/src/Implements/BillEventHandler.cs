using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailMicroservice.src.Infrastructure.MessageBroker.Models;
using EmailMicroservice.src.Interfaces;
using System.Net.Mail;
using Serilog;
using DotNetEnv;
using System.Net;

namespace EmailMicroservice.src.Implements
{
    public class BillEventHandler : IBillEventHandler
    {
        public async Task HandleBillUpdatedEvent(BillUpdated billEvent)
        {
            Log.Information("📧 Enviando email de factura actualizada a: {UserEmail}, Estado: {BillStatus}",
               billEvent.UserEmail, billEvent.BillStatus);
            try
            {
                var mailMessage = new MailMessage();
                mailMessage.From = new MailAddress(Env.GetString("FROM_EMAIL"));
                mailMessage.To.Add(new MailAddress(billEvent.UserEmail));
                mailMessage.Subject = "Factura Actualizada";
                mailMessage.IsBodyHtml = true;
                mailMessage.Body = GenerateEmailTemplate(billEvent);

                using (var smtpClient = new SmtpClient("smtp.gmail.com"))
                {
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new NetworkCredential(Env.GetString("FROM_EMAIL"), Env.GetString("FROM_EMAIL_PASSWORD"));
                    smtpClient.EnableSsl = true;
                    await smtpClient.SendMailAsync(mailMessage);
                }
                Log.Information("✅ Email enviado exitosamente a: {UserEmail}", billEvent.UserEmail);
            }
            catch (Exception ex)
            {
                Log.Error($"Error al manejar el evento de actualización de factura: {ex.Message}");
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