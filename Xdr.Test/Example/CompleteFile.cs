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
		[XdrField(0), VarLength(MaxNameLen)]
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
		[VarLength(MaxNameLen)]
		public string Creator {get; set;}
		
		/// <summary>
		/// data creator
		/// </summary>
		[XdrCase("Type", FileKind.Exec)]
		[VarLength(MaxNameLen)]
		public string Interpretor {get; set;}
		
		/// <summary>
		/// owner of file
		/// </summary>
		[XdrField(2), VarLength(MaxUserName)]
		public string Owner {get; set;}
		
		/// <summary>
		/// file data
		/// </summary>
		[XdrField(3), VarLength(MaxFileLen)]
		public byte[] Data {get; set;}
	}
}

