using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EmitTest
{
	public class TreeNode_ReadContext
	{
		private TreeNode _target;
		private IByteReader _reader;
		private Action<TreeNode> _completed;
		private Action<Exception> _excepted;

		public TreeNode_ReadContext(IByteReader reader, Action<TreeNode> completed, Action<Exception> excepted)
		{
			_target = new TreeNode();
			_reader = reader;
			_completed = completed;
			_excepted = excepted;

			_reader.Read(4, Field1_Readed, _excepted);
		}

		private void Field1_Readed(byte[] val)
		{
			_target.Field1 = XdrEncoding.DecodeInt32(val);
			_reader.Read(4, Field2_Readed, _excepted);
		}

		private void Field2_Readed(byte[] val)
		{
			_target.Field2 = XdrEncoding.DecodeInt32(val);
			_reader.Read(4, Field3_Readed, _excepted);
		}

		private void Field3_Readed(byte[] val)
		{
			_target.Field3 = XdrEncoding.DecodeInt32(val);
			_completed(_target);
		}
		
		public static void Read(IByteReader reader, Action<TreeNode> completed, Action<Exception> excepted)
		{
			new TreeNode_ReadContext(reader, completed, excepted);
		}
	}
}
