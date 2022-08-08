using System.Buffers;
using System.Linq.Expressions;
using System.Reflection;

namespace MetaPropertyBenchmark.ExpressionTreeOp
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

        public static Func<object, IBufferWriter<byte>, long> GenerateFormatter(this PropertyInfo p)
            => IsSupported(p.PropertyType)
                ? p.GenerateSupported()
                : p.GenerateObject();

        static bool IsSupported(Type t)
        {
            if(t.IsPrimitive)
                return true;
            if (t == typeof(string) || t == typeof(Guid) || t == typeof(Enum))
                return true;
            else if (t == typeof(DateTime) || t == typeof(DateOnly) || t == typeof(TimeOnly) || t == _objectType)
                return true;
            return false;
        }

        static Func<object, IBufferWriter<byte>, long> GenerateSupported(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsGenericType)
                return (o, v) => Formatter.WriteEmpty(v);

            var method = typeof(Formatter).GetMethod("Serialize", new Type[] { propertyInfo.PropertyType, typeof(IBufferWriter<byte>) });
            if (method == null)
                return (o, v) => Formatter.WriteEmpty(v);

            var target = Expression.Parameter(typeof(object), "i");
            var instance = Expression.Convert(target, propertyInfo.DeclaringType);
            var property = Expression.PropertyOrField(instance, propertyInfo.Name);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            var ps = new Expression[] { property, writer };

            var call = Expression.Call(method, ps);
            var lambda = Expression.Lambda(call, target, writer);
            return (Func<object, IBufferWriter<byte>, long>)lambda.Compile();
        }

        static Func<object, IBufferWriter<byte>, long> GenerateObject(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsGenericType)
                return (o, v) => Formatter.WriteEmpty(v);

            var method = typeof(Formatter).GetMethod("Serialize", new Type[] { _objectType, typeof(IBufferWriter<byte>) });
            if (method == null)
                return (o, v) => Formatter.WriteEmpty(v);

            // Func<object, long> getCategoryId = (i,writer) => Formatter.Serialize((object)((i as T).CategoryId), writer);
            var target = Expression.Parameter(typeof(object), "i");
            var instance = Expression.Convert(target, propertyInfo.DeclaringType);
            var property = Expression.PropertyOrField(instance, propertyInfo.Name);
            var propertyConv = Expression.Convert(property, _objectType);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");

            var ps = new Expression[] { propertyConv, writer };
            var call = Expression.Call(method, ps);
            var lambda = Expression.Lambda(call, target, writer);
            return (Func<object, IBufferWriter<byte>, long>)lambda.Compile();
        }
    }
}
