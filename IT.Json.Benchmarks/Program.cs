using IT.Json.Benchmarks;

//new EnumJsonConverterBenchmark().Test();
//new FlagsEnumJsonConverterBenchmark().Test();
new Base64JsonConverterBenchmark().Test();

//BenchmarkDotNet.Running.BenchmarkRunner.Run<EnumJsonConverterBenchmark>();
//BenchmarkDotNet.Running.BenchmarkRunner.Run<FlagsEnumJsonConverterBenchmark>();
BenchmarkDotNet.Running.BenchmarkRunner.Run<Base64JsonConverterBenchmark>();

Console.WriteLine("End....");