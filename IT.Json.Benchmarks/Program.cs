using IT.Json.Benchmarks;

new EnumJsonConverterBenchmark().Test();
new FlagsEnumJsonConverterBenchmark().Test();

BenchmarkDotNet.Running.BenchmarkRunner.Run<EnumJsonConverterBenchmark>();
BenchmarkDotNet.Running.BenchmarkRunner.Run<FlagsEnumJsonConverterBenchmark>();

Console.WriteLine("End....");