/*
 * Author: Antaryami Basuri
 * Email: a.basuri2002@gmail.com
 */

using System;

namespace MeetingCalendar.Interfaces
{
	/// <summary>
	/// Interface for <see cref="IMeetingInfo"/>.
	/// </summary>
	public interface IMeetingInfo : ITimeSlot
	{
		/// <summary>
		/// Gets or sets Id of the <see cref="IMeetingInfo"/>.
		/// </summary>
		Guid MeetingId { get; set; }
	}
}