using System.Buffers;
using System.Linq.Expressions;
using System.Reflection;

namespace MetaPropertyBenchmark.ExpressionTreeOp
{
    public class FormatterHelper
    {
        public FormatterHelper(Type t, PropertyInfo p, int i)
        {
            Name = p.Name;
            Formatter = p.GenerateFormatter();
            MaxLength = 0;
            Index = i;
        }

        public int Index { get; init; }
        public string Name { get; init; }
        public Func<object, IBufferWriter<byte>, long> Formatter { get; init; }
        public int MaxLength { get; set; }
    }

    public static class FormatterHelperExtention
    {
        readonly static Type _objectType = typeof(object);

        public static Func<object, IBufferWriter<byte>, long> GenerateFormatter(this PropertyInfo p)
            => IsSupported(p.PropertyType)
                ? p.GeneratePrimitive()
                : p.GenerateObject();

        static bool IsSupported(Type t)
        {
            if (t == typeof(string) || t == typeof(Guid) || t == typeof(Enum))
                return true;
            else if (t == typeof(int) || t == typeof(long) || t == typeof(decimal) || t == typeof(double) || t == typeof(float))
                return true;
            else if (t == typeof(DateTime) || t == typeof(DateOnly) || t == typeof(TimeOnly) || t == _objectType)
                return true;
            return false;
        }

        static Func<object, IBufferWriter<byte>, long> GeneratePrimitive(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsGenericType)
                return (o, v) => Formatter.WriteEmptyCoulumn(v);

            // Func<object, long> getCategoryId = (i,writer) => Formatter.Serialize((i as T).CategoryId, writer);
            var instanceObj = Expression.Parameter(_objectType, "i");
            var instance = Expression.Convert(instanceObj, propertyInfo.DeclaringType);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            var property = Expression.Property(instance, propertyInfo);
            var ps = new Expression[] { property, writer };
            var method = typeof(Formatter).GetMethod("Serialize", new Type[] { propertyInfo.PropertyType, typeof(IBufferWriter<byte>) });
            if (method == null)
                return (o, v) => Formatter.WriteEmptyCoulumn(v);

            var call = Expression.Call(method, ps);
            var lambda = Expression.Lambda(call, instanceObj, writer);
            return (Func<object, IBufferWriter<byte>, long>)lambda.Compile();
        }

        static Func<object, IBufferWriter<byte>, long> GenerateObject(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsGenericType)
                return (o, v) => Formatter.WriteEmptyCoulumn(v);

            // Func<object, long> getCategoryId = (i,writer) => Formatter.Serialize((object)((i as T).CategoryId), writer);
            var instanceObj = Expression.Parameter(_objectType, "i");
            var instance = Expression.Convert(instanceObj, propertyInfo.DeclaringType);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            var property = Expression.Property(instance, propertyInfo);
            var propertyObject = Expression.Convert(property, _objectType);
            var ps = new Expression[] { propertyObject, writer };
            var method = typeof(Formatter).GetMethod("Serialize", new Type[] { _objectType, typeof(IBufferWriter<byte>) });
            if (method == null)
                return (o, v) => Formatter.WriteEmptyCoulumn(v);

            var call = Expression.Call(method, ps);
            var lambda = Expression.Lambda(call, instanceObj, writer);
            return (Func<object, IBufferWriter<byte>, long>)lambda.Compile();
        }
    }
}
