using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Visma_Room2Meet.Helpers
{
    public enum BookingStatus
    {
        Pending = 1,
        Booked = 2,
        Meeting = 3,
        Completed = 4,
        Canceled = 5
    }
}