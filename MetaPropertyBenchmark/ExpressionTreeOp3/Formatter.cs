using System.Buffers;
using System.Text;

namespace MetaPropertyBenchmark.ExpressionTreeOp3
{
    public static class Formatter
    {
        static readonly byte[] _emptyColumn = Encoding.UTF8.GetBytes("<c></c>");
        static readonly byte[] _colStart = Encoding.UTF8.GetBytes("<c>");
        static readonly byte[] _colEnd = Encoding.UTF8.GetBytes("</c>");

        public static long WriteEmpty(ref IBufferWriter<byte> writer)
        {
            writer.Write(_emptyColumn);
            return 0;
        }

        public static long Write(ReadOnlySpan<char> chars, ref IBufferWriter<byte> writer)
        {
            writer.Write(_colStart);
            _ = Encoding.UTF8.GetBytes(chars, writer);
            writer.Write(_colEnd);
            return chars.Length;
        }

        public static long Serialize(string value, ref IBufferWriter<byte> writer)
            => Write(value.AsSpan(), ref writer);
        public static long Serialize(object? value, ref IBufferWriter<byte> writer)
            => Write(value?.ToString() ?? "", ref writer);
        public static long Serialize(Guid value, ref IBufferWriter<byte> writer)
            => Write($"{value}", ref writer);
        public static long Serialize(Enum value, ref IBufferWriter<byte> writer)
            => Write($"{value}", ref writer);
        public static long Serialize(int value, ref IBufferWriter<byte> writer)
            => Write($"{value}", ref writer);
        public static long Serialize(long value, ref IBufferWriter<byte> writer)
            => Write($"{value}", ref writer);
        public static long Serialize(float value, ref IBufferWriter<byte> writer)
            => Write($"{value}", ref writer);
        public static long Serialize(double value, ref IBufferWriter<byte> writer)
            => Write($"{value}", ref writer);
        public static long Serialize(decimal value, ref IBufferWriter<byte> writer)
            => Write($"{value}", ref writer);
        public static long Serialize(bool value, ref IBufferWriter<byte> writer)
            => Write($"{value}", ref writer);
        public static long Serialize(DateTime value, ref IBufferWriter<byte> writer)
            => Write($"{value:yyyy-MM-ddTHH:mm:ss}", ref writer);
        public static long Serialize(DateOnly value, ref IBufferWriter<byte> writer)
            => Write($"{value:yyyy-MM-dd}", ref writer);
        public static long Serialize(TimeOnly value, ref IBufferWriter<byte> writer)
            => Write($"{value:HH:mm:ss}", ref writer);
    }
}

