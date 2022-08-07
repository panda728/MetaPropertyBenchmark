using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Bogus;
using static Bogus.DataSets.Name;

namespace MetaPropertyBenchmark
{
    [MarkdownExporterAttribute.GitHub]
    [ShortRunJob]
    [MemoryDiagnoser]
    public class Benchmark
    {
        List<User>? _users;
        string _workPath;

        readonly MetaPropertyBenchmark.Reflection.Builder _builderRef = new();
        readonly MetaPropertyBenchmark.ReflectionOp.Builder _builderRefOp = new();
        readonly MetaPropertyBenchmark.ExpressionTree.Builder _builderExp = new();
        readonly MetaPropertyBenchmark.ExpressionTreeOp.Builder _builderExpOp = new();
        readonly MetaPropertyBenchmark.ExpressionTreeOp2.Builder _builderExpOp2 = new();

        public Benchmark()
        {
            _workPath = Path.Combine("work");
            if (!Directory.Exists(_workPath))
                Directory.CreateDirectory(_workPath);
        }

        [Params(1000, 100000)]
        public int N = 10;

        [GlobalSetup]
        public void Setup()
        {

            //_builderRef.Compile(typeof(User));
            //_builderExp.Compile(typeof(User));

            Randomizer.Seed = new Random(8675309);

            var fruit = new[] { "apple", "banana", "orange", "strawberry", "kiwi" };

            var orderIds = 0;
            var testOrders = new Faker<Order>()
                .StrictMode(true)
                .RuleFor(o => o.OrderId, f => orderIds++)
                .RuleFor(o => o.Item, f => f.PickRandom(fruit))
                .RuleFor(o => o.Quantity, f => f.Random.Number(1, 10))
                .RuleFor(o => o.LotNumber, f => f.Random.Int(0, 100).OrNull(f, .8f));

            var userIds = 0;
            var testUsers = new Faker<User>()
                .CustomInstantiator(f => new User(userIds++, f.Random.Replace("###-##-####")))
                .RuleFor(u => u.Gender, f => f.PickRandom<Gender>())
                .RuleFor(u => u.FirstName, (f, u) => f.Name.FirstName(u.Gender))
                .RuleFor(u => u.LastName, (f, u) => f.Name.LastName(u.Gender))
                .RuleFor(u => u.Avatar, f => f.Internet.Avatar())
                .RuleFor(u => u.UserName, (f, u) => f.Internet.UserName(u.FirstName, u.LastName))
                .RuleFor(u => u.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName))
                .RuleFor(u => u.SomethingUnique, f => $"Value {f.UniqueIndex}")
                .RuleFor(u => u.CreateTime, f => DateTime.Now)
                .RuleFor(u => u.SomeGuid, f => Guid.NewGuid())
                .RuleFor(u => u.CartId, f => Guid.NewGuid())
                .RuleFor(u => u.FullName, (f, u) => u.FirstName + " " + u.LastName)
                .RuleFor(u => u.Orders, f => testOrders.Generate(3).ToList())
                .FinishWith((f, u) =>
                {
                    //Console.WriteLine("User Created! Id={0}", u.Id);
                });

            _users = testUsers.Generate(N);
        }
        private static Stream CreateStream(string fileName)
        {
#if DEBUG
            return new FileStream(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
#else
            return new FakeStream();
#endif
        }

        [Benchmark(Baseline = true)]
        public void Reflection()
        {
            if (_users == null)
                throw new ApplicationException("users is null");

            using var stream = CreateStream(Path.Combine(_workPath, "Reflection.txt"));
            _builderRef.Run(stream, _users);
        }
        [Benchmark]
        public void ReflectionOp()
        {
            if (_users == null)
                throw new ApplicationException("users is null");

            using var stream = CreateStream(Path.Combine(_workPath, "ReflectionOp.txt"));
            _builderRefOp.Run(stream, _users);
        }
        [Benchmark]
        public void ExpressionTree()
        {
            if (_users == null)
                throw new ApplicationException("users is null");

            using var stream = CreateStream(Path.Combine(_workPath, "ExpressionTree.txt"));
            _builderExp.Run(stream, _users);
        }
        [Benchmark]
        public void ExpressionTreeOp()
        {
            if (_users == null)
                throw new ApplicationException("users is null");

            using var stream = CreateStream(Path.Combine(_workPath, "ExpressionTreeOp.txt"));
            _builderExpOp.Run(stream, _users);
        }
        [Benchmark]
        public void ExpressionTreeOp2()
        {
            if (_users == null)
                throw new ApplicationException("users is null");

            using var stream = CreateStream(Path.Combine(_workPath, "ExpressionTreeOp2.txt"));
            _builderExpOp2.Run(stream, _users);
        }
    }
}
