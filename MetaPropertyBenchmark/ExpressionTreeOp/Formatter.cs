using System.Buffers;
using System.Text;

namespace MetaPropertyBenchmark.ExpressionTreeOp
{
    public static class Formatter
    {
        static readonly byte[] _emptyColumn = Encoding.UTF8.GetBytes("<c></c>");
        static readonly byte[] _colStart = Encoding.UTF8.GetBytes("<c>");
        static readonly byte[] _colEnd = Encoding.UTF8.GetBytes("</c>");

        public static long Serialize(string value, IBufferWriter<byte> writer)
        {
            if (string.IsNullOrEmpty(value)) WriteEmptyCoulumn(writer);
            writer.Write(_colStart);
            Encoding.ASCII.GetBytes(value, writer);
            writer.Write(_colEnd);
            return value.Length;
        }

        public static long WriteEmptyCoulumn(IBufferWriter<byte> writer)
        {
            writer.Write(_emptyColumn);
            return 0;
        }

        public static long Serialize(object value, IBufferWriter<byte> writer)
            => value == null 
                ? WriteEmptyCoulumn(writer)
                : Serialize(value?.ToString() ?? "", writer);

        public static long Serialize(Guid value, IBufferWriter<byte> writer)
        {
            var s = value.ToString();
            Serialize(s, writer);
            return s.Length;
        }

        public static long Serialize(Enum value, IBufferWriter<byte> writer)
        {
            var s = value.ToString();
            Serialize(s, writer);
            return s.Length;
        }

        #region number
        public static long Serialize(int value, IBufferWriter<byte> writer)
            => WriterNumber($"{value}", writer);
        public static long Serialize(long value, IBufferWriter<byte> writer)
            => WriterNumber($"{value}", writer);
        public static long Serialize(float value, IBufferWriter<byte> writer)
            => WriterNumber($"{value}", writer);
        public static long Serialize(double value, IBufferWriter<byte> writer)
            => WriterNumber($"{value}", writer);
        public static long Serialize(decimal value, IBufferWriter<byte> writer)
            => WriterNumber($"{value}", writer);

        static long WriterNumber(ReadOnlySpan<char> chars, IBufferWriter<byte> writer)
        {
            writer.Write(_colStart);
            _ = Encoding.UTF8.GetBytes(chars, writer);
            writer.Write(_colEnd);
            return chars.Length;
        }
        #endregion

        public static long Serialize(bool? value, IBufferWriter<byte> writer)
        {
            if (value == null) return WriteEmptyCoulumn(writer);
            writer.Write(_colStart);
            var s = $"{value}";
            _ = Encoding.UTF8.GetBytes(s, writer);
            writer.Write(_colEnd);
            return s.Length;
        }

        #region date
        public static long Serialize(DateTime value, IBufferWriter<byte> writer)
        {
            var s = $"{value:yyyy-MM-dd HH:mm:ss}";
            writer.Write(_colStart);
            _ = Encoding.UTF8.GetBytes(s, writer);
            writer.Write(_colEnd);
            return s.Length;
        }
        public static long Serialize(DateOnly? value, IBufferWriter<byte> writer)
        {
            var s = $"{value:yyyy-MM-dd}";
            writer.Write(_colStart);
            _ = Encoding.UTF8.GetBytes(s, writer);
            writer.Write(_colEnd);
            return s.Length;
        }
        public static long Serialize(TimeOnly? value, IBufferWriter<byte> writer)
        {
            var s = $"{value:HH:mm:ss}";
            writer.Write(_colStart);
            _ = Encoding.UTF8.GetBytes(s, writer);
            writer.Write(_colEnd);
            return s.Length;
        }
        #endregion
    }
}

