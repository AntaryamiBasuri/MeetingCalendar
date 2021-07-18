/*
 * Author: Antaryami Basuri
 * Email: a.basuri2002@gmail.com
 */

using System;
using System.Collections.Generic;

namespace MeetingCalendar.Interfaces
{
	/// <summary>
	/// Interface for <see cref="IAttendee"/>.
	/// </summary>
	public interface IAttendee
	{
		/// <summary>
		/// Gets or sets the Id of the <see cref="IAttendee"/>.
		/// </summary>
		Guid AttendeeId { get; set; }

		/// <summary>
		/// Gets the name of the <see cref="IAttendee"/>.
		/// </summary>
		string AttendeeName { get; }

		/// <summary>
		/// Gets the email id of the <see cref="IAttendee"/>.
		/// </summary>
		string AttendeeEmailId { get; }

		/// <summary>
		/// Gets the mobile number or phone number of the <see cref="IAttendee"/>.
		/// </summary>
		string PhoneNumber { get; }

		/// <summary>
		/// Gets a value indicating whether the <see cref="IAttendee"/> as an optional  or mandatory.
		/// </summary>
		bool IsOptionalAttendee { get; }

		/// <summary>
		/// Gets a list of <see cref="IMeetingInfo"/> associated with the <see cref="IAttendee"/>.
		/// </summary>
		IReadOnlyCollection<IMeetingInfo> Meetings { get; }
	}
}