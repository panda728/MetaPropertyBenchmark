using System.Buffers;
using System.Linq.Expressions;
using System.Reflection;

namespace MetaPropertyBenchmark.ExpressionTreeOp2
{
    public class FormatterHelper<T>
    {
        public FormatterHelper(PropertyInfo p, int i)
        {
            Name = p.Name;
            Formatter = p.GenerateFormatter<T>();
            Index = i;
        }

        public int Index { get; init; }
        public string Name { get; init; }
        public Func<T, IBufferWriter<byte>, long> Formatter { get; init; }
    }

    public static class FormatterHelperExtention
    {
        readonly static Type[] _methodParams = new Type[] { Type.MakeGenericMethodParameter(0), typeof(IBufferWriter<byte>) };

        public static Func<T, IBufferWriter<byte>, long> GenerateFormatter<T>(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsGenericType || propertyInfo.DeclaringType == null)
                return (o, w) => FormatterExtention.SerializeNone(o, w);

            var method = typeof(Formatter<T>).GetMethod("Serialize",
                genericParameterCount: 1,
                types: _methodParams);

            if (method == null)
                return (o, w) => FormatterExtention.SerializeNone(o, w);

            var target = Expression.Parameter(propertyInfo.DeclaringType, "i");
            var property = Expression.PropertyOrField(target, propertyInfo.Name);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            var ps = new Expression[] { property, writer };
            var call = Expression.Call(method.MakeGenericMethod(propertyInfo.PropertyType), ps);
            var lambda = Expression.Lambda(call, target, writer);
            return (Func<T, IBufferWriter<byte>, long>)lambda.Compile();
        }
    }
}
