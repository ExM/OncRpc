using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xdr;
using Xdr.ReadContexts;

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
			new StringData(_reader, 29, Field2_Readed, _excepted);
		}

		private void Field2_Readed(string val)
		{
			_target.Field2 = val;
			new StringData(_reader, 31, Field3_Readed, _excepted);
		}
		/*
		IL_0000: nop
		IL_0001: ldarg.0
		IL_0002: ldfld class EmitTest.TreeNode EmitTest.TreeNode_ReadContext::_target
		IL_0007: ldarg.1
		IL_0008: callvirt instance void EmitTest.TreeNode::set_Field2(string)
		IL_000d: nop
		IL_000e: ldarg.0
		IL_000f: ldfld class [Xdr]Xdr.IByteReader EmitTest.TreeNode_ReadContext::_reader
		IL_0014: ldc.i4.s 31
		IL_0016: ldarg.0
		IL_0017: ldftn instance void EmitTest.TreeNode_ReadContext::Field3_Readed(string)
		IL_001d: newobj instance void class [mscorlib]System.Action`1<string>::.ctor(object, native int)
		IL_0022: ldarg.0
		IL_0023: ldfld class [mscorlib]System.Action`1<class [mscorlib]System.Exception> EmitTest.TreeNode_ReadContext::_excepted
		IL_0028: newobj instance void [Xdr]Xdr.ReadContexts.StringData::.ctor(class [Xdr]Xdr.IByteReader, uint32, class [mscorlib]System.Action`1<string>, class [mscorlib]System.Action`1<class [mscorlib]System.Exception>)
		IL_002d: pop
		IL_002e: ret
		*/
		
		
		private void Field3_Readed(string val)
		{
			_target.Field3 = val;
			_completed(_target);
		}
		/*
		IL_0000: nop
		IL_0001: ldarg.0
		IL_0002: ldfld class EmitTest.TreeNode EmitTest.TreeNode_ReadContext::_target
		IL_0007: ldarg.1
		IL_0008: stfld string EmitTest.TreeNode::Field3
		IL_000d: ldarg.0
		IL_000e: ldfld class [mscorlib]System.Action`1<class EmitTest.TreeNode> EmitTest.TreeNode_ReadContext::_completed
		IL_0013: ldarg.0
		IL_0014: ldfld class EmitTest.TreeNode EmitTest.TreeNode_ReadContext::_target
		IL_0019: callvirt instance void class [mscorlib]System.Action`1<class EmitTest.TreeNode>::Invoke(!0)
		IL_001e: nop
		IL_001f: ret
		*/
		
		public static void Read(IByteReader reader, Action<TreeNode> completed, Action<Exception> excepted)
		{
			new TreeNode_ReadContext(reader, completed, excepted);
		}
	}
}
