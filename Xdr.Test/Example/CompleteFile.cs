using System;

namespace Xdr2.Example
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
		[Order(0), Var(MaxNameLen)]
		public string FileName {get; set;}
		
		/// <summary>
		/// info about file
		/// </summary>
		[Order(1)]
		public FileType Type {get; set;}
		
		/// <summary>
		/// owner of file
		/// </summary>
		[Order(2), Var(MaxUserName)]
		public string Owner {get; set;}
		
		/// <summary>
		/// file data
		/// </summary>
		[Order(3), Var(MaxFileLen)]
		public byte[] Data {get; set;}

		public static CompleteFile Read(Reader reader)
		{
			CompleteFile result = new CompleteFile();
			result.FileName = reader.ReadVar<string>(MaxNameLen);
			result.Type = reader.Read<FileType>();
			result.Owner = reader.ReadVar<string>(MaxUserName);
			result.Data = reader.ReadVar<byte[]>(MaxFileLen);
			return result;
		}
		
		public static void Write(Writer writer, CompleteFile item)
		{
			writer.WriteVar<string>(MaxNameLen, item.FileName);
			writer.Write<FileType>(item.Type);
			writer.WriteVar<string>(MaxUserName, item.Owner);
			writer.WriteVar<byte[]>(MaxFileLen, item.Data);
		}
	}
}

