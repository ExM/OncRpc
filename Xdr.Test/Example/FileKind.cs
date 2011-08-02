using System;

namespace Xdr.Example
{
	/// <summary>
	/// Types of files
	/// http://tools.ietf.org/html/rfc4506#section-7
	/// </summary>
	public enum FileKind
	{
		/// <summary>
		/// ascii data
		/// </summary>
		Text = 0,
		
		/// <summary>
		/// raw data
		/// </summary>
		Data = 1,
		
		/// <summary>
		/// executable
		/// </summary>
		Exec = 2
	}
}

