using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;

namespace EmitTest
{
	public class TreeNode2_ReadContext
	{
		private TreeNode2 _target;
		private IByteReader _reader;
		private Action<TreeNode2> _completed;
		private Action<Exception> _excepted;

		public TreeNode2_ReadContext(IByteReader reader, Action<TreeNode2> completed, Action<Exception> excepted)
		{
			_target = new TreeNode2();
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
	}
}
