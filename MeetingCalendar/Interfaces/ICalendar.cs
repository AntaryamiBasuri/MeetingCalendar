/*
 * Author: Antaryami Basuri
 * Email: a.basuri2002@gmail.com
 */

using MeetingCalendar.Models;
using System;
using System.Collections.Generic;

namespace MeetingCalendar.Interfaces
{
	/// <summary>
	/// Interface for <see cref="ICalendar"/>.
	/// </summary>
	public interface ICalendar : ITimeFrame
	{
		/// <summary>
		/// Gets the current date and time calibrated to minutes.
		/// </summary>
		DateTime CurrentTime { get; }

		/// <summary>
		/// Gets the <see cref="ICalendar"/> time frame in minutes.
		/// </summary>
		double CalendarWindowInMinutes { get; }

		/// <summary>
		/// Gets the list of <see cref="IAttendee"/>.
		/// </summary>
		IReadOnlyCollection<IAttendee> Attendees { get; }

		/// <summary>
		/// Add attendees to the calendar.
		/// </summary>
		/// <param name="attendees">The list of <see cref="IAttendee"/>.</param>
		void AddAttendees(ICollection<IAttendee> attendees);

		/// <summary>
		/// Appends a list of additional attendees to the existing attendees list.
		/// </summary>
		/// <param name="additionalAttendees">The list of additional <see cref="IAttendee"/>.</param>
		void AppendAttendees(ICollection<IAttendee> additionalAttendees);

		/// <summary>
		/// Removes a specified <see cref="IAttendee"/> instance.
		/// </summary>
		/// <param name="attendee">The <see cref="IAttendee"/> instance.</param>
		/// <returns><c>True</c> if removal is successful, <c>false</c> otherwise.</returns>
		bool RemoveAttendee(IAttendee attendee);

		/// <summary>
		/// Removes a specified <see cref="IAttendee"/>.
		/// </summary>
		/// <param name="attendeeId">The id of the <see cref="IAttendee"/>.</param>
		/// <returns><c>True</c> if removal is successful, <c>false</c> otherwise.</returns>
		bool RemoveAttendee(Guid attendeeId);

		/// <summary>
		/// Removes a specified <see cref="IAttendee"/>.
		/// </summary>
		/// <param name="attendeeName">The name of the <see cref="IAttendee"/>.</param>
		/// <param name="attendeeEmailId">The email id of the <see cref="IAttendee"/>.</param>
		/// <returns><c>True</c> if removal is successful, <c>false</c> otherwise.</returns>
		bool RemoveAttendee(string attendeeName, string attendeeEmailId);

		/// <summary>
		/// Finds the first available time slot for the requested meeting duration.
		/// </summary>
		/// <param name="meetingDuration">The meeting duration in minutes.</param>
		/// <returns>A an instance of <see cref="ITimeSlot"/> or null.</returns>
		ITimeSlot FindFirstAvailableSlot(int meetingDuration);

		/// <summary>
		/// Finds the first available time slot for the requested meeting duration, optionally within a specific time frame.
		/// </summary>
		/// <param name="meetingDuration">The meeting duration in minutes.</param>
		/// <param name="timeSlot">The time slot as the search range.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when:
		/// The meeting duration is less than or equal to zero.
		/// The meeting duration is longer than the search range.
		/// The search range upper limit is less than current time.
		/// The search range start time is invalid.
		/// The search range upper bound is less than the lower bound time.
		/// </exception>
		/// <returns>A an instance of <see cref="ITimeSlot"/> or null.</returns>
		ITimeSlot FindFirstAvailableSlot(int meetingDuration, ITimeSlot timeSlot);

		/// <summary>
		/// Finds the first available time slot for the requested meeting duration within a specific time frame.
		/// </summary>
		/// <param name="meetingDuration">The meeting duration in minutes.</param>
		/// <param name="fromTime">The lower bound date time for a search.</param>
		/// <param name="toTime">The upper bound date time for a search. The default is EndTime of <see cref="ICalendar"/>.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when:
		/// The meeting duration is less than or equal to zero.
		/// The meeting duration is longer than the search range.
		/// The search range upper limit is less than current time.
		/// The search range start time is invalid.
		/// The search range upper bound is less than the lower bound time.
		/// </exception>
		/// <returns>A an instance of <see cref="ITimeSlot"/> or null.</returns>
		ITimeSlot FindFirstAvailableSlot(int meetingDuration, DateTime fromTime, DateTime toTime = default);

		/// <summary>
		/// Find all available meeting slots.
		/// </summary>
		/// <returns>A list of <see cref="TimeSlot"/>.</returns>
		IEnumerable<ITimeSlot> GetAllAvailableTimeSlots();

		/// <summary>
		/// Deconstruct a <see cref="ICalendar"/>.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <param name="currentTime">The current time.</param>
		/// <param name="calendarWindowInMinutes">The calendar time frame in minutes.</param>
		/// <param name="attendees">The attendees.</param>
		void Deconstruct(out DateTime startTime, out DateTime endTime, out DateTime currentTime,
			out double calendarWindowInMinutes, out IReadOnlyCollection<IAttendee> attendees);
	}
}