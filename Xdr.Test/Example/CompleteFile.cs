using System;

namespace Xdr.Example
{
	/// <summary>
	/// Complete file structure.
	/// http://tools.ietf.org/html/rfc4506#section-7
	/// </summary>
	public class CompleteFile
	{
		/// <summary>
		/// max length of a user name
		/// </summary>
		public const int MaxUserName = 32;
		
		/// <summary>
		/// max length of a file
		/// </summary>
		public const int MaxFileLen = 65535;
		
		/// <summary>
		/// max length of a file name
		/// </summary>
		public const int MaxNameLen = 255;
		
		/// <summary>
		/// name of file
		/// </summary>
		[XdrField(Order = 0, MaxLength = MaxNameLen)]
		public string FileName {get; set;}
		
		/// <summary>
		/// info about file
		/// </summary>
		[XdrUnion(Order = 1)]
		[XdrCase(FileKind.Text)] // no extra information
		public FileKind Type {get; set;}
		
		/// <summary>
		/// data creator
		/// </summary>
		[XdrCase("Type", FileKind.Data)]
		[XdrField(MaxLength = MaxNameLen)]
		public string Creator {get; set;}
		
		/// <summary>
		/// data creator
		/// </summary>
		[XdrCase("Type", FileKind.Exec)]
		[XdrField(MaxLength = MaxNameLen)]
		public string Interpretor {get; set;}
		
		/// <summary>
		/// owner of file
		/// </summary>
		[XdrField(Order = 2, MaxLength = MaxUserName)]
		public string Owner {get; set;}
		
		/// <summary>
		/// file data
		/// </summary>
		[XdrField(Order = 3, MaxLength = MaxFileLen)]
		public byte[] Data {get; set;}
	}
}

