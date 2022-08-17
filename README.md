# MetaPropertyBenchmark


### // * Summary *

BenchmarkDotNet=v0.13.1, OS=Windows 10.0.22000
Intel Core i7-10610U CPU 1.80GHz, 1 CPU, 8 logical and 4 physical cores
.NET SDK=6.0.400
  [Host]   : .NET 6.0.8 (6.0.822.36306), X64 RyuJIT  [AttachedDebugger]
  ShortRun : .NET 6.0.8 (6.0.822.36306), X64 RyuJIT

Job=ShortRun  IterationCount=3  LaunchCount=1
WarmupCount=3

|            Method |      N |       Mean |      Error |     StdDev | Ratio | RatioSD |      Gen 0 | Allocated |
|------------------ |------- |-----------:|-----------:|-----------:|------:|--------:|-----------:|----------:|
|        Reflection |   1000 |   2.480 ms |   2.483 ms |  0.1361 ms |  1.00 |    0.00 |    97.6563 |    414 KB |
|      ReflectionOp |   1000 |   2.409 ms |   1.641 ms |  0.0900 ms |  0.97 |    0.02 |    97.6563 |    414 KB |
|    ExpressionTree |   1000 |   2.249 ms |   5.562 ms |  0.3049 ms |  0.91 |    0.18 |    97.6563 |    414 KB |
|  ExpressionTreeOp |   1000 |   2.246 ms |   1.493 ms |  0.0819 ms |  0.91 |    0.02 |    74.2188 |    305 KB |
| ExpressionTreeOp2 |   1000 |   2.505 ms |   1.562 ms |  0.0856 ms |  1.01 |    0.09 |    82.0313 |    340 KB |
| ExpressionTreeOp3 |   1000 |   2.246 ms |   1.246 ms |  0.0683 ms |  0.91 |    0.07 |    74.2188 |    305 KB |
|                   |        |            |            |            |       |         |            |           |
|        Reflection | 100000 | 244.169 ms | 106.295 ms |  5.8264 ms |  1.00 |    0.00 | 10000.0000 | 41,411 KB |
|      ReflectionOp | 100000 | 234.878 ms | 298.596 ms | 16.3671 ms |  0.96 |    0.05 | 10000.0000 | 41,413 KB |
|    ExpressionTree | 100000 | 212.888 ms | 228.749 ms | 12.5385 ms |  0.87 |    0.03 | 10000.0000 | 41,411 KB |
|  ExpressionTreeOp | 100000 | 222.624 ms | 275.768 ms | 15.1158 ms |  0.91 |    0.08 |  7000.0000 | 30,473 KB |
| ExpressionTreeOp2 | 100000 | 254.164 ms | 240.593 ms | 13.1877 ms |  1.04 |    0.04 |  8000.0000 | 33,989 KB |
| ExpressionTreeOp3 | 100000 | 233.606 ms | 241.625 ms | 13.2443 ms |  0.96 |    0.05 |  7000.0000 | 30,473 KB |
