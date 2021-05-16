using System;
using System.IO;
using System.Net;
using System.Net.Mail;
using System.Text;
using GameStore.Domain.Abstract;
using GameStore.Domain.Entities;

namespace GameStore.Domain.Concrete
{
    public class EmailSettings
    {
        public string MailToAddress = "mail@example.com";
        public string MailFromAddress = "gamestore@example.com";
        public bool UseSsl = false;
        public string Username = "SmtpUsername";
        public string Password = "SmtpPassword";
        public string ServerName = "smtp.sever.com";
        public bool WriteAsFile = true;
        public int ServerPort = 648;
        public string FileLocation = @"c:\users\mrjav\source\repos\gameStore\mailExmplForOrders";
    }

    public class EmailOrderProcessor : IOrderProcessor
    {
        private EmailSettings emailSettings;
        
        public EmailOrderProcessor(EmailSettings settings)
        {
            emailSettings = settings;
            emailSettings.WriteAsFile = true;
        }

        public void ProcessOrder(Cart cart, ShippingDetails shippingInfo)
        {
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.EnableSsl = emailSettings.UseSsl;
                smtpClient.Host = emailSettings.ServerName;
                smtpClient.Port = emailSettings.ServerPort;
                smtpClient.UseDefaultCredentials = false;
                smtpClient.Credentials
                    = new NetworkCredential(emailSettings.Username, emailSettings.Password);

                if (emailSettings.WriteAsFile)
                {
                    smtpClient.DeliveryMethod
                        = SmtpDeliveryMethod.SpecifiedPickupDirectory;
                    smtpClient.PickupDirectoryLocation = emailSettings.FileLocation;
                    smtpClient.EnableSsl = false;
                }

                StringBuilder body = new StringBuilder()
                    .AppendLine("Нове замовлення оброблене")
                    .AppendLine("---")
                    .AppendLine("Товари:");

                foreach (var line in cart.Lines)
                {
                    var subtotal = line.Game.Price * line.Quantity;
                    body.AppendFormat("{0} x {1} (разом: {2:c}",
                        line.Quantity, line.Game.Name, subtotal);
                }

                body.AppendFormat("Загальна вартість: {0:c}", cart.ComputeTotalValue())
                    .AppendLine("---")
                    .AppendLine("Доставка:")
                    .AppendLine(shippingInfo.Name)
                    .AppendLine(shippingInfo.Line1)
                    .AppendLine(shippingInfo.Line2 ?? "")
                    .AppendLine(shippingInfo.Line3 ?? "")
                    .AppendLine(shippingInfo.City)
                    .AppendLine(shippingInfo.Country)
                    .AppendLine("---")
                    .AppendFormat("Подрункова упаковка: {0}",
                        shippingInfo.GiftWrap ? "Да" : "Ні");

                MailMessage mailMessage = new MailMessage(
                                       emailSettings.MailFromAddress,	
                                       emailSettings.MailToAddress,		
                                       "Нове замовлення відправлене!",		
                                       body.ToString()); 				

                if (emailSettings.WriteAsFile == true)
                {
                    mailMessage.BodyEncoding = Encoding.Default;
                }
                else
                {
                    smtpClient.Send(mailMessage);
                }
            }
        }
    }
}