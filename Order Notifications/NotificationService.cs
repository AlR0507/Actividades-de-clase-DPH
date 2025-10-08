using OrderNotifications.Models;
using System;
using System.Runtime.InteropServices;

namespace OrderNotifications
{
    public class NotificationService : INotificationService
    {
        public void NotifyOrderStatus(Order order)
        {
            foreach (NotificationChannel channel in Enum.GetValues<NotificationChannel>())
            {
                if (ShouldNotify(order.Customer, channel))
                {
                    SendNotification(order, channel);
                }
            }
        }

        private bool ShouldNotify(Customer customer, NotificationChannel channel)
        {
            if(customer.Preferences.TryGetValue(channel, out var preference))
            {
                return preference == NotificationChannelPreference.Enabled;
            }

            if (customer.CountryCode == "AK")
            {
                
                    return false;
            }

            return channel switch
            {
                NotificationChannel.Email => true,
                NotificationChannel.SMS => customer.CountryCode == "MX" ? false : true,
                NotificationChannel.WhatsApp => customer.CountryCode == "MX" ? true : false,
            };
        }

        private void SendNotification(Order order, NotificationChannel channel)
        {
            var message = $"Order {order.Id} is now {order.Status}";
            var contact = order.Customer.ContactInfo;

            switch (channel)
            {
                case NotificationChannel.Email:
                    Console.WriteLine($"Sending Email to {contact.Email}: {message}");
                    break;
                case NotificationChannel.SMS:
                    Console.WriteLine($"Sending SMS to {contact.PhoneNumber}: {message}");
                    break;
                case NotificationChannel.WhatsApp:
                    Console.WriteLine($"Sending WhatsApp message to {contact.PhoneNumber}: {message}");
                    break;
            }
        }
    }
}
