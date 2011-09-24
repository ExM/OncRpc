using System;

namespace Xdr.Example
{
	/// <summary>
	/// Complete file structure.
	/// http://tools.ietf.org/html/rfc4506#section-7
	/// </summary>
	public partial class CompleteFile
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
		[Field(0), Var(MaxNameLen)]
		public string FileName {get; set;}
		
		/// <summary>
		/// info about file
		/// </summary>
		[Field(1)]
		public FileType Type {get; set;}
		
		/// <summary>
		/// owner of file
		/// </summary>
		[Field(2), Var(MaxUserName)]
		public string Owner {get; set;}
		
		/// <summary>
		/// file data
		/// </summary>
		[Field(3), Var(MaxFileLen)]
		public byte[] Data {get; set;}
		
		public static void Read(Reader reader, Action<CompleteFile> completed, Action<Exception> excepted)
		{
			new ReadContext(reader, completed, excepted);
		}
		
		public static void Write(Writer writer, CompleteFile item, Action completed, Action<Exception> excepted)
		{
			new WriteContext(writer, item, completed, excepted);
		}
	}
}

