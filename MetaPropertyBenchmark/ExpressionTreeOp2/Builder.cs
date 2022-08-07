using System.Buffers;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MetaPropertyBenchmark.ExpressionTreeOp2
{
    /// <summary>
    /// IEnumerable<T>からXML形式のファイルを出力
    /// </summary>
    /// <remarks>EpressionTreeでプロパティをobjectに変換せずに処理</remarks>
    public class Builder 
    {
        readonly byte[] _newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
        readonly byte[] _rowTag1 = Encoding.UTF8.GetBytes("<r>");
        readonly byte[] _rowTag2 = Encoding.UTF8.GetBytes("</r>");
        readonly byte[] _columnTag1 = Encoding.UTF8.GetBytes("<c>");
        readonly byte[] _columnTag2 = Encoding.UTF8.GetBytes("</c>");

        readonly ConcurrentDictionary<Type, FormatterHelper[]> _dic = new();
        
        public void Compile(Type t) => GetPropertiesCache(t);
        FormatterHelper[] GetPropertiesCache(Type t)
            => _dic.GetOrAdd(t, key
                => t.GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .AsParallel()
                    .Select((p, i) => new FormatterHelper(p, i))
                    .OrderBy(p => p.Index)
                    .ToArray()
            );

        public void Run<T>(Stream stream, IEnumerable<T> rows)
        {
            var properties = GetPropertiesCache(typeof(T)).AsSpan();
            using var writer = new ArrayPoolBufferWriter();

            WriteLine("<body>", writer);
            writer.CopyTo(stream);
            foreach (var row in rows)
            {
                Write(_rowTag1, writer);
                foreach (var p in properties)
                {
                    p.Formatter(row, writer);
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
    }
}
