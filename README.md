# MetaPropertyBenchmark


### * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.301
  [Host]     : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT  [AttachedDebugger]
  DefaultJob : .NET 6.0.6 (6.0.622.26707), X64 RyuJIT


|            Method |      N |       Mean |     Error |    StdDev | Ratio | RatioSD |      Gen 0 | Allocated |
|------------------ |------- |-----------:|----------:|----------:|------:|--------:|-----------:|----------:|
|        Reflection |   1000 |   2.090 ms | 0.0415 ms | 0.0525 ms |  1.00 |    0.00 |    97.6563 |    414 KB |
|      ReflectionOp |   1000 |   2.101 ms | 0.0401 ms | 0.0507 ms |  1.01 |    0.03 |    97.6563 |    414 KB |
|    ExpressionTree |   1000 |   1.942 ms | 0.0372 ms | 0.0366 ms |  0.93 |    0.03 |    99.6094 |    414 KB |
|  ExpressionTreeOp |   1000 |   2.041 ms | 0.0408 ms | 0.0544 ms |  0.97 |    0.03 |    74.2188 |    305 KB |
| ExpressionTreeOp2 |   1000 |   2.288 ms | 0.0353 ms | 0.0330 ms |  1.10 |    0.03 |    82.0313 |    340 KB |
|                   |        |            |           |           |       |         |            |           |
|        Reflection | 100000 | 222.988 ms | 1.2983 ms | 1.0136 ms |  1.00 |    0.00 | 10000.0000 | 41,410 KB |
|      ReflectionOp | 100000 | 223.482 ms | 2.2000 ms | 1.9502 ms |  1.00 |    0.01 | 10000.0000 | 41,411 KB |
|    ExpressionTree | 100000 | 206.860 ms | 3.3221 ms | 2.9450 ms |  0.93 |    0.02 | 10000.0000 | 41,411 KB |
|  ExpressionTreeOp | 100000 | 212.401 ms | 2.5162 ms | 2.3536 ms |  0.95 |    0.01 |  7333.3333 | 30,473 KB |
| ExpressionTreeOp2 | 100000 | 234.261 ms | 2.1842 ms | 1.8239 ms |  1.05 |    0.01 |  8000.0000 | 33,989 KB |
