using System;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Collections.Generic;
using Xdr.Translating;
using System.IO;

namespace Xdr
{
	public sealed partial class TranslatorBuilder
	{
		private BaseTranslator _t;
		
		internal TranslatorBuilder(string name)
		{
			Stream stream = typeof(TranslatorBuilder).Assembly.GetManifestResourceStream("Xdr.StaticSingletones.dll");
			byte[] bytes = new byte[stream.Length];
			stream.Read(bytes, 0, (int)stream.Length - 1);

			Assembly asm = Assembly.Load(bytes);

			object inst = asm.CreateInstance("Xdr.StaticSingletones.Translator");
			_t = inst as BaseTranslator;
		}

		public TranslatorBuilder Map<T>(ReadOneDelegate<T> reader)
		{
			_t.AppendMethod(typeof(T), MethodType.ReadOne, reader);
			return this;
		}

		public TranslatorBuilder Map<T>(ReadManyDelegate<T> reader)
		{
			_t.AppendMethod(typeof(T), MethodType.ReadMany, reader);
			return this;
		}

		public ITranslator Build()
		{
			return _t;
		}
	}
}

