using System.Buffers;
using System.Text;

namespace MetaPropertyBenchmark.ExpressionTreeOp2
{
    public static class Formatter<T>
    {
        static Formatter()
        {
            var t = typeof(T);
            if (t.IsGenericType) Serialize = FormatterExtention.SerializeNone;
            else if (t == typeof(string)) Serialize = FormatterExtention.SerializeString;
            else if (t == typeof(DateTime)) Serialize = FormatterExtention.SerializeDate;
            else if (t == typeof(int)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(uint)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(short)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(sbyte)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(byte)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(ushort)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(uint)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(long)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(ulong)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(nint)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(nuint)) Serialize = FormatterExtention.SerializeNumber;
            else if (t == typeof(DateOnly)) Serialize = FormatterExtention.SerializeDate;
            else if (t == typeof(TimeOnly)) Serialize = FormatterExtention.SerializeDate;
            else if (t == typeof(bool)) Serialize = FormatterExtention.SerializeBoolean;
            else Serialize = FormatterExtention.SerializeString;
        }
        public static readonly Func<T, IBufferWriter<byte>, long> Serialize;
    }

    public static class FormatterExtention
    {
        static readonly byte[] _emptyColumn = Encoding.UTF8.GetBytes("<c></c>");
        static readonly byte[] _colStart = Encoding.UTF8.GetBytes("<c>");
        static readonly byte[] _colEnd = Encoding.UTF8.GetBytes("</c>");

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
            => value != null && value is string s
                ? Write(s, writer)
                : Write($"{value}", writer);

        public static long SerializeNone<TFunc>(TFunc _, IBufferWriter<byte> writer)
            => WriteEmpty(writer);

        public static long SerializeBoolean<TFunc>(TFunc value, IBufferWriter<byte> writer)
        {
            writer.Write(_colStart);
            var s = $"{value}";
            Encoding.UTF8.GetBytes(s, writer);
            writer.Write(_colEnd);
            return s.Length;
        }

        public static long SerializeNumber<TFunc>(TFunc value, IBufferWriter<byte> writer)
        {
            writer.Write(_colStart);
            var s = $"{value}";
            Encoding.UTF8.GetBytes(s, writer);
            writer.Write(_colEnd);
            return s.Length;
        }

        public static long SerializeDate<TFunc>(TFunc value, IBufferWriter<byte> writer)
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

