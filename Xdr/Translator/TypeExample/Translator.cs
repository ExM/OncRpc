using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Xdr.Examples
{
	internal class Translator: BaseTranslator
	{
		public Translator()
		{
			DelegateCache.BuildRequest = AppendBuildRequest;
		}
		
		public override void Read<T>(IReader reader, Action<T> completed, Action<Exception> excepted)
		{
			if (ReadOneCache<T>.Instance == null)
				BuildCaches();
			ReadOneCache<T>.Instance(reader, completed, excepted);
		}

		public override void Read<T>(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{
			if (ReadManyCache<T>.Instance == null)
				BuildCaches();
			ReadManyCache<T>.Instance(reader, len, fix, completed, excepted);
		}

		public override Type ReadOneCacheType
		{
			get
			{
				return typeof(ReadOneCache<>);
			}
		}
		
		public override Type ReadManyCacheType
		{
			get
			{
				return typeof(ReadManyCache<>);
			}
		}
	}
}
