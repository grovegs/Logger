```

BenchmarkDotNet v0.14.0, macOS Ventura 13.7 (22H123) [Darwin 22.6.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK 8.0.403
  [Host]     : .NET 8.0.10 (8.0.1024.46610), Arm64 RyuJIT AdvSIMD
  Job-GPFYEV : .NET 8.0.10 (8.0.1024.46610), Arm64 RyuJIT AdvSIMD

InvocationCount=1000  IterationCount=1000  LaunchCount=1  
UnrollFactor=1  WarmupCount=100  

```
| Method     | Mean     | Error    | StdDev    | Median   | Allocated |
|----------- |---------:|---------:|----------:|---------:|----------:|
| FileLogger | 224.2 ns |  0.71 ns |   6.15 ns | 222.3 ns |       1 B |
| ZLogger    | 352.5 ns | 12.36 ns | 115.92 ns | 411.3 ns |      41 B |
