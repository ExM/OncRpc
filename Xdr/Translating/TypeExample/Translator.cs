using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Xdr.Examples
{
	internal sealed class Translator: BaseTranslator
	{
		private readonly Type _readOneCacheType;
		private readonly Type _readManyCacheType;

		public Translator()
		{
			DelegateCache.BuildRequest = AppendBuildRequest;
			_readOneCacheType = typeof(ReadOneCache<>);
			_readManyCacheType = typeof(ReadManyCache<>);
		}
		
		internal override void Read<T>(IReader reader, Action<T> completed, Action<Exception> excepted)
		{
			if (ReadOneCache<T>.Instance == null)
				BuildCaches();
			ReadOneCache<T>.Instance(reader, completed, excepted);
		}

		internal override void Read<T>(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{
			if (ReadManyCache<T>.Instance == null)
				BuildCaches();
			ReadManyCache<T>.Instance(reader, len, fix, completed, excepted);
		}

		protected override Type GetReadOneCacheType()
		{
			return _readOneCacheType;
		}

		protected override Type GetReadManyCacheType()
		{
			return _readManyCacheType;
		}
	}
}
