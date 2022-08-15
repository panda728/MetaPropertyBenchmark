using System.Buffers;
using System.Linq.Expressions;
using System.Reflection;

namespace MetaPropertyBenchmark.ExpressionTreeOp3
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
        public SerializeDelegate<T> Formatter { get; init; }
    }

    public delegate long SerializeDelegate<T>(T value, ref IBufferWriter<byte> writer);
    public static class FormatterHelperExtention
    {
        readonly static Type _objectType = typeof(object);
        readonly static (MethodInfo method, Type? type)[] _methods =
            typeof(Formatter)
                .GetMethods()
                .Where(m => m.Name == "Serialize")
                .Select(m => (m, m.GetParameters()?.FirstOrDefault()?.ParameterType))
                .ToArray();

        readonly static MethodInfo _methodObject = _methods.Where(x => x.type == typeof(object)).First().method;
        readonly static ParameterExpression _writer = Expression.Parameter(typeof(IBufferWriter<byte>).MakeByRefType(), "w");

        public static SerializeDelegate<T> GenerateFormatter<T>(this PropertyInfo p)
        {
            if (p.DeclaringType == null || p.PropertyType.IsGenericType)
                return (T value, ref IBufferWriter<byte> writer) => Formatter.WriteEmpty(ref writer);

            var methodTyped = _methods.Where(x => x.type == p.PropertyType)?.Select(x => x.method)?.FirstOrDefault();
            var target = Expression.Parameter(p.DeclaringType, "i");
            var parameters = methodTyped == null
                ? new Expression[] { Expression.Convert(Expression.PropertyOrField(target, p.Name), _objectType), _writer }
                : new Expression[] { Expression.PropertyOrField(target, p.Name), _writer };

            var call = Expression.Call(methodTyped ?? _methodObject, parameters);
            var lambda = Expression.Lambda<SerializeDelegate<T>>(call, target, _writer);
            return lambda.Compile();
        }
    }
}
