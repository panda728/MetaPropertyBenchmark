using System.Buffers;
using System.Text;

namespace MetaPropertyBenchmark.ExpressionTreeOp2
{
    public static class Formatter<T>
    {
        public static readonly Func<T, IBufferWriter<byte>, long> Builder;

        static Formatter()
        {
            var t = typeof(T);
            if (t.IsGenericType) Builder = FormatterExtention.SerializeNone;
            else if (t == typeof(string)) Builder = FormatterExtention.SerializeString;
            else if (t == typeof(DateTime)) Builder = FormatterExtention.SerializeDate;
            else if (t == typeof(int)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(uint)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(short)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(sbyte)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(byte)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(ushort)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(uint)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(long)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(ulong)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(nint)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(nuint)) Builder = FormatterExtention.SerializeNumber;
            else if (t == typeof(DateOnly)) Builder = FormatterExtention.SerializeDate;
            else if (t == typeof(TimeOnly)) Builder = FormatterExtention.SerializeDate;
            else if (t == typeof(bool)) Builder = FormatterExtention.SerializeBoolean;
            else Builder = FormatterExtention.SerializeString;
        }
    }

    public static class FormatterExtention
    {
        static readonly byte[] _emptyColumn = Encoding.UTF8.GetBytes("<c></c>");
        static readonly byte[] _colStart = Encoding.UTF8.GetBytes("<c>");
        static readonly byte[] _colEnd = Encoding.UTF8.GetBytes("</c>");

        static long Write(object? value, IBufferWriter<byte> writer)
        {
            if (value == null)
                WriteEmpty(writer);
            var s = $"{value}";
            writer.Write(_colStart);
            Encoding.UTF8.GetBytes(s, writer);
            writer.Write(_colEnd);
            return s.Length;
        }

        static long Write(ReadOnlySpan<char> value, IBufferWriter<byte> writer)
        {
            if (value.Length == 0)
                WriteEmpty(writer);
            writer.Write(_colStart);
            Encoding.UTF8.GetBytes(value, writer);
            writer.Write(_colEnd);
            return value.Length;
        }

        static long WriteEmpty(IBufferWriter<byte> writer)
        {
            writer.Write(_emptyColumn);
            return 0;
        }

        public static long SerializeString<TFunc>(TFunc? value, IBufferWriter<byte> writer)
            => value is string s
                ? Write(s.AsSpan(), writer)
                : Write(value, writer);

        public static long SerializeNone<TFunc>(TFunc? _, IBufferWriter<byte> writer)
            => WriteEmpty(writer);

        public static long SerializeBoolean<TFunc>(TFunc? value, IBufferWriter<byte> writer)
        {
            writer.Write(_colStart);
            var s = $"{value}";
            Encoding.UTF8.GetBytes(s, writer);
            writer.Write(_colEnd);
            return s.Length;
        }

        public static long SerializeNumber<TFunc>(TFunc? value, IBufferWriter<byte> writer)
        {
            writer.Write(_colStart);
            var s = $"{value}";
            Encoding.UTF8.GetBytes(s, writer);
            writer.Write(_colEnd);
            return s.Length;
        }

        public static long SerializeDate<TFunc>(TFunc? value, IBufferWriter<byte> writer)
        {
            if (value is DateTime dateTime)
                return Write($"{dateTime:yyyy-MM-ddTHH:mm:ss}".AsSpan(), writer);
            else if (value is DateOnly dateOnly)
                return Write($"{dateOnly:yyyy-MM-dd}".AsSpan(), writer);
            else if (value is TimeOnly timeOnly)
                return Write($"{timeOnly:HH:mm:ss}".AsSpan(), writer);
            else
                return WriteEmpty(writer);
        }
    }
}

