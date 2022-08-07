using System.Buffers;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MetaPropertyBenchmark.ExpressionTreeOp
{
    public class Builder : IDisposable
    {
        readonly byte[] _newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
        readonly byte[] _rowTag1 = Encoding.UTF8.GetBytes("<r>");
        readonly byte[] _rowTag2 = Encoding.UTF8.GetBytes("</r>");
        readonly byte[] _columnTag1 = Encoding.UTF8.GetBytes("<c>");
        readonly byte[] _columnTag2 = Encoding.UTF8.GetBytes("</c>");

        readonly ConcurrentDictionary<Type, FormatterHelper[]> _dic = new();
        readonly ArrayPoolBufferWriter _writer = new();

        public void Dispose()
        {
            _writer.Dispose();
        }

        public void Compile(Type t) => GetPropertiesChace(t);
        FormatterHelper[] GetPropertiesChace(Type t)
            => _dic.GetOrAdd(t, key
                => t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .AsParallel()
                    .Select((p, i) => new FormatterHelper(t, p, i))
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
                    p.Formatter(row, _writer);
                    _writer.CopyTo(stream);
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
            _writer.Write(_newLine);
            _writer.CopyTo(stream);
        }

        internal class PropCache
        {
            public PropCache(PropertyInfo p, int index)
            {
                Name = p.Name;
                var target = Expression.Parameter(typeof(object), p.Name);
                var instance = Expression.Convert(target, p.DeclaringType);
                var property = Expression.PropertyOrField(instance, p.Name);
                var propertyObj = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<object, object>>(propertyObj, target);
                Getter = lambda.Compile();
                Index = index;
            }

            public string Name { get; init; }
            public Func<object, object> Getter { get; init; }
            public int Index { get; set; }
        }
    }
}
