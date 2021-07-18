/*
 * Author: Antaryami Basuri
 * Email: a.basuri2002@gmail.com
 */

using System.Collections.Generic;

namespace MeetingCalendar.Interfaces
{
	/// <summary>
	/// Interface for <see cref="IMeetingDetails"/>.
	/// </summary>
	public interface IMeetingDetails : IMeetingInfo
	{
		/// <summary>
		/// Gets the title of the <see cref="IMeetingDetails"/>.
		/// </summary>
		string MeetingTitle { get; }

		/// <summary>
		/// Gets the agenda of the <see cref="IMeetingDetails"/>.
		/// </summary>
		string MeetingAgenda { get; }

		/// <summary>
		/// Gets the location/venue of the <see cref="IMeetingDetails"/>.
		/// </summary>
		string MeetingLocation { get; }

		/// <summary>
		/// Gets a list of Attendees associated with the <see cref="IMeetingDetails"/>.
		/// </summary>
		IReadOnlyCollection<IAttendee> Attendees { get; }

		/// <summary>
		/// Gets or sets a list of file paths as Attachment associated with the <see cref="IMeetingDetails"/>.
		/// </summary>
		IList<string> AttachmentFilePaths { get; set; }
	}
}