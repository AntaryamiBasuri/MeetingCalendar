/*
 * Author: Antaryami Basuri
 * Email: a.basuri2002@gmail.com
 */

using MeetingCalendar.Extensions;
using MeetingCalendar.Models;
using NUnit.Framework;
using System;

namespace MeetingCalendarTest.Extensions
{
	[TestFixture]
	[Author("A Basuri", "a.basuri2002@gmail.com")]
	public class MeetingInfoExtensionsTests
	{
		[Test]
		public void IsOver_Returns_True_When_Meeting_EndTime_Is_Less_Than_CurrentTime()
			=> Assert.That(new MeetingInfo(DateTime.Now.AddHours(-2), DateTime.Now.AddHours(-1)).IsOver(), Is.True);

		[Test]
		public void AvailableDuration_Is_GreaterThan_Zero_When_EndTime_Is_Greater_Than_StartTime()
			=> Assert.That(new MeetingInfo(DateTime.Now.AddHours(-1), DateTime.Now.AddHours(1)).IsOver(), Is.False);

		[Test]
		public void GetTimeSlotMappedToCalenderTimeFrame_Returns_A_New_TimeSlot_Mapped_To_Calendar_Time()
		{
			//TODO: Use Calender object and use calendar properties and remove the call to CalibrateToMinutes method

			var calendarStartTime = DateTime.Now.AddHours(1).CalibrateToMinutes();
			var calendarEndTime = DateTime.Now.AddHours(3).CalibrateToMinutes();
			//var calendar = new Calendar(DateTime.Now.AddHours(-1), DateTime.Now.AddHours(2));

			var meeting1 = new MeetingInfo(DateTime.Now.AddMinutes(75), DateTime.Now.AddMinutes(105));  //T	T
			var meeting2 = new MeetingInfo(DateTime.Now.AddMinutes(75), DateTime.Now.AddHours(4));      //T	F
			var meeting3 = new MeetingInfo(DateTime.Now.AddHours(-1), DateTime.Now.AddMinutes(105));    //F	T
			var meeting4 = new MeetingInfo(DateTime.Now.AddHours(-1), DateTime.Now.AddHours(4));        //F	F

			var mappedTime = meeting1.GetTimeSlotMappedToCalenderTimeFrame(calendarStartTime, calendarEndTime);
			Assert.That(mappedTime.StartTime, Is.EqualTo(meeting1.StartTime));
			Assert.That(mappedTime.EndTime, Is.EqualTo(meeting1.EndTime));

			mappedTime = meeting2.GetTimeSlotMappedToCalenderTimeFrame(calendarStartTime, calendarEndTime);
			Assert.That(mappedTime.StartTime, Is.EqualTo(meeting2.StartTime));
			Assert.That(mappedTime.EndTime, Is.EqualTo(calendarEndTime));

			mappedTime = meeting3.GetTimeSlotMappedToCalenderTimeFrame(calendarStartTime, calendarEndTime);
			Assert.That(mappedTime.StartTime, Is.EqualTo(calendarStartTime));
			Assert.That(mappedTime.EndTime, Is.EqualTo(meeting3.EndTime));

			mappedTime = meeting4.GetTimeSlotMappedToCalenderTimeFrame(calendarStartTime, calendarEndTime);
			Assert.That(mappedTime.StartTime, Is.EqualTo(calendarStartTime));
			Assert.That(mappedTime.EndTime, Is.EqualTo(calendarEndTime));
		}

		[Test]
		public void GetTimeSlotMappedToCalenderTimeFrame_Returns_Null_When_TimeSlot_Is_Outside_Of_Calendar_TimeFrame()
		{
			//TODO: Use Calender object and use calendar properties and remove the call to CalibrateToMinutes method

			var calendarStartTime = DateTime.Now.AddHours(2).CalibrateToMinutes();
			var calendarEndTime = DateTime.Now.AddHours(6).CalibrateToMinutes();

			var meeting1 = new MeetingInfo(DateTime.Now.AddHours(1), DateTime.Now.AddHours(2));
			var meeting2 = new MeetingInfo(DateTime.Now.AddHours(7), DateTime.Now.AddHours(8));

			var mappedTime = meeting1.GetTimeSlotMappedToCalenderTimeFrame(calendarStartTime, calendarEndTime);
			Assert.That(mappedTime, Is.Null);
			mappedTime = meeting2.GetTimeSlotMappedToCalenderTimeFrame(calendarStartTime, calendarEndTime);
			Assert.That(mappedTime, Is.Null);
		}
	}
}