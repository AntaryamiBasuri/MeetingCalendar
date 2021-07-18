/*
 * Author: Antaryami Basuri
 * Email: a.basuri2002@gmail.com
 */

using MeetingCalendar.Extensions;
using MeetingCalendar.Interfaces;
using MeetingCalendar.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace MeetingCalendar
{
	/// <summary>
	/// Provides an instance of <see cref="ICalendar"/>.
	/// </summary>
	public class Calendar : ICalendar
	{
		#region Members

		private static class DateTimeConstants
		{
			internal static DateTime DistantFutureDateTime => DateTime.MaxValue.AddMinutes(-1);
		}

		private DateTime _startTime;
		private DateTime _endTime;
		private ICollection<IAttendee> _attendees;

		#endregion Members

		#region Private computed properties

		private DateTime TimeSlotComputationStartTime => (_startTime >= CurrentTime) ? _startTime : CurrentTime;

		#endregion Private computed properties

		#region Public Properties

		/// <summary>
		/// Gets the current date and time calibrated to minutes.
		/// </summary>
		public DateTime CurrentTime => DateTime.Now.CalibrateToMinutes();

		/// <summary>
		/// Gets the <see cref="Calendar"/> time frame in minutes.
		/// </summary>
		public double CalendarWindowInMinutes => this.GetDuration();

		/// <summary>
		/// Gets the list of <see cref="IAttendee"/>.
		/// </summary>
		public IReadOnlyCollection<IAttendee> Attendees => _attendees?.ToList().AsReadOnly();

		/// <summary>
		/// Gets the start time of a <see cref="Calendar"/>.
		/// </summary>
		public DateTime StartTime => _startTime;

		/// <summary>
		/// Gets the end time of a <see cref="Calendar"/>.
		/// </summary>
		public DateTime EndTime => _endTime;

		#endregion Public Properties

		#region Constructors

		/// <summary>
		/// Initializes a new instance of the <see cref="Calendar"/> class.
		/// </summary>
		/// <param name="startTime">The lower bound of allowed meeting hours.</param>
		/// <param name="endTime">The upper bound of allowed meeting hours.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when:
		/// The <see cref="Calendar"/> start time is invalid.
		/// The <see cref="Calendar"/> end time is invalid.
		/// The <see cref="Calendar"/> start time is greater than or equals to start time.
		/// </exception>
		public Calendar(DateTime startTime, DateTime endTime)
		{
			if (startTime.IsInvalidDate())
				throw new ArgumentException("Invalid Calendar start time.", nameof(startTime));

			if (endTime.IsInvalidDate())
				throw new ArgumentException("Invalid Calendar end time.", nameof(endTime));

			_startTime = startTime.CalibrateToMinutes();
			_endTime = endTime.CalibrateToMinutes();

			if (_startTime >= _endTime)
				throw new ArgumentException("The Calendar end time must be greater than the start time.", nameof(endTime));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Calendar"/> class.
		/// </summary>
		/// <param name="timeSlot">The <see cref="ITimeSlot"/> as calendar window.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when:
		/// The <see cref="Calendar"/> start time is invalid.
		/// The <see cref="Calendar"/> end time is invalid.
		/// The <see cref="Calendar"/> start time is greater than or equals to start time.
		/// </exception>
		public Calendar(ITimeSlot timeSlot)
			: this(timeSlot.StartTime, timeSlot.EndTime)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="Calendar"/> class.
		/// </summary>
		/// <param name="timeSlot">The <see cref="ITimeSlot"/> as calendar window.</param>
		/// <param name="attendees">A list of <see cref="Attendee"/> along with their <see cref="MeetingInfo"/>.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when:
		/// The <see cref="Calendar"/> start time is invalid.
		/// The <see cref="Calendar"/> end time is invalid.
		/// The <see cref="Calendar"/> start time is greater than or equals to start time.
		/// </exception>
		public Calendar(ITimeSlot timeSlot, ICollection<IAttendee> attendees)
			: this(timeSlot)
			=> _attendees = attendees;

		/// <summary>
		/// Initializes a new instance of the <see cref="Calendar"/> class.
		/// </summary>
		/// <param name="startTime">The lower bound of allowed meeting hours.</param>
		/// <param name="endTime">The upper bound of allowed meeting hours.</param>
		/// <param name="attendees">A list of <see cref="Attendee"/> along with their <see cref="MeetingInfo"/>.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when:
		/// The <see cref="Calendar"/> start time is invalid.
		/// The <see cref="Calendar"/> end time is invalid.
		/// The <see cref="Calendar"/> start time is greater than or equals to start time.
		/// </exception>
		public Calendar(DateTime startTime, DateTime endTime, ICollection<IAttendee> attendees)
			: this(startTime, endTime)
			=> _attendees = attendees;

		#endregion Constructors

		#region Public methods

		/// <summary>
		/// Add attendees to the calendar.
		/// </summary>
		/// <param name="attendees">The list of <see cref="Attendee"/>.</param>
		public void AddAttendees(ICollection<IAttendee> attendees) => _attendees = attendees;

		/// <summary>
		/// Append additional attendees to existing attendees list.
		/// </summary>
		/// <param name="additionalAttendees">The list of additional <see cref="Attendee"/> to append.</param>
		public void AppendAttendees(ICollection<IAttendee> additionalAttendees)
			=> _attendees = _attendees?.Concat(additionalAttendees ?? new List<IAttendee>()) ?? additionalAttendees;

		/// <summary>
		/// Removes a specified <see cref="IAttendee"/> instance from the <see cref="Calendar"/> based on
		/// the object reference, AttendeeId, AttendeeName, and AttendeeEmailId values.
		/// </summary>
		/// <param name="attendee">The <see cref="IAttendee"/> instance.</param>
		/// <returns>True if removal is successful, false otherwise.</returns>
		public bool RemoveAttendee(IAttendee attendee) => _attendees.Remove(attendee);

		/// <summary>
		/// Removes an <see cref="IAttendee"/> from the <see cref="Calendar"/> by the specified Id.
		/// </summary>
		/// <param name="attendeeId">The id of the <see cref="IAttendee"/>.</param>
		/// <returns><c>true</c> if removal is successful, <c>false</c> otherwise.</returns>
		public bool RemoveAttendee(Guid attendeeId)
		{
			var attendee = _attendees.FirstOrDefault(item => item.AttendeeId == attendeeId);

			return attendee != null && RemoveAttendee(attendee);
		}

		/// <summary>
		/// Removes an <see cref="IAttendee"/> from the <see cref="Calendar"/> by the name and emailId.
		/// </summary>
		/// <param name="attendeeName">The name of the <see cref="IAttendee"/>.</param>
		/// <param name="attendeeEmailId">The email id of the <see cref="IAttendee"/>.</param>
		/// <returns><c>true</c> if removal is successful, <c>false</c> otherwise.</returns>
		public bool RemoveAttendee(string attendeeName, string attendeeEmailId)
		{
			var attendee = _attendees.FirstOrDefault(item =>
				item.AttendeeEmailId == attendeeEmailId && item.AttendeeName == attendeeName);

			return attendee != null && RemoveAttendee(attendee);
		}

		/// <summary>
		/// Finds the first available time slot for the requested meeting duration.
		/// </summary>
		/// <param name="meetingDuration">The meeting duration in minutes.</param>
		/// <returns>A an instance of <see cref="ITimeSlot"/> or null.</returns>
		public ITimeSlot FindFirstAvailableSlot(int meetingDuration)
		{
			var availableMeetingSlots = GetAllAvailableTimeSlots();

			return availableMeetingSlots.FindFirst(t => t.GetDuration() >= meetingDuration);
		}

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
		public ITimeSlot FindFirstAvailableSlot(int meetingDuration, ITimeSlot timeSlot)
			=> FindFirstAvailableSlot(meetingDuration, timeSlot.StartTime, timeSlot.EndTime);

		/// <summary>
		/// Finds the first available time slot for the requested meeting duration, optionally within a specific time frame.
		/// </summary>
		/// <param name="meetingDuration">The meeting duration in minutes.</param>
		/// <param name="fromTime">The lower bound date time for a search.</param>
		/// <param name="toTime">The upper bound date time for a search. The default is EndTime of <see cref="Calendar"/>.</param>
		/// <exception cref="ArgumentException">
		/// Thrown when:
		/// The meeting duration is less than or equal to zero.
		/// The meeting duration is longer than the search range.
		/// The search range upper limit is less than current time.
		/// The search range start time is invalid.
		/// The search range upper bound is less than the lower bound time.
		/// </exception>
		/// <returns>A an instance of <see cref="ITimeSlot"/> or null.</returns>
		public ITimeSlot FindFirstAvailableSlot(int meetingDuration, DateTime fromTime, DateTime toTime = default)
		{
			ValidateSearchRange(meetingDuration, fromTime, toTime);

			toTime = (toTime == default) ? _endTime : toTime;

			var lowerBound = (fromTime >= _startTime) ? fromTime.CalibrateToMinutes() : _startTime;
			var upperBound = (toTime >= _endTime) ? _endTime : toTime.CalibrateToMinutes();

			var availableMeetingSlots = GetAllAvailableTimeSlots();

			return availableMeetingSlots
				.Select(t => t.TimeSlotMapper(lowerBound, upperBound))
				.FindFirst(t => t.GetDuration() >= meetingDuration && t.StartTime >= lowerBound && t.EndTime <= upperBound);
		}

		/// <summary>
		/// Find all available meeting slots.
		/// </summary>
		/// <returns>A list of <see cref="ITimeSlot"/>.</returns>
		public IEnumerable<ITimeSlot> GetAllAvailableTimeSlots()
		{
			ICollection<ITimeSlot> availableMeetingSlots = new List<ITimeSlot>();

			//Do not calculate available meeting slots for past meetings - Performance improvement
			if (_endTime <= CurrentTime)
			{
				return availableMeetingSlots;
			}

			var meetingHoursByMinutes =
				this.GetTimeSeriesByMinutes(TimeSlotComputationStartTime)
					.FillWith(AvailabilityTypes.Available);

			if (Attendees != null && Attendees.Any())
			{
				availableMeetingSlots = GetAvailability(availableMeetingSlots, meetingHoursByMinutes);
			}
			else
			{
				var meetingHoursByMinutesList = meetingHoursByMinutes.OrderBy(i => i.Key).ToList();
				availableMeetingSlots.Add(new TimeSlot(meetingHoursByMinutesList.First().Key, meetingHoursByMinutesList.Last().Key));
			}

			return availableMeetingSlots;
		}

		/// <summary>
		/// Moves forward the <see cref="ITimeFrame"/> window of the <see cref="Calendar"/>.
		/// </summary>
		/// <param name="clearAttendees">
		/// Clears the list of attendees, if <c>true</c>; otherwise, <c>false</c>. The default is <c>true</c>.
		/// </param>
		public void MoveForward(bool clearAttendees = true)
		{
			var duration = CalendarWindowInMinutes;

			if (clearAttendees)
			{
				_attendees.Clear();
			}

			_startTime = _endTime;
			_endTime = _endTime.AddMinutes(duration);
		}

		/// <summary>
		/// Moves forward the <see cref="ITimeFrame"/> window of the <see cref="Calendar"/>.
		/// </summary>
		/// <param name="attendees">
		/// Replaces existing attendees with the specified list of attendees.
		/// </param>
		public void MoveForward(ICollection<IAttendee> attendees)
		{
			MoveForward();
			AddAttendees(attendees);
		}

		/// <summary>
		/// Moves backward the <see cref="ITimeFrame"/> window of the <see cref="Calendar"/>.
		/// </summary>
		/// <param name="clearAttendees">
		/// Clears the list of attendees, if <c>true</c>; otherwise, <c>false</c>. The default is <c>true</c>.
		/// </param>
		public void MoveBackward(bool clearAttendees = true)
		{
			var duration = CalendarWindowInMinutes;

			if (clearAttendees)
			{
				_attendees.Clear();
			}

			_endTime = _startTime;
			_startTime = _startTime.AddMinutes(duration * -1);
		}

		/// <summary>
		/// Moves backward the <see cref="ITimeFrame"/> window of the <see cref="Calendar"/>.
		/// </summary>
		/// <param name="attendees">
		/// Replaces existing attendees with the specified list of attendees.
		/// </param>
		public void MoveBackward(ICollection<IAttendee> attendees)
		{
			MoveBackward();
			AddAttendees(attendees);
		}

		/// <summary>
		/// Deconstruct a <see cref="Calendar"/>.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		public void Deconstruct(out DateTime startTime, out DateTime endTime)
		{
			startTime = StartTime;
			endTime = EndTime;
		}

		/// <summary>
		/// Deconstruct a <see cref="Calendar"/>.
		/// </summary>
		/// <param name="startTime">The start time.</param>
		/// <param name="endTime">The end time.</param>
		/// <param name="currentTime">The current time.</param>
		/// <param name="calendarWindowInMinutes">The calendar time frame in minutes.</param>
		/// <param name="attendees">The attendees.</param>
		public void Deconstruct(out DateTime startTime, out DateTime endTime, out DateTime currentTime,
			out double calendarWindowInMinutes, out IReadOnlyCollection<IAttendee> attendees)
		{
			startTime = StartTime;
			endTime = EndTime;
			currentTime = CurrentTime;
			calendarWindowInMinutes = CalendarWindowInMinutes;
			attendees = Attendees;
		}

		#endregion Public methods

		#region Private methods

		private static void CalculateAvailableSlots(ICollection<ITimeSlot> availableMeetingSlots,
			IEnumerable<KeyValuePair<DateTime, AvailabilityTypes>> scheduledHoursByMinutes,
			ITimeSlot availableTimeSlot = null, AvailabilityTypes searchVal = AvailabilityTypes.Available)
		{
			var hoursByMinutes = scheduledHoursByMinutes.ToList();
			if (!hoursByMinutes.Any()) return;

			var foundItem = hoursByMinutes.FirstOrDefault(item => item.Value == searchVal);
			if (foundItem.Key != default)
			{
				var subSet = hoursByMinutes.Where(dict => dict.Key > foundItem.Key);
				if (searchVal == AvailabilityTypes.Scheduled && availableTimeSlot != null)
				{
					availableMeetingSlots.Add(new TimeSlot(availableTimeSlot.StartTime, foundItem.Key.AddMinutes(-1)));
					CalculateAvailableSlots(availableMeetingSlots, subSet);
				}
				else
				{
					availableTimeSlot = new TimeSlot(foundItem.Key, DateTimeConstants.DistantFutureDateTime);
					CalculateAvailableSlots(availableMeetingSlots, subSet, availableTimeSlot, AvailabilityTypes.Scheduled);
				}
			}
			else if (availableTimeSlot != null)
			{
				foundItem = hoursByMinutes.Last();
				availableTimeSlot = new TimeSlot(availableTimeSlot.StartTime, foundItem.Key);
				availableMeetingSlots.Add(availableTimeSlot);
			}
		}

		private ICollection<ITimeSlot> GetAvailability(ICollection<ITimeSlot> availableMeetingSlots, IDictionary<DateTime, AvailabilityTypes> meetingHoursByMinutes)
		{
			var updatedMeetingHoursByMinutes = Attendees.Count <= 8 && CalendarWindowInMinutes <= 960
				? GetAllAvailableTimeSlots(meetingHoursByMinutes)
				: GetAllAvailableTimeSlotsAsParallel(meetingHoursByMinutes.ToConcurrentDictionary());

			if (updatedMeetingHoursByMinutes.Any(i => i.Value == AvailabilityTypes.Available))
			{
				CalculateAvailableSlots(availableMeetingSlots, updatedMeetingHoursByMinutes.OrderBy(i => i.Key));
			}

			return availableMeetingSlots;
		}

		private IDictionary<DateTime, AvailabilityTypes> GetAllAvailableTimeSlots(IDictionary<DateTime, AvailabilityTypes> meetingHoursByMinutes)
		{
			Debug.Assert(Attendees != null, nameof(Attendees) + "Attendees != null");

			Attendees.ForEach(attendee =>
			{
				attendee.Meetings.Where(meeting => !meeting.IsOver())
					.ForEach(scheduledMeeting =>
						scheduledMeeting
							.GetTimeSlotMappedToCalendarTimeFrame(_startTime, _endTime)
							.GetTimeSeriesByMinutes()
							.FillWith(AvailabilityTypes.Scheduled)
							.ForEach(item =>
							{
								if (meetingHoursByMinutes.TryGetValue(item.Key, out var previousValue) &&
									previousValue == AvailabilityTypes.Available)
								{
									meetingHoursByMinutes[item.Key] = AvailabilityTypes.Scheduled;
								}
							}));
			});

			return meetingHoursByMinutes;
		}

		private IDictionary<DateTime, AvailabilityTypes> GetAllAvailableTimeSlotsAsParallel(ConcurrentDictionary<DateTime, AvailabilityTypes> meetingHoursByMinutes)
		{
			Debug.Assert(Attendees != null, nameof(Attendees) + "Attendees != null");

			Attendees.ForEach(attendee => attendee.Meetings
				.Where(meeting => !meeting.IsOver())
				.AsParallel()
				.ForAll(scheduledMeeting =>
					scheduledMeeting
						.GetTimeSlotMappedToCalendarTimeFrame(_startTime, _endTime)
						.GetTimeSeriesByMinutes()
						.FillWith(AvailabilityTypes.Scheduled)
						.AsParallel()
						.ForAll(item =>
							meetingHoursByMinutes.TryUpdate(item.Key, AvailabilityTypes.Scheduled, AvailabilityTypes.Available))));

			return meetingHoursByMinutes;
		}

		private void ValidateSearchRange(int meetingDuration, DateTime fromTime, DateTime toTime)
		{
			if (fromTime.IsInvalidDate())
				throw new ArgumentException("Invalid start time.", nameof(fromTime));

			if (meetingDuration <= 0)
			{
				throw new ArgumentException("The meeting duration can not be less than or equal to zero.", nameof(meetingDuration));
			}

			if (toTime != default && toTime.CalibrateToMinutes() <= fromTime.CalibrateToMinutes())
			{
				throw new ArgumentException("Search range upper limit can not be less than the lower limit time.", nameof(toTime));
			}

			if (toTime != default &&
				meetingDuration > toTime.CalibrateToMinutes().Subtract(fromTime.CalibrateToMinutes()).TotalMinutes)
			{
				throw new ArgumentException(
					"The meeting duration can not be longer than the search range. Consider to increase the search range.",
					nameof(meetingDuration));
			}

			if (toTime != default && toTime.CalibrateToMinutes() <= CurrentTime)
			{
				throw new ArgumentException("Search range upper limit can not be less than the current time.", nameof(toTime));
			}
		}

		#endregion Private methods
	}
}