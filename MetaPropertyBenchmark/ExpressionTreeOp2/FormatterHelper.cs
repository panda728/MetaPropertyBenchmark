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
        readonly static Type _this = typeof(FormatterHelperExtention);
        readonly static Type _buffer = typeof(IBufferWriter<byte>);

        public static long Serialize<T>(T value, IBufferWriter<byte> writer)
            => Formatter<T>.Serialize(value, writer);

        public static Func<object, IBufferWriter<byte>, long> GenerateFormatter(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsGenericType || propertyInfo.DeclaringType == null)
                return (o, w) => FormatterExtention.SerializeNone(o, w);

            var method = _this.GetMethod("Serialize",
                genericParameterCount: 1,
                types: new Type[] { Type.MakeGenericMethodParameter(0), _buffer });
            if (method == null)
                return (o, w) => FormatterExtention.SerializeNone(o, w);

            var target = Expression.Parameter(_objectType, "i");
            var instance = Expression.Convert(target, propertyInfo.DeclaringType);
            var property = Expression.PropertyOrField(instance, propertyInfo.Name);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            var ps = new Expression[] { property, writer };
            var call = Expression.Call(method.MakeGenericMethod(propertyInfo.PropertyType), ps);
            var lambda = Expression.Lambda(call, target, writer);
            return (Func<object, IBufferWriter<byte>, long>)lambda.Compile();

            //// Func<object, long> getCategoryId = (i,writer) => FormatterHelperExtention.Serialize<T>((i as T).CategoryId, writer);
            //var instanceObj = Expression.Parameter(_objectType, "i");
            //var instance = Expression.Convert(instanceObj, propertyInfo.DeclaringType);
            //var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            //var property = Expression.Property(instance, propertyInfo);

            //var method = _this.GetMethod("Serialize",
            //    genericParameterCount: 1,
            //    types: new Type[] { Type.MakeGenericMethodParameter(0), _buffer });
            //if (method == null)
            //    return (o, w) => FormatterExtention.SerializeNone(o, w);

            //var ps = new Expression[] { property, writer };
            //var call = Expression.Call(method.MakeGenericMethod(propertyInfo.PropertyType), ps);
            //var lambda = Expression.Lambda(call, instanceObj, writer);
            //return (Func<object, IBufferWriter<byte>, long>)lambda.Compile();
        }
    }
}
