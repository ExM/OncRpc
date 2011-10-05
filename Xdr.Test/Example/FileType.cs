using System;

namespace Xdr.Example
{
	/// <summary>
	/// union types of files
	/// http://tools.ietf.org/html/rfc4506#section-7
	/// </summary>
	public partial class FileType
	{
		/// <summary>
		/// max length of a file name
		/// </summary>
		public const int MaxNameLen = 255;
		
		/// <summary>
		/// info about file
		/// </summary>
		[Switch]
		[Case(FileKind.Text)] // no extra information
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

		public static FileType Read(Reader reader)
		{
			FileType result = new FileType();
			result.Type = reader.Read<FileKind>();
			switch (result.Type)
			{
				case FileKind.Text:
					break;
				case FileKind.Data:
					result.Creator = reader.ReadVar<string>(MaxNameLen);
					break;
				case FileKind.Exec:
					result.Interpretor = reader.ReadVar<string>(MaxNameLen);
					break;
				default:
					throw new InvalidOperationException("unexpected value");
			}

			return result;
		}
		
		public static void Write(Writer writer, FileType item)
		{
			switch (item.Type)
			{
				case FileKind.Text:
					writer.Write<FileKind>(FileKind.Text);
					return;
				case FileKind.Data:
					writer.Write<FileKind>(FileKind.Data);
					writer.WriteVar<string>(MaxNameLen, item.Creator);
					return;
				case FileKind.Exec:
					writer.Write<FileKind>(FileKind.Exec);
					writer.WriteVar<string>(MaxNameLen, item.Interpretor);
					return;
				default:
					throw new InvalidOperationException("unexpected value");
			}
		}
	}
}

