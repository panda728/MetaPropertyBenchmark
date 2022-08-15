using BenchmarkDotNet.Running;
using MetaPropertyBenchmark;

#if DEBUG
var test = new Benchmark();
test.N = 1000;
test.Setup();
test.Reflection();
test.ReflectionOp();
test.ExpressionTree();
test.ExpressionTreeOp();
test.ExpressionTreeOp2();
test.ExpressionTreeOp3();

#else
var summary = BenchmarkRunner.Run<Benchmark>();
#endif

