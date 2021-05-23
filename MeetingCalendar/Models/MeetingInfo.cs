﻿/*
 * Author: Antaryami Basuri
 * Email: a.basuri2002@gmail.com
 */

using System;

namespace MeetingCalendar.Models
{
	/// <summary>
	/// Provides information about a <see cref="MeetingInfo"/> with a specific start time and end time.
	/// </summary>
	public class MeetingInfo : TimeSlot
	{
		/// <summary>
		/// Gets or sets Id of the <see cref="MeetingInfo"/>.
		/// </summary>
		public Guid MeetingId { get; set; }

		/// <summary>
		/// Initializes a new instance of <see cref="MeetingInfo"/>
		/// </summary>
		/// <param name="startTime">The meeting start time.</param>
		/// <param name="endTime">The meeting end time.</param>
		public MeetingInfo(DateTime startTime, DateTime endTime)
			: base(startTime, endTime)
		{
		}
	}
}