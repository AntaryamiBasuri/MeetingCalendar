﻿/*
 * Author: Antaryami Basuri
 * Email: a.basuri2002@gmail.com
 */

using MeetingCalendar.Extensions;
using MeetingCalendar.Interfaces;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace MeetingCalendar
{
    public class Calendar : ICalendar
    {
        private readonly DateTime _startTime;
        private readonly DateTime _endTime;
        private readonly IList<TimeSlot> _availableMeetingSlots;

        private double CalendarWindowInMinutes => _endTime.Subtract(_startTime).TotalMinutes;

        /// <summary>
        /// Gets the list of <see cref="Attendees"/>.
        /// </summary>
        public IEnumerable<Attendee> Attendees { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="Calendar"/>
        /// </summary>
        /// <param name="startTime">The lower bound of allowed meeting hours.</param>
        /// <param name="endTime">The upper bound of allowed meeting hours.</param>
        public Calendar(DateTime startTime, DateTime endTime)
        {
            _startTime = startTime.CalibrateToMinutes();
            _endTime = endTime.CalibrateToMinutes();

            if (_startTime >= _endTime)
                throw new ArgumentException("The allowed meeting hours end time must be greater than the start time.", nameof(endTime));

            _availableMeetingSlots = new List<TimeSlot>();
        }

        /// <summary>
        /// Initializes a new instance of <see cref="Calendar"/>
        /// </summary>
        /// <param name="startTime">The lower bound of allowed meeting hours.</param>
        /// <param name="endTime">The upper bound of allowed meeting hours.</param>
        /// <param name="attendees">Attendees along with their <see cref="MeetingInfo"/></param>
        public Calendar(DateTime startTime, DateTime endTime, IEnumerable<Attendee> attendees)
            : this(startTime, endTime)
            => Attendees = attendees;

        /// <summary>
        /// Add attendees to the calender.
        /// </summary>
        public void AddAttendees(IEnumerable<Attendee> attendees) => Attendees = attendees;

        /// <summary>
        /// Appends additional attendees to existing attendees list.
        /// </summary>
        /// <param name="additionalAttendees"></param>
        public void AppendAttendees(IEnumerable<Attendee> additionalAttendees)
            => Attendees = (Attendees == null) ? additionalAttendees : Attendees.Concat<Attendee>(additionalAttendees);

        /// <summary>
        /// Returns the first time slot available for the requested meeting duration.
        /// </summary>
        /// <param name="meetingDuration">The meeting duration in minutes.</param>
        /// <returns>A time slot or null</returns>
        public TimeSlot GetFirstAvailableSlot(int meetingDuration)
        {
            var availableMeetingSlots = GetAllAvailableTimeSlots();

            var meetingSlots = availableMeetingSlots.ToArray();
            if (meetingSlots.Any())
            {
                return meetingSlots.Length == 1 ?
                    meetingSlots.First(t => t.AvailableDuration >= meetingDuration) :
                    meetingSlots.OrderBy(o => o.AvailableDuration).ThenBy(i => i.StartTime)
                        .FirstOrDefault(t => t.AvailableDuration >= meetingDuration);
            }
            return null;
        }

        /// <summary>
        /// Find all available meeting slots.
        /// </summary>
        /// <returns>A list of <see cref="TimeSlot"/></returns>
        public IEnumerable<TimeSlot> GetAllAvailableTimeSlots()
        {
            //Do not calculate available meeting slots for past meetings - Performance improvement
            if (_endTime <= DateTime.Now.CalibrateToMinutes())
            {
                return _availableMeetingSlots;
            }

            //Calculate the availability only from NOW onwards - Performance improvement
            var startTime = (_startTime >= DateTime.Now) ? _startTime : DateTime.Now.CalibrateToMinutes();
            var meetingHoursByMinutes = GetTimeSeriesByMinutes(startTime, _endTime);

            //Map the meeting timings of each attendees
            if (Attendees != null && Attendees.Any())
            {
                if (Attendees.Count() < 8 || CalendarWindowInMinutes <= 480)
                {
                    GetAllAvailableTimeSlots(meetingHoursByMinutes);
                }
                else
                {
                    GetAllAvailableTimeSlotsAsParallel(meetingHoursByMinutes);
                }

                //Calculate only if any slot is available.
                if (meetingHoursByMinutes.Any(i => i.Value == false)) 
                {
                    CalculateAvailableSlots(meetingHoursByMinutes.OrderBy(i => i.Key));
                }
            }
            else
            {
                var meetingHoursByMinutesList = meetingHoursByMinutes.OrderBy(i => i.Key).ToList();
                _availableMeetingSlots.Add(new TimeSlot(meetingHoursByMinutesList.First().Key, meetingHoursByMinutesList.Last().Key));
            }

            return _availableMeetingSlots;
        }

        private void GetAllAvailableTimeSlotsAsParallel(ConcurrentDictionary<DateTime, bool> meetingHoursByMinutes)
        {
            Attendees.ForEach(attendee =>
            {
                attendee.MeetingInfo.AsParallel().ForAll(scheduledMeeting =>
                {
                    // if the meeting is not over yet, then only include in the calculation - Performance improvement
                    if (scheduledMeeting.EndTime > DateTime.Now)
                    {
                        //Consider the scheduled meeting durations only within the time frame of Calendar- Performance improvement
                        var timeSeries = GetTimeSeriesByMinutes(
                            (scheduledMeeting.StartTime >= _startTime) ? scheduledMeeting.StartTime : _startTime,
                            (scheduledMeeting.EndTime <= _endTime) ? scheduledMeeting.EndTime : _endTime
                            , true);
                        //Merge the meeting duration of the attendee
                        timeSeries.AsParallel().ForAll(item =>
                        {
                            //Update the value only when the minute has not been marked yet as unavailable- Performance improvement
                            if (meetingHoursByMinutes.TryGetValue(item.Key, out bool prevValue) && !prevValue)
                            {
                                meetingHoursByMinutes.TryUpdate(item.Key, item.Value, false);
                            }
                        });
                    }
                });
            });
        }

        private void GetAllAvailableTimeSlots(IDictionary<DateTime, bool> meetingHoursByMinutes)
        {
            Attendees.ForEach(attendee =>
            {
                attendee.MeetingInfo.ForEach(scheduledMeeting =>
                {
                    // if the meeting is not over yet, then only include in the calculation - Performance improvement
                    if (scheduledMeeting.EndTime > DateTime.Now)
                    {
                        //Consider the scheduled meeting durations only within the time frame of Calendar- Performance improvement
                        var timeSeries = GetTimeSeriesByMinutes(
                            (scheduledMeeting.StartTime >= _startTime) ? scheduledMeeting.StartTime : _startTime,
                            (scheduledMeeting.EndTime <= _endTime) ? scheduledMeeting.EndTime : _endTime
                            , true);
                        //Merge the meeting duration of the attendee
                        timeSeries.ForEach(item =>
                        {
                            //Update the value only when the minute has not been marked yet as unavailable - Performance improvement
                            if (meetingHoursByMinutes.TryGetValue(item.Key, out bool prevValue) && !prevValue)
                            {
                                meetingHoursByMinutes[item.Key] = item.Value;
                            }
                        });
                    }
                });
            });
        }

        private void CalculateAvailableSlots(IEnumerable<KeyValuePair<DateTime, bool>> scheduledHoursByMinutes, TimeSlot availableTimeSlot = null, bool searchVal = false)
        {
            var hoursByMinutes = scheduledHoursByMinutes.ToList();
            if (!hoursByMinutes.Any()) return;

            var foundItem = hoursByMinutes.FirstOrDefault(item => item.Value == searchVal);
            if (foundItem.Key != DateTime.MinValue)
            {
                var subSet = hoursByMinutes.Where(dict => dict.Key > foundItem.Key);
                if (searchVal && availableTimeSlot != null)
                {
                    availableTimeSlot = new TimeSlot(availableTimeSlot.StartTime, foundItem.Key.AddMinutes(-1));
                    _availableMeetingSlots.Add(availableTimeSlot);
                    CalculateAvailableSlots(subSet);
                }
                else
                {
                    availableTimeSlot = new TimeSlot(foundItem.Key, DateTime.MaxValue);
                    CalculateAvailableSlots(subSet, availableTimeSlot, true);
                }
            }
            else if (availableTimeSlot != null)
            {
                foundItem = hoursByMinutes.Last();
                availableTimeSlot = new TimeSlot(availableTimeSlot.StartTime, foundItem.Key);
                _availableMeetingSlots.Add(availableTimeSlot);
            }
        }

        private static ConcurrentDictionary<DateTime, bool> GetTimeSeriesByMinutes(DateTime seriesStartTime, DateTime seriesEndTime, bool isScheduled = false)
        {
            var timeRange = new ConcurrentDictionary<DateTime, bool>();
            var temp = seriesStartTime;
            while (temp < seriesEndTime)
            {
                timeRange.TryAdd(temp, isScheduled);
                temp = temp.AddMinutes(1);
            }

            return timeRange;
        }
    }
}