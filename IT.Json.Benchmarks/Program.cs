using IT.Json.Benchmarks;

//new EnumJsonConverterBenchmark().Test();
new SequenceEnumJsonConverterBenchmark().Test();
//new FlagsEnumJsonConverterBenchmark().Test();
//new Base64JsonConverterBenchmark().Test();
//new SequenceBase64JsonConverterBenchmark().Test();

//BenchmarkDotNet.Running.BenchmarkRunner.Run<EnumJsonConverterBenchmark>();
BenchmarkDotNet.Running.BenchmarkRunner.Run<SequenceEnumJsonConverterBenchmark>();
//BenchmarkDotNet.Running.BenchmarkRunner.Run<FlagsEnumJsonConverterBenchmark>();
//BenchmarkDotNet.Running.BenchmarkRunner.Run<Base64JsonConverterBenchmark>();
//BenchmarkDotNet.Running.BenchmarkRunner.Run<SequenceBase64JsonConverterBenchmark>();

Console.WriteLine("End....");