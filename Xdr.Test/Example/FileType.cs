using System;

namespace Xdr.Example
{
	/// <summary>
	/// union types of files
	/// http://tools.ietf.org/html/rfc4506#section-7
	/// </summary>
	public class FileType
	{
		/// <summary>
		/// max length of a file name
		/// </summary>
		public const int MaxNameLen = 255;
		
		/// <summary>
		/// info about file
		/// </summary>
		[Switch, Case(FileKind.Text)] // no extra information
		public FileKind Type {get; set;}
		
		/// <summary>
		/// data creator
		/// </summary>
		[Case(FileKind.Data), Var(MaxNameLen)]
		public string Creator {get; set;}
		
		/// <summary>
		/// data creator
		/// </summary>
		[Case(FileKind.Exec), Var(MaxNameLen)]
		public string Interpretor {get; set;}
	}
}

