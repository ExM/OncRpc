using System;

namespace Xdr.StaticSingletones
{
	public sealed class Translator: BaseTranslator
	{
		private readonly Type _readOneCacheType;
		private readonly Type _readManyCacheType;
		private readonly Type _writeOneCacheType;
		private readonly Type _writeManyCacheType;

		public Translator()
		{
			DelegateCache.BuildRequest = AppendBuildRequest;
			_readOneCacheType = typeof(ReadOneCache<>);
			_readManyCacheType = typeof(ReadManyCache<>);
			_writeOneCacheType = typeof(WriteOneCache<>);
			_writeManyCacheType = typeof(WriteManyCache<>);
		}

		protected override Type GetReadOneCacheType()
		{
			return _readOneCacheType;
		}
		
		public override void Read<T>(IReader reader, Action<T> completed, Action<Exception> excepted)
		{
			if (ReadOneCache<T>.Instance == null)
				BuildCaches();
			ReadOneCache<T>.Instance(reader, completed, excepted);
		}

		protected override Type GetReadManyCacheType()
		{
			return _readManyCacheType;
		}

		public override void Read<T>(IReader reader, uint len, bool fix, Action<T> completed, Action<Exception> excepted)
		{
			if (ReadManyCache<T>.Instance == null)
				BuildCaches();
			ReadManyCache<T>.Instance(reader, len, fix, completed, excepted);
		}

		protected override Type GetWriteOneCacheType()
		{
			return _writeOneCacheType;
		}

		public override void Write<T>(IWriter writer, T item, Action completed, Action<Exception> excepted)
		{
			if (WriteOneCache<T>.Instance == null)
				BuildCaches();
			WriteOneCache<T>.Instance(writer, item, completed, excepted);
		}

		protected override Type GetWriteManyCacheType()
		{
			return _writeManyCacheType;
		}

		public override void Write<T>(IWriter writer, T items, bool fix, Action completed, Action<Exception> excepted)
		{
			if (WriteManyCache<T>.Instance == null)
				BuildCaches();
			WriteManyCache<T>.Instance(writer, items, fix, completed, excepted);
		}
	}
}
