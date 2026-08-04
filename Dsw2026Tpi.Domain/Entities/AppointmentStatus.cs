using System;
using System.Collections.Generic;
using System.Text;

namespace Dsw2026Tpi.Domain.Entities;


public enum AppointmentStatus
{
    Booked,
    Cancelled,
    Attended,
    NoShow
}
public static class AppointmentStatusMapper
{
    public static string ToApi(AppointmentStatus status) => status switch
    {
        AppointmentStatus.Booked => "BOOKED",
        AppointmentStatus.Cancelled => "CANCELLED",
        AppointmentStatus.Attended => "ATTENDED",
        AppointmentStatus.NoShow => "NO_SHOW",
        _ => throw new ArgumentOutOfRangeException(nameof(status))
    };

    public static AppointmentStatus FromApi(string value) => value switch
    {
        "BOOKED" => AppointmentStatus.Booked,
        "CANCELLED" => AppointmentStatus.Cancelled,
        "ATTENDED" => AppointmentStatus.Attended,
        "NO_SHOW" => AppointmentStatus.NoShow,
        _ => throw new ArgumentOutOfRangeException(nameof(value))
    };
}
