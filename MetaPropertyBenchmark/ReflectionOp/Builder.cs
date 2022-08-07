using System.Buffers;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace MetaPropertyBenchmark.ReflectionOp
{
    public class Builder : IDisposable
    {
        readonly byte[] _newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
        readonly byte[] _rowTag1 = Encoding.UTF8.GetBytes("<r>");
        readonly byte[] _rowTag2 = Encoding.UTF8.GetBytes("</r>");
        readonly byte[] _columnTag1 = Encoding.UTF8.GetBytes("<c>");
        readonly byte[] _columnTag2 = Encoding.UTF8.GetBytes("</c>");

        readonly ConcurrentDictionary<Type, PropCache[]> _dic = new();
        readonly ArrayPoolBufferWriter _writer = new();

        public void Dispose()
        {
            _writer.Dispose();
        }

        public void Compile(Type t) => GetPropertiesChace(t);
        PropCache[] GetPropertiesChace(Type t)
            => _dic.GetOrAdd(t, key
                => t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .AsParallel()
                    .Select((x, i) => new PropCache(x, i))
                    .OrderBy(p => p.Index)
                    .ToArray()
            );

        public void Run<T>(Stream stream, IEnumerable<T> rows)
        {
            var properties = GetPropertiesChace(typeof(T)).AsSpan();

            WriteLine("<body>", stream);
            foreach (var row in rows)
            {
                Write(_rowTag1, stream);
                foreach (var p in properties)
                {
                    WriteColumn(p.Accessor.GetValue(row), stream);
                }
                WriteLine(_rowTag2, stream);
            }
            WriteLine("</body>", stream);
        }

        void Write(ReadOnlySpan<char> chars, Stream stream)
        {
            Encoding.UTF8.GetBytes(chars, _writer);
            _writer.CopyTo(stream);
        }

        void WriteLine(ReadOnlySpan<char> chars, Stream stream)
        {
            Encoding.UTF8.GetBytes(chars, _writer);
            _writer.Write(_newLine);
            _writer.CopyTo(stream);
        }
        void Write(byte[] bytes, Stream stream)
        {
            stream.Write(bytes);
        }

        void WriteLine(byte[] bytes, Stream stream)
        {
            stream.Write(bytes);
            stream.Write(_newLine);
        }
        void WriteLine(ReadOnlySpan<byte> bytes, Stream stream)
        {
            _writer.Write(bytes);
            _writer.Write(_newLine);
            _writer.CopyTo(stream);
        }

        void WriteLine(Stream stream)
        {
            stream.Write(_newLine);
        }

        void WriteColumn(object? value, Stream stream)
        {
            _writer.Write(_columnTag1);
            Encoding.UTF8.GetBytes(value?.ToString() ?? "", _writer);
            _writer.Write(_columnTag2);
            _writer.CopyTo(stream);
        }

        internal class PropCache
        {
            public PropCache(PropertyInfo p, int index)
            {
                Name = p.Name;
                Accessor = p.GetAccessor();
                Length = index;
                Index = index;
            }

            public string Name { get; init; }
            public IAccessor Accessor { get; init; }
            public int Length { get; set; }
            public int Index { get; set; }
        }
    }
}
