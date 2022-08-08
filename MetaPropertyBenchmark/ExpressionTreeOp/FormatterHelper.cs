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
            => p.PropertyType.IsPrimitive 
                ? p.GeneratePrimitive()
                : p.GenerateObject();
        
        static Func<object, IBufferWriter<byte>, long> GeneratePrimitive(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsGenericType)
                return (o, v) => Formatter.WriteEmpty(v);
            // Func<object, long> getCategoryId = (i,writer) => Formatter.Serialize((i as T).CategoryId, writer);
            var instanceObj = Expression.Parameter(_objectType, "i");
            var instance = Expression.Convert(instanceObj, propertyInfo.DeclaringType);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            var property = Expression.Property(instance, propertyInfo);
            var ps = new Expression[] { property, writer };
            var method = typeof(Formatter).GetMethod("Serialize", new Type[] { propertyInfo.PropertyType, typeof(IBufferWriter<byte>) });
            if (method == null)
                return (o, v) => Formatter.WriteEmpty(v);

            var call = Expression.Call(method, ps);
            var lambda = Expression.Lambda(call, instanceObj, writer);
            return (Func<object, IBufferWriter<byte>, long>)lambda.Compile();
        }

        static Func<object, IBufferWriter<byte>, long> GenerateObject(this PropertyInfo propertyInfo)
        {
            if (propertyInfo.PropertyType.IsGenericType)
                return (o, v) => Formatter.WriteEmpty(v);

            // Func<object, long> getCategoryId = (i,writer) => Formatter.Serialize((object)((i as T).CategoryId), writer);
            var instanceObj = Expression.Parameter(_objectType, "i");
            var instance = Expression.Convert(instanceObj, propertyInfo.DeclaringType);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            var property = Expression.Property(instance, propertyInfo);
            var propertyObject = Expression.Convert(property, _objectType);
            var ps = new Expression[] { propertyObject, writer };
            var method = typeof(Formatter).GetMethod("Serialize", new Type[] { _objectType, typeof(IBufferWriter<byte>) });
            if (method == null)
                return (o, v) => Formatter.WriteEmpty(v);

            var call = Expression.Call(method, ps);
            var lambda = Expression.Lambda(call, instanceObj, writer);
            return (Func<object, IBufferWriter<byte>, long>)lambda.Compile();
        }
    }
}
