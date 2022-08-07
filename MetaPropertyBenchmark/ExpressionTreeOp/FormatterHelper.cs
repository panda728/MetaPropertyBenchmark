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
            Formatter = FormatterHelperExtention.GenerateEncodedGetterLambda(p);
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

        public static Func<object, IBufferWriter<byte>, long> GenerateEncodedGetterLambda(PropertyInfo p)
            => IsSupported(p.PropertyType)
                ? p.GeneratePrimitiveLambda()
                : p.GenerateObjectLambda();

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

        static Func<object, IBufferWriter<byte>, long> GeneratePrimitiveLambda(this PropertyInfo propertyInfo)
        {
            var instanceObj = Expression.Parameter(_objectType, "i");
            var instance = Expression.Convert(instanceObj, propertyInfo.DeclaringType);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            var property = Expression.Property(instance, propertyInfo);
            var ps = new Expression[] { property, writer };
            var method = typeof(ColumnFormatter).GetMethod("Serialize", new Type[] { propertyInfo.PropertyType, typeof(IBufferWriter<byte>) });
            if (method == null)
                return (o, v) => ColumnFormatter.WriteEmptyCoulumn(o, v);

            var call = Expression.Call(method, ps);
            var d = Expression.Lambda(call, instanceObj, writer).Compile();
            return (Func<object, IBufferWriter<byte>, long>)d;
        }

        static Func<object, IBufferWriter<byte>, long> GenerateObjectLambda(this PropertyInfo propertyInfo)
        {
            var instanceObj = Expression.Parameter(_objectType, "i");
            var instance = Expression.Convert(instanceObj, propertyInfo.DeclaringType);
            var writer = Expression.Parameter(typeof(IBufferWriter<byte>), "writer");
            var property = Expression.Property(instance, propertyInfo);
            var propertyObject = Expression.Convert(property, _objectType);
            var ps = new Expression[] { propertyObject, writer };
            var method = typeof(ColumnFormatter).GetMethod("Serialize", new Type[] { _objectType, typeof(IBufferWriter<byte>) });
            if (method == null)
                return (o, v) => ColumnFormatter.WriteEmptyCoulumn(o, v);

            var call = Expression.Call(method, ps);
            var d = Expression.Lambda(call, instanceObj, writer).Compile();
            return (Func<object, IBufferWriter<byte>, long>)d;
        }
    }
}
