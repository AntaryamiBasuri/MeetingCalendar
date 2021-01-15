﻿using System;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MeetingCalenderTest")]
namespace MeetingCalender.Extensions
{
    /// <summary>
    /// Extension class for <see cref="DateTime"/> type.
    /// </summary>
    internal static class DateTimeExtensions
    {
        /// <summary>
        /// Calibrates the datetime to minutes by discarding its seconds and milliseconds component.
        /// </summary>
        /// <param name="dateTime">The date time instance</param>
        /// <returns>A new calibrated datetime instance.</returns>
        
        internal static DateTime CalibrateToMinutes(this DateTime dateTime)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, 0, dateTime.Kind);
        }
    }
}