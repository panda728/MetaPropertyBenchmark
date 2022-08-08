# MetaPropertyBenchmark


### * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.301
  [Host]     : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT  [AttachedDebugger]
  DefaultJob : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT


|            Method |      N |       Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 | Allocated |
|------------------ |------- |-----------:|----------:|----------:|------:|--------:|-----------:|----------:|
|        Reflection |   1000 |   2.037 ms | 0.0268 ms | 0.0250 ms |  1.00 |    0.00 |    99.6094 |    414 KB |
|      ReflectionOp |   1000 |   2.022 ms | 0.0140 ms | 0.0117 ms |  1.00 |    0.01 |    97.6563 |    414 KB |
|    ExpressionTree |   1000 |   1.945 ms | 0.0175 ms | 0.0146 ms |  0.96 |    0.01 |    97.6563 |    414 KB |
|  ExpressionTreeOp |   1000 |   2.036 ms | 0.0241 ms | 0.0226 ms |  1.00 |    0.02 |    74.2188 |    305 KB |
| ExpressionTreeOp2 |   1000 |   2.162 ms | 0.0182 ms | 0.0162 ms |  1.06 |    0.01 |    82.0313 |    340 KB |
|                   |        |            |           |           |       |         |            |           |
|        Reflection | 100000 | 214.885 ms | 0.9381 ms | 0.7834 ms |  1.00 |    0.00 | 10000.0000 | 41,410 KB |
|      ReflectionOp | 100000 | 213.728 ms | 1.5480 ms | 1.3723 ms |  0.99 |    0.01 | 10000.0000 | 41,411 KB |
|    ExpressionTree | 100000 | 199.854 ms | 3.9867 ms | 4.5911 ms |  0.93 |    0.03 | 10000.0000 | 41,415 KB |
|  ExpressionTreeOp | 100000 | 207.632 ms | 2.1872 ms | 1.8264 ms |  0.97 |    0.01 |  7333.3333 | 30,472 KB |
| ExpressionTreeOp2 | 100000 | 227.747 ms | 1.8600 ms | 1.7399 ms |  1.06 |    0.01 |  8000.0000 | 33,989 KB |
