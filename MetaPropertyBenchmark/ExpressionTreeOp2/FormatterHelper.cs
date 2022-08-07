using System.Buffers;
using System.Linq.Expressions;
using System.Reflection;

namespace MetaPropertyBenchmark.ExpressionTreeOp2
{
    public class FormatterHelper
    {
        public FormatterHelper(PropertyInfo p, int i)
        {
            Name = p.Name;
            Formatter = p.GenerateFormatter();
            Index = i;
        }

        public int Index { get; init; }
        public string Name { get; init; }
        public Func<object, IBufferWriter<byte>, long> Formatter { get; init; }
    }

    public static class FormatterHelperExtention
    {
        readonly static Type _objectType = typeof(object);

        public static long Serialize<T>(T value, IBufferWriter<byte> writer)
            => Formatter<T>.Builder(value, writer);

        public static Func<object, IBufferWriter<byte>, long> GenerateFormatter(this PropertyInfo propertyInfo)
        {
            // Func<object, long> getCategoryId = (i,writer) => FormatterHelperExtention.Serialize<T>((i as T).CategoryId, writer);
            var instanceObj = Expression.Parameter(_objectType, "i");
            var instance = Expression.Convert(instanceObj, propertyInfo.DeclaringType);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            var property = Expression.Property(instance, propertyInfo);

            var method = typeof(FormatterHelperExtention).GetMethod("Serialize",
                genericParameterCount: 1,
                types: new Type[] { Type.MakeGenericMethodParameter(0), typeof(IBufferWriter<byte>) });
            if (method == null)
                return (o, w) => FormatterExtention.SerializeNone(o, w);

            var ps = new Expression[] { property, writer };
            var call = Expression.Call(method.MakeGenericMethod(propertyInfo.PropertyType), ps);
            var lambda = Expression.Lambda(call, instanceObj, writer);
            return (Func<object, IBufferWriter<byte>, long>)lambda.Compile();
        }
    }
}
