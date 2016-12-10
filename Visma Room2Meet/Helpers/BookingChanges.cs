using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Visma_Room2Meet.Helpers
{
    public static class BookingChanges
    {
        public static bool HandleChange(int bookingID, string userID, string changeText)
        {
            room2meet_dbEntities db = new room2meet_dbEntities();
            Booking booking = db.Bookings.Find(bookingID);
            if (booking == null)
            {
                return false;
            }
            AspNetUser user = db.AspNetUsers.Find(userID);
            if (user == null)
            {
                return false;
            }
            BookingHistory bh = new BookingHistory
            {
                AspNetUserID = userID,
                BookingID = bookingID,
                ChangeDate = DateTime.Now,
                ChangeText = changeText
            };
            db.BookingHistories.Add(bh);
            db.SaveChanges();
            if (userID != booking.AspNetUserID)
            {
                SMS.SendSMS(booking.AspNetUser.PhoneNumber, "Hello, Today, " + bh.ChangeDate + ", " + user.Name + " has changed something regarding your booking. Please sign in and review this change. Visma Room2Meet");
            }
            return true;
        }
    }
}