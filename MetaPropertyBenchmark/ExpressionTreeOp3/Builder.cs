using System.Buffers;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

namespace MetaPropertyBenchmark.ExpressionTreeOp3
{
    /// <summary>
    /// IEnumerable<T>からXML風のファイルを出力
    /// </summary>
    /// <remarks>EpressionTreeでプロパティをobjectに変換せずに処理</remarks>
    public class Builder
    {
        readonly byte[] _newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
        readonly byte[] _rowTag1 = Encoding.UTF8.GetBytes("<r>");
        readonly byte[] _rowTag2 = Encoding.UTF8.GetBytes("</r>");

        public void Compile<T>() => _ = GetPropertiesCache<T>.Properties;

        private static class GetPropertiesCache<T>
        {
            static GetPropertiesCache()
            {
                Properties = typeof(T)
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    .AsParallel()
                    .Select((p, i) => new FormatterHelper<T>(p, i))
                    .OrderBy(p => p.Index)
                    .ToArray();
            }
            public static readonly FormatterHelper<T>[] Properties;
        }

        public void Run<T>(Stream stream, IEnumerable<T> rows)
        {
            var properties = GetPropertiesCache<T>.Properties.AsSpan();
            using var buffer = new ArrayPoolBufferWriter();
            var writer = (IBufferWriter<byte>)buffer;
            WriteLine("<body>", buffer);
            buffer.CopyTo(stream);
            foreach (var row in rows)
            {
                Write(_rowTag1, buffer);
                foreach (var p in properties)
                {
                    p.Formatter(row, ref writer);
                    buffer.CopyTo(stream);
                }

                WriteLine(_rowTag2, buffer);
                buffer.CopyTo(stream);
            }
            WriteLine("</body>", buffer);
            buffer.CopyTo(stream);
        }

        void WriteLine(ReadOnlySpan<char> chars, IBufferWriter<byte> buffer)
        {
            Encoding.UTF8.GetBytes(chars, buffer);
            buffer.Write(_newLine);
        }

        void Write(byte[] bytes, IBufferWriter<byte> buffer)
        {
            buffer.Write(bytes);
        }

        void WriteLine(byte[] bytes, IBufferWriter<byte> buffer)
        {
            buffer.Write(bytes);
            buffer.Write(_newLine);
        }
    }
}
