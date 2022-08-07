using System.Buffers;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MetaPropertyBenchmark.ExpressionTree
{
    /// <summary>
    /// IEnumerable<T>からXML形式のファイルを出力
    /// </summary>
    /// <remarks>EpressionTreeでプロパティにアクセス</remarks>
    public class Builder : IDisposable
    {
        readonly byte[] _newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
        readonly byte[] _rowTag1 = Encoding.UTF8.GetBytes("<r>");
        readonly byte[] _rowTag2 = Encoding.UTF8.GetBytes("</r>");
        readonly byte[] _columnTag1 = Encoding.UTF8.GetBytes("<c>");
        readonly byte[] _columnTag2 = Encoding.UTF8.GetBytes("</c>");

        readonly ConcurrentDictionary<Type, PropCache[]> _dic = new();
        readonly ArrayPoolBufferWriter writer = new();

        public void Dispose()
        {
            writer.Dispose();
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
            using var writer = new ArrayPoolBufferWriter();

            WriteLine("<body>", writer);
            writer.CopyTo(stream);
            foreach (var row in rows)
            {
                Write(_rowTag1, writer);
                foreach (var p in properties)
                {
                    if (p.Getter == null)
                        WriteColumnEmpty(writer);
                    else
                        WriteColumn(p.Getter(row), writer);
                    writer.CopyTo(stream);
                }

                WriteLine(_rowTag2, writer);
                writer.CopyTo(stream);
            }
            WriteLine("</body>", writer);
            writer.CopyTo(stream);
        }
        void WriteLine(ReadOnlySpan<char> chars, IBufferWriter<byte> writer)
        {
            Encoding.UTF8.GetBytes(chars, writer);
            writer.Write(_newLine);
        }
        void Write(byte[] bytes, IBufferWriter<byte> writer)
        {
            writer.Write(bytes);
        }

        void WriteLine(byte[] bytes, IBufferWriter<byte> writer)
        {
            writer.Write(bytes);
            writer.Write(_newLine);
        }

        void WriteColumn(object? value, IBufferWriter<byte> writer)
        {
            writer.Write(_columnTag1);
            Encoding.UTF8.GetBytes(value?.ToString() ?? "", writer);
            writer.Write(_columnTag2);
        }
        void WriteColumnEmpty(IBufferWriter<byte> writer)
        {
            writer.Write(_columnTag1);
            writer.Write(_columnTag2);
        }

        internal class PropCache
        {
            public PropCache(PropertyInfo p, int index)
            {
                Name = p.Name;
                Getter = GenerateLamda(p);
                Index = index;
            }

            Func<object, object> GenerateLamda(PropertyInfo p)
            {
                if (p.PropertyType.IsGenericType)
                    return null;

                var target = Expression.Parameter(typeof(object), p.Name);
                var instance = Expression.Convert(target, p.DeclaringType);
                var property = Expression.PropertyOrField(instance, p.Name);
                var propertyObj = Expression.Convert(property, typeof(object));
                var lambda = Expression.Lambda<Func<object, object>>(propertyObj, target);
                return lambda.Compile();
            }

            public string Name { get; init; }
            public Func<object, object> Getter { get; init; }
            public int Index { get; set; }
        }


    }
}
