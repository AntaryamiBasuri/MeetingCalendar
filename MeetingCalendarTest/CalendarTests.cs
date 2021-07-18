/*
 * Author: Antaryami Basuri
 * Email: a.basuri2002@gmail.com
 */

using MeetingCalendar.Extensions;
using MeetingCalendar.Interfaces;
using MeetingCalendar.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MeetingCalendar.Tests
{
	[TestFixture]
	[Author("A Basuri", "a.basuri2002@gmail.com")]
	public class CalendarTests
	{
		private ICalendar _meetingCalendar;

		[SetUp]
		public void Setup()
		{
			//Get the allowed meeting hours
			var startTime = DateTime.Now;
			var endTime = startTime.AddHours(3);

			var attendeesWithMeetingTimings = new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now.AddMinutes(5), DateTime.Now.AddMinutes(7)),
					new MeetingInfo(DateTime.Now.AddMinutes(12), DateTime.Now.AddMinutes(18))
				}),
				new Attendee("Person2", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now.AddMinutes(6), DateTime.Now.AddMinutes(10)),
					new MeetingInfo(DateTime.Now.AddMinutes(15), DateTime.Now.AddMinutes(20))
				})
			};

			_meetingCalendar = new Calendar(startTime, endTime, attendeesWithMeetingTimings);
		}

		[Test]
		public void Constructor_With_TimesSlot()
		{
			var timeSlot = new TimeSlot(DateTime.Now, DateTime.Now.AddDays(1));
			var meetingCalendar = new Calendar(timeSlot);

			Assert.That(meetingCalendar.StartTime, Is.EqualTo(timeSlot.StartTime));
			Assert.That(meetingCalendar.EndTime, Is.EqualTo(timeSlot.EndTime));
			Assert.That(meetingCalendar.Attendees, Is.Null);
		}

		[Test]
		public void Constructor_With_TimesSlot_With_Attendees()
		{
			var timeSlot = new TimeSlot(DateTime.Now, DateTime.Now.AddDays(1));
			var attendees = new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now.AddMinutes(5), DateTime.Now.AddMinutes(7))
				}),
				new Attendee("Person2", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now.AddMinutes(12), DateTime.Now.AddMinutes(18))
				})
			};
			var meetingCalendar = new Calendar(timeSlot, attendees);

			Assert.That(meetingCalendar.StartTime, Is.EqualTo(timeSlot.StartTime));
			Assert.That(meetingCalendar.EndTime, Is.EqualTo(timeSlot.EndTime));
			Assert.That(meetingCalendar.Attendees.Count, Is.EqualTo(2));
			Assert.That(meetingCalendar.Attendees.First(), Is.EqualTo(attendees.First()));
			Assert.That(meetingCalendar.Attendees.Last(), Is.EqualTo(attendees.Last()));
			Assert.That(meetingCalendar.Attendees, Is.EqualTo(attendees));
		}

		[Test]
		public void Deconstructor_Provides_StartTime_And_EndTime()
		{
			var timeSlot = new TimeSlot(DateTime.Now, DateTime.Now.AddDays(1));
			var meetingCalendar = new Calendar(timeSlot);

			var (startTime, endTime) = meetingCalendar;

			Assert.That(meetingCalendar.StartTime, Is.EqualTo(startTime));
			Assert.That(meetingCalendar.EndTime, Is.EqualTo(endTime));
		}

		[Test]
		public void TimeSlotAvailableForRequestedDuration()
		{
			var availableSlot = _meetingCalendar.FindFirstAvailableSlot(1);
			Assert.That(availableSlot, Is.Not.Null);
			Assert.That(availableSlot.GetDuration(), Is.EqualTo(1));
		}

		[Test]
		public void FetchAttendees() => Assert.That(_meetingCalendar.Attendees.Count, Is.EqualTo(2));

		[Test]
		public void AddNewAttendeesToMeeting()
		{
			var meetingCalendar = new Calendar(DateTime.Now, DateTime.Now.AddDays(1));
			meetingCalendar.AddAttendees(new List<IAttendee>
			{
				new Attendee("Person4", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now.AddMinutes(5), DateTime.Now.AddMinutes(7)),
					new MeetingInfo(DateTime.Now.AddMinutes(12), DateTime.Now.AddMinutes(18))
				})
			});

			Assert.That(meetingCalendar.Attendees.Count, Is.EqualTo(1));
			Assert.That(meetingCalendar.Attendees.First().AttendeeName, Is.EqualTo("Person4"));
		}

		[Test]
		public void AppendNewAttendeesToMeeting()
		{
			_meetingCalendar.AppendAttendees(new List<IAttendee>
			{
				new Attendee("Person3", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now.AddMinutes(5), DateTime.Now.AddMinutes(7)),
					new MeetingInfo(DateTime.Now.AddMinutes(12), DateTime.Now.AddMinutes(18))
				})
			});

			Assert.That(_meetingCalendar.Attendees.Count, Is.EqualTo(3));
		}

		[Test]
		public void AppendNullAttendeesToMeeting_Should_Not_Raise_Error()
		{
			var meetingCalendar = new Calendar(DateTime.Now, DateTime.Now.AddDays(1));
			meetingCalendar.AddAttendees(new List<IAttendee>
			{
				new Attendee("Person4", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now.AddMinutes(5), DateTime.Now.AddMinutes(7)),
					new MeetingInfo(DateTime.Now.AddMinutes(12), DateTime.Now.AddMinutes(18))
				})
			});

			meetingCalendar.AppendAttendees(null);

			Assert.That(meetingCalendar.Attendees.Count, Is.EqualTo(1));
			Assert.That(meetingCalendar.Attendees.First().AttendeeName, Is.EqualTo("Person4"));

			void AppendAttendees() => meetingCalendar.AppendAttendees(null);
			Assert.DoesNotThrow(AppendAttendees, "AppendAttendees threw exception.");
		}

		[Test]
		public void AppendNewAttendeesToMeeting_When_AttendeeList_Is_Null()
		{
			Assert.DoesNotThrow(() =>
			{
				_meetingCalendar.AddAttendees(null);
				_meetingCalendar.AppendAttendees(new List<IAttendee>
				{
					new Attendee("Person3", new List<IMeetingInfo>
					{
						new MeetingInfo(DateTime.Now.AddMinutes(5), DateTime.Now.AddMinutes(7)),
						new MeetingInfo(DateTime.Now.AddMinutes(12), DateTime.Now.AddMinutes(18))
					})
				});
			});
			Assert.That(_meetingCalendar.Attendees.Count, Is.EqualTo(1));
		}

		[Test]
		public void GetAllAvailableTimeSlots_Returns_No_TimeSlot_When_Calendar_Upper_Limit_Approaches_Current_Time()
		{
			var calendar = new Calendar(DateTime.Now.AddHours(-8), DateTime.Now.AddMinutes(1), new List<IAttendee>
			{
				new Attendee("Attendee1", new List<IMeetingInfo>())
			});

			var source = calendar.GetAllAvailableTimeSlots().ToList();
			Assert.That(source.Count, Is.Zero);
		}

		[Test]
		public void FindFirstAvailableSlot_Returns_No_TimeSlot_For_200Minutes_Meeting_Duration()
		{
			var availableSlot = _meetingCalendar.FindFirstAvailableSlot(200);
			Assert.That(availableSlot, Is.Null);
		}

		[Test]
		public void FindFirstAvailableSlot_Returns_Null_When_NoSlot_Available()
		{
			var startTime = DateTime.Now;
			var endTime = startTime.AddHours(8);
			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(startTime, endTime)
				})
			});

			var availableSlot = meetingCalendar.FindFirstAvailableSlot(1);
			Assert.That(availableSlot, Is.Null);
		}

		[Test]
		public void FindFirstAvailableSlot_Returns_Null_Beyond_Allowed_MeetingHours()
		{
			var endTime = DateTime.Now;
			var startTime = endTime.AddHours(-8);

			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee>());

			var availableSlot = meetingCalendar.FindFirstAvailableSlot(1);
			Assert.That(availableSlot, Is.Null);
		}

		[Test]
		public void FindFirstAvailableSlot_Returns_TimeSlot_Within_Given_Time_Range()
		{
			var startTime = DateTime.Now.CalibrateToMinutes();
			var endTime = startTime.AddHours(8);
			var secondMeetingStartTime = startTime.AddHours(2.5);
			var secondMeetingEndTime = startTime.AddHours(3.5);

			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(startTime.AddHours(1), startTime.AddHours(2)),
					new MeetingInfo(secondMeetingStartTime, secondMeetingEndTime),
					//Here is the start time of available slot
					new MeetingInfo(startTime.AddHours(7), startTime.AddHours(7.5))
				})
			});

			var availableSlot = meetingCalendar.FindFirstAvailableSlot(15, secondMeetingStartTime);
			Assert.That(availableSlot, Is.Not.Null);
			Assert.That(availableSlot.StartTime, Is.GreaterThanOrEqualTo(secondMeetingEndTime));
		}

		[Test]
		public void FindFirstAvailableSlot_Returns_TimeSlot_Within_A_Given_Search_Range()
		{
			var startTime = DateTime.Now.CalibrateToMinutes();
			var endTime = startTime.AddHours(8);
			var secondMeetingStartTime = startTime.AddHours(2.5);
			var secondMeetingEndTime = startTime.AddHours(3.5);

			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(startTime.AddHours(1), startTime.AddHours(2)),
					new MeetingInfo(secondMeetingStartTime, secondMeetingEndTime),
					//Here is the start time of available slot
					new MeetingInfo(startTime.AddHours(7), startTime.AddHours(7.5))
				})
			});

			var searchRange = new TimeSlot(secondMeetingStartTime, secondMeetingStartTime.AddHours(2));
			var availableSlot = meetingCalendar.FindFirstAvailableSlot(15, searchRange);
			Assert.That(availableSlot, Is.Not.Null);
			Assert.That(availableSlot.StartTime, Is.GreaterThanOrEqualTo(secondMeetingEndTime));
		}

		[Test]
		public void
			FindFirstAvailableSlot_Returns_TimeSlot_Equals_To_Calendar_TimeFrame_When_Search_Range_Longer_Than_Calendar_TimeFrame()
		{
			var startTime = DateTime.Now;
			var endTime = startTime.AddHours(4);
			var meetingCalendar = new Calendar(startTime, endTime);

			const int meetingDuration = 120;
			var availableSlot =
				meetingCalendar.FindFirstAvailableSlot(meetingDuration, startTime.AddHours(-1), endTime.AddHours(1));

			Assert.That(availableSlot, Is.Not.Null);
			Assert.That(availableSlot.GetDuration(), Is.GreaterThanOrEqualTo(meetingDuration));
			Assert.That(availableSlot.StartTime, Is.EqualTo(meetingCalendar.StartTime));
			Assert.That(availableSlot.EndTime, Is.LessThanOrEqualTo(meetingCalendar.EndTime));
		}

		[Test]
		public void
			FindFirstAvailableSlot_Returns_TimeSlot_Equals_To_Search_Range_When_Range_Shorter_Than_Calendar_TimeFrame()
		{
			var startTime = DateTime.Now.CalibrateToMinutes();
			var endTime = startTime.AddHours(4).CalibrateToMinutes();
			var meetingCalendar = new Calendar(startTime, endTime);

			var searchRangeStartTime = startTime.AddHours(1.5);
			var searchEndTime = startTime.AddHours(3.75);

			const int meetingDuration = 60;
			var availableSlot =
				meetingCalendar.FindFirstAvailableSlot(meetingDuration, searchRangeStartTime, searchEndTime);

			Assert.That(availableSlot, Is.Not.Null);
			Assert.That(availableSlot.GetDuration(), Is.GreaterThanOrEqualTo(meetingDuration));
			Assert.That(availableSlot.StartTime, Is.EqualTo(searchRangeStartTime));
			Assert.That(availableSlot.EndTime, Is.EqualTo(searchEndTime));
		}

		[Test]
		public void
			FindFirstAvailableSlot_Returns_TimeSlot_Having_EndTime_Shorter_Than_Calendar_EndTime_When_SearchRange_UB_Longer_Than_Calendar_TimeFrame()
		{
			var startTime = DateTime.Now.CalibrateToMinutes();
			var endTime = startTime.AddHours(4).CalibrateToMinutes();
			var firstMeetingEndTime = startTime.AddHours(2);
			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(startTime, firstMeetingEndTime),
				})
			});

			var searchRangeStartTime = startTime.AddHours(1.5);
			var searchEndTime = endTime.AddMinutes(-15);

			const int meetingDuration = 60;
			var availableSlot =
				meetingCalendar.FindFirstAvailableSlot(meetingDuration, searchRangeStartTime, searchEndTime);

			Assert.That(availableSlot, Is.Not.Null);
			Assert.That(availableSlot.GetDuration(), Is.GreaterThanOrEqualTo(meetingDuration));
			Assert.That(availableSlot.StartTime, Is.EqualTo(firstMeetingEndTime));
			Assert.That(availableSlot.EndTime, Is.EqualTo(searchEndTime));
		}

		[Test]
		public void
			FindFirstAvailableSlot_Returns_TimeSlot_StartTime_Equals_To_Search_Range_Starting_Time_When_SearchRange_Is_Longer()
		{
			var startTime = DateTime.Now.CalibrateToMinutes();
			var endTime = startTime.AddHours(4).CalibrateToMinutes();
			var meetingCalendar = new Calendar(startTime, endTime);

			var searchRangeStartTime = startTime.AddHours(1.5);

			const int meetingDuration = 120;
			var availableSlot =
				meetingCalendar.FindFirstAvailableSlot(meetingDuration, searchRangeStartTime, endTime.AddHours(1));

			Assert.That(availableSlot, Is.Not.Null);
			Assert.That(availableSlot.GetDuration(), Is.GreaterThanOrEqualTo(meetingDuration));
			Assert.That(availableSlot.StartTime, Is.EqualTo(searchRangeStartTime));
			Assert.That(availableSlot.EndTime, Is.LessThanOrEqualTo(meetingCalendar.EndTime));
		}

		[Test]
		public void GetFirstAvailableSlot_Returns_MeetingSlot_When_No_Meetings_Scheduled_Within_Allowed_MeetingHours()
		{
			var startTime = DateTime.Now.CalibrateToMinutes();
			var endTime = startTime.AddHours(8);

			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee>
			{
				new Attendee("Person10", new List<IMeetingInfo>
				{
					new MeetingInfo(startTime.AddHours(-2), startTime.AddHours(-1)),
					new MeetingInfo(endTime.AddHours(1), endTime.AddHours(2))
				})
			});

			var availableSlot = meetingCalendar.FindFirstAvailableSlot(60);

			Assert.That(availableSlot.GetDuration(), Is.GreaterThanOrEqualTo(60));
			Assert.That(availableSlot.StartTime, Is.GreaterThanOrEqualTo(startTime));
		}

		[Test]
		public void AvailableTimeSlotShouldBeGreaterThanEqualsToCurrentTime()
		{
			var now = DateTime.Now;
			var meetingCalendar = new Calendar(now.AddHours(-1), now.AddHours(1));

			meetingCalendar.AddAttendees(new List<IAttendee>());
			var firstAvailableTimeSlot = meetingCalendar.FindFirstAvailableSlot(10);

			Assert.That(firstAvailableTimeSlot.StartTime, Is.GreaterThanOrEqualTo(now.CalibrateToMinutes()));
		}

		[Test]
		public void GetFirstAvailableSlot_Includes_Meeting_Duration_When_Not_Over()
		{
			var startTime = DateTime.Now.AddHours(-4);
			var endTime = startTime.AddHours(8);

			var meetingStartTime = DateTime.Now.AddMinutes(-15);
			var meetingEndTime = DateTime.Now.AddMinutes(15);

			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingEndTime)
				})
			});

			var availableSlot = meetingCalendar.FindFirstAvailableSlot(10);

			Assert.That(availableSlot.StartTime, Is.GreaterThanOrEqualTo(meetingEndTime.CalibrateToMinutes()));
		}

		[Test]
		public void GetFirstAvailableSlot_When_Attendees_Count_Greater_Than_Eight()
		{
			var startTime = DateTime.Now;
			var endTime = startTime.AddHours(17);

			var meetingStartTime = DateTime.Now.AddMinutes(-15);
			var meetingEndTime = DateTime.Now.AddMinutes(15);

			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingEndTime)
				}),
				new Attendee("Person2", new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingEndTime)
				}),
				new Attendee("Person3", new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingEndTime)
				}),
				new Attendee("Person4", new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingEndTime)
				}),
				new Attendee("Person5", new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingEndTime)
				}),
				new Attendee("Person6", new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingEndTime)
				}),
				new Attendee("Person7", new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingEndTime)
				}),
				new Attendee("Person8", new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingEndTime)
				}),
				new Attendee("Person9", new List<IMeetingInfo>
				{
					new MeetingInfo(endTime.AddMinutes(-15), endTime.AddMinutes(15))
				}),
				new Attendee("Person10", new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingStartTime.AddMinutes(10))
				}),
				new Attendee("Person11", "test@email.com", "1234567890", true, new List<IMeetingInfo>
				{
					new MeetingInfo(meetingStartTime, meetingEndTime)
				})
			});

			var availableSlot = meetingCalendar.FindFirstAvailableSlot(10);

			Assert.That(availableSlot.StartTime, Is.GreaterThanOrEqualTo(meetingEndTime.CalibrateToMinutes()));
		}

		[Test]
		public void RemoveAttendee_Returns_True_When_Attendee_Removed_Successfully()
		{
			var startTime = DateTime.Now;
			var endTime = startTime.AddHours(8);

			var attendee = new Attendee("Person1", new List<IMeetingInfo>
			{
				new MeetingInfo(startTime.AddHours(-2), startTime.AddHours(-1)),
				new MeetingInfo(endTime.AddHours(1), endTime.AddHours(2))
			});

			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee> { attendee });

			var isRemoved = meetingCalendar.RemoveAttendee(attendee);

			Assert.That(meetingCalendar.Attendees.Count, Is.Zero);
			Assert.That(isRemoved, Is.True);
		}

		[Test]
		public void RemoveAttendee_Returns_False_When_No_Matching_Attendee_Found()
		{
			var startTime = DateTime.Now;
			var endTime = startTime.AddHours(8);
			var meetings = new List<IMeetingInfo>
			{
				new MeetingInfo(startTime.AddHours(-2), startTime.AddHours(-1)),
				new MeetingInfo(endTime.AddHours(1), endTime.AddHours(2))
			};

			var attendee = new Attendee("Person1", meetings);
			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee> { attendee });

			var attendeeCopy = new Attendee("Person1", meetings);
			var attendeeId = Guid.NewGuid();
			attendeeCopy.AttendeeId = attendeeId;

			var isRemoved = meetingCalendar.RemoveAttendee(attendeeCopy);

			var attendeeFound = meetingCalendar.Attendees.FirstOrDefault(a => a.AttendeeId == attendeeId);

			Assert.That(attendeeFound, Is.Null);
			Assert.That(meetingCalendar.Attendees.Count, Is.Not.Zero);
			Assert.That(isRemoved, Is.False);
		}

		[Test]
		public void RemoveAttendee_By_Id_Returns_True_When_Matching_Attendee_Removed()
		{
			var startTime = DateTime.Now;
			var endTime = startTime.AddHours(8);
			var meetings = new List<IMeetingInfo>
			{
				new MeetingInfo(startTime.AddHours(-2), startTime.AddHours(-1)),
				new MeetingInfo(endTime.AddHours(1), endTime.AddHours(2))
			};
			var attendeeId = Guid.NewGuid();
			var attendee = new Attendee("Person1", meetings) { AttendeeId = attendeeId };
			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee> { attendee });

			var attendeeFound = meetingCalendar.Attendees.FirstOrDefault(a => a.AttendeeId == attendeeId);

			var isRemoved = meetingCalendar.RemoveAttendee(attendeeId);

			Assert.That(attendeeFound, Is.Not.Null);
			Assert.That(meetingCalendar.Attendees.Count, Is.Zero);
			Assert.That(isRemoved, Is.True);
		}

		[Test]
		public void RemoveAttendee_By_Id_Returns_False_When_No_Matching_Attendee_Found()
		{
			var startTime = DateTime.Now;
			var endTime = startTime.AddHours(8);
			var meetings = new List<IMeetingInfo>
			{
				new MeetingInfo(startTime.AddHours(-2), startTime.AddHours(-1)),
				new MeetingInfo(endTime.AddHours(1), endTime.AddHours(2))
			};
			var attendeeId = Guid.NewGuid();
			var attendee = new Attendee("Person1", meetings) { AttendeeId = attendeeId };
			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee> { attendee });

			var anotherAttendeeId = Guid.NewGuid();
			var attendeeFound = meetingCalendar.Attendees.FirstOrDefault(a => a.AttendeeId == anotherAttendeeId);

			var isRemoved = meetingCalendar.RemoveAttendee(anotherAttendeeId);

			Assert.That(attendeeFound, Is.Null);
			Assert.That(meetingCalendar.Attendees.Count, Is.Not.Zero);
			Assert.That(isRemoved, Is.False);
		}

		[Test]
		public void RemoveAttendee_Using_Name_And_EmailId_Returns_True_When_Matching_Attendee_Removed()
		{
			var startTime = DateTime.Now;
			var endTime = startTime.AddHours(8);
			var meetings = new List<IMeetingInfo>
			{
				new MeetingInfo(startTime.AddHours(-2), startTime.AddHours(-1)),
				new MeetingInfo(endTime.AddHours(1), endTime.AddHours(2))
			};
			const string attendeeName = "John Doe";
			const string attendeeEmailId = "JohnDoe@test.com";

			var attendee = new Attendee(attendeeName, attendeeEmailId, "", false, meetings);
			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee> { attendee });

			var attendeeCopy = new Attendee(attendeeName, attendeeEmailId, "", false, meetings);

			var attendeeFound = meetingCalendar.Attendees.FirstOrDefault(a =>
				a.AttendeeName == attendeeName && a.AttendeeEmailId == attendeeEmailId);

			var isRemoved = meetingCalendar.RemoveAttendee(attendeeCopy.AttendeeName, attendeeCopy.AttendeeEmailId);

			Assert.That(attendeeFound, Is.Not.Null);
			Assert.That(meetingCalendar.Attendees.Count, Is.Zero);
			Assert.That(isRemoved, Is.True);
		}

		[Test]
		public void RemoveAttendee_Using_Name_EmailId_Returns_False_When_No_Matching_Attendee_Found()
		{
			var startTime = DateTime.Now;
			var endTime = startTime.AddHours(8);
			var meetings = new List<IMeetingInfo>
			{
				new MeetingInfo(startTime.AddHours(-2), startTime.AddHours(-1)),
				new MeetingInfo(endTime.AddHours(1), endTime.AddHours(2))
			};
			const string attendeeName = "John Doe";
			const string attendeeEmailId = "JohnDoe@test.com";
			const string anotherAttendeeEmailId = "John_Doe@test.com";

			var attendee = new Attendee(attendeeName, attendeeEmailId, "", false, meetings);
			var meetingCalendar = new Calendar(startTime, endTime, new List<IAttendee> { attendee });

			var anotherAttendee = new Attendee(attendeeName, anotherAttendeeEmailId, "", false, meetings);
			var attendeeFound = meetingCalendar.Attendees.FirstOrDefault(a =>
				a.AttendeeName == attendeeName && a.AttendeeEmailId == anotherAttendeeEmailId);

			var isRemoved = meetingCalendar.RemoveAttendee(anotherAttendee.AttendeeName, anotherAttendee.AttendeeEmailId);

			Assert.That(attendeeFound, Is.Null);
			Assert.That(meetingCalendar.Attendees.Count, Is.Not.Zero);
			Assert.That(isRemoved, Is.False);
		}

		[Test]
		public void MoveForwardTest()
		{
			var calendarEndTime = DateTime.Now.AddHours(8);
			var meetingCalendar = new Calendar(DateTime.Now, calendarEndTime, new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now, DateTime.Now.AddHours(2))
				})
			});

			meetingCalendar.MoveForward();

			Assert.That(meetingCalendar.Attendees.Count, Is.Zero);
			Assert.That(meetingCalendar.StartTime, Is.EqualTo(calendarEndTime.CalibrateToMinutes()));
			Assert.That(meetingCalendar.EndTime, Is.EqualTo(calendarEndTime.CalibrateToMinutes().AddMinutes(meetingCalendar.GetDuration())));
		}

		[Test]
		public void MoveForward_With_Retaining_Attendees()
		{
			var calendarEndTime = DateTime.Now.AddHours(8);
			var meetingCalendar = new Calendar(DateTime.Now, calendarEndTime, new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now, DateTime.Now.AddHours(2))
				})
			});

			meetingCalendar.MoveForward(false);

			Assert.That(meetingCalendar.Attendees.Count, Is.Not.Zero);
			Assert.That(meetingCalendar.StartTime, Is.EqualTo(calendarEndTime.CalibrateToMinutes()));
			Assert.That(meetingCalendar.EndTime, Is.EqualTo(calendarEndTime.CalibrateToMinutes().AddMinutes(meetingCalendar.GetDuration())));
		}
		
		[Test]
		public void MoveForward_With_Attendees()
		{
			var calendarEndTime = DateTime.Now.AddHours(8);
			var meetingCalendar = new Calendar(DateTime.Now, calendarEndTime, new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now, DateTime.Now.AddHours(2))
				})
			});

			var newAttendees = new List<IAttendee>
			{
				new Attendee("Person2", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now, DateTime.Now.AddHours(2))
				})
			};

			meetingCalendar.MoveForward(newAttendees);

			Assert.That(meetingCalendar.Attendees.Count, Is.EqualTo(newAttendees.Count));
			Assert.That(meetingCalendar.Attendees.First(), Is.EqualTo(newAttendees.First()));
			Assert.That(meetingCalendar.StartTime, Is.EqualTo(calendarEndTime.CalibrateToMinutes()));
			Assert.That(meetingCalendar.EndTime, Is.EqualTo(calendarEndTime.CalibrateToMinutes().AddMinutes(meetingCalendar.CalendarWindowInMinutes)));
		}

		[Test]
		public void MoveBackwardNext()
		{
			var calendarStartTime = DateTime.Now;
			var meetingCalendar = new Calendar(calendarStartTime, DateTime.Now.AddHours(8), new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now, DateTime.Now.AddHours(2))
				})
			});

			meetingCalendar.MoveBackward();

			Assert.That(meetingCalendar.Attendees.Count, Is.Zero);
			Assert.That(meetingCalendar.StartTime, Is.EqualTo(calendarStartTime.CalibrateToMinutes().AddMinutes(-1 * meetingCalendar.GetDuration())));
			Assert.That(meetingCalendar.EndTime, Is.EqualTo(calendarStartTime.CalibrateToMinutes()));
		}

		[Test]
		public void MoveBackward_With_Retaining_Attendees()
		{
			var calendarStartTime = DateTime.Now;
			var meetingCalendar = new Calendar(calendarStartTime, DateTime.Now.AddHours(8), new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now, DateTime.Now.AddHours(2))
				})
			});

			meetingCalendar.MoveBackward(false);

			Assert.That(meetingCalendar.Attendees.Count, Is.Not.Zero);
			Assert.That(meetingCalendar.StartTime, Is.EqualTo(calendarStartTime.CalibrateToMinutes().AddMinutes(-1 * meetingCalendar.CalendarWindowInMinutes)));
			Assert.That(meetingCalendar.EndTime, Is.EqualTo(calendarStartTime.CalibrateToMinutes()));
		}

		[Test]
		public void MoveBackward_With_Attendees()
		{
			var calendarStartTime = DateTime.Now;
			var meetingCalendar = new Calendar(calendarStartTime, DateTime.Now.AddHours(8), new List<IAttendee>
			{
				new Attendee("Person1", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now, DateTime.Now.AddHours(2))
				})
			});

			var newAttendees = new List<IAttendee>
			{
				new Attendee("Person2", new List<IMeetingInfo>
				{
					new MeetingInfo(DateTime.Now, DateTime.Now.AddHours(2))
				})
			};

			meetingCalendar.MoveBackward(newAttendees);

			Assert.That(meetingCalendar.Attendees.Count, Is.EqualTo(newAttendees.Count));
			Assert.That(meetingCalendar.Attendees.First(), Is.EqualTo(newAttendees.First()));
			Assert.That(meetingCalendar.StartTime, Is.EqualTo(calendarStartTime.CalibrateToMinutes().AddMinutes(-1 * meetingCalendar.GetDuration())));
			Assert.That(meetingCalendar.EndTime, Is.EqualTo(calendarStartTime.CalibrateToMinutes()));
		}

		[Test]
		public void Deconstruct_Using_Name_EmailId_Returns_False_When_No_Matching_Attendee_Found()
		{
			var calendarStartTime = DateTime.Now;
			var calendarEndTime = calendarStartTime.AddHours(8);
			var meetings = new List<IMeetingInfo>
			{
				new MeetingInfo(calendarStartTime.AddHours(-2), calendarStartTime.AddHours(-1)),
				new MeetingInfo(calendarEndTime.AddHours(1), calendarEndTime.AddHours(2))
			};
			const string attendeeName = "John Doe";
			const string attendeeEmailId = "JohnDoe@test.com";
			var calendarAttendees = new List<IAttendee> {  new Attendee(attendeeName, attendeeEmailId, "", false, meetings)};
			var meetingCalendar = new Calendar(calendarStartTime, calendarEndTime, calendarAttendees );

			var (startTime, endTime, currentTime, calendarWindowInMinutes, attendees) = meetingCalendar;

			Assert.That(currentTime, Is.EqualTo(DateTime.Now.CalibrateToMinutes()));
			Assert.That(startTime, Is.EqualTo(calendarStartTime.CalibrateToMinutes()));
			Assert.That(endTime, Is.EqualTo(calendarEndTime.CalibrateToMinutes()));
			Assert.That(calendarWindowInMinutes, Is.EqualTo(calendarEndTime.Subtract(calendarStartTime).TotalMinutes));
			Assert.That(attendees.Count, Is.EqualTo(calendarAttendees.Count));
			Assert.That(attendees.First().Equals(calendarAttendees.First()), Is.True);
			Assert.That(attendees.First().Meetings.Count, Is.EqualTo(calendarAttendees.First().Meetings.Count));
		}
	}
}