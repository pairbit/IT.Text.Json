using IT.Json.Benchmarks;

//new EnumJsonConverterBenchmark().Test();
//BenchmarkDotNet.Running.BenchmarkRunner.Run<EnumJsonConverterBenchmark>();

//new SequenceEnumJsonConverterBenchmark().Test();
//BenchmarkDotNet.Running.BenchmarkRunner.Run<SequenceEnumJsonConverterBenchmark>();

//new FlagsEnumJsonConverterBenchmark().Test();
//BenchmarkDotNet.Running.BenchmarkRunner.Run<FlagsEnumJsonConverterBenchmark>();

//new SequenceFlagsEnumJsonConverterBenchmark().Test();
//BenchmarkDotNet.Running.BenchmarkRunner.Run<SequenceFlagsEnumJsonConverterBenchmark>();

//new Base64JsonConverterBenchmark().Test();
//BenchmarkDotNet.Running.BenchmarkRunner.Run<Base64JsonConverterBenchmark>();

//new SequenceBase64JsonConverterBenchmark().Test();
//BenchmarkDotNet.Running.BenchmarkRunner.Run<SequenceBase64JsonConverterBenchmark>();

await new RentedArrayBase64JsonConverterBenchmark().Test();
BenchmarkDotNet.Running.BenchmarkRunner.Run<RentedArrayBase64JsonConverterBenchmark>();

Console.WriteLine("End....");