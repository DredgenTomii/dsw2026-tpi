using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;

internal class AppointmentStatus
{
    public enum AppointmentStatus
    {
        Booked,
        Cancelled,
        Attended,
        NoShow
    }
}
