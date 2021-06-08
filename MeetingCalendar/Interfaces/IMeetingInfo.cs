﻿using System;

namespace MeetingCalendar.Interfaces
{
	/// <summary>
	/// Interface for <see cref="IMeetingInfo"/>
	/// </summary>
	public interface IMeetingInfo : ITimeSlot
	{
		/// <summary>
		/// Gets or sets Id of the <see cref="IMeetingInfo"/>.
		/// </summary>
		public Guid MeetingId { get; set; }
	}
}