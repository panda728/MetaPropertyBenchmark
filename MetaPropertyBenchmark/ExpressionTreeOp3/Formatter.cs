using System.Buffers;
using System.Text;

namespace MetaPropertyBenchmark.ExpressionTreeOp3
{
    public static class Formatter
    {
        static readonly byte[] _emptyColumn = Encoding.UTF8.GetBytes("<c></c>");
        static readonly byte[] _colStart = Encoding.UTF8.GetBytes("<c>");
        static readonly byte[] _colEnd = Encoding.UTF8.GetBytes("</c>");

        public static long WriteEmpty(IBufferWriter<byte> writer)
        {
            writer.Write(_emptyColumn);
            return 0;
        }

        public static long Write(ReadOnlySpan<char> chars, IBufferWriter<byte> writer)
        {
            writer.Write(_colStart);
            _ = Encoding.UTF8.GetBytes(chars, writer);
            writer.Write(_colEnd);
            return chars.Length;
        }

        public static long Serialize(string value, IBufferWriter<byte> writer)
            => Write(value.AsSpan(), writer);
        public static long Serialize(object? value, IBufferWriter<byte> writer)
            => Write(value?.ToString() ?? "", writer);
        public static long Serialize(Guid value, IBufferWriter<byte> writer)
            => Write($"{value}", writer);
        public static long Serialize(Enum value, IBufferWriter<byte> writer)
            => Write($"{value}", writer);
        public static long Serialize(int value, IBufferWriter<byte> writer)
            => Write($"{value}", writer);
        public static long Serialize(long value, IBufferWriter<byte> writer)
            => Write($"{value}", writer);
        public static long Serialize(float value, IBufferWriter<byte> writer)
            => Write($"{value}", writer);
        public static long Serialize(double value, IBufferWriter<byte> writer)
            => Write($"{value}", writer);
        public static long Serialize(decimal value, IBufferWriter<byte> writer)
            => Write($"{value}", writer);
        public static long Serialize(bool value, IBufferWriter<byte> writer)
            => Write($"{value}", writer);
        public static long Serialize(DateTime value, IBufferWriter<byte> writer)
            => Write($"{value:yyyy-MM-ddTHH:mm:ss}", writer);
        public static long Serialize(DateOnly value, IBufferWriter<byte> writer)
            => Write($"{value:yyyy-MM-dd}", writer);
        public static long Serialize(TimeOnly value, IBufferWriter<byte> writer)
            => Write($"{value:HH:mm:ss}", writer);
    }
}

