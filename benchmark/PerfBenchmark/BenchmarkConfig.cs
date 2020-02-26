using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Environments;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Exporters.Csv;
using BenchmarkDotNet.Jobs;

namespace PerfBenchmark
{
    public class BenchmarkConfig : ManualConfig
    {
        public BenchmarkConfig()
        {
            // run quickly:)
            var baseConfig = Job.ShortRun.WithIterationCount(1).WithWarmupCount(1);

            // Add(baseConfig.With(Runtime.Clr).With(Jit.RyuJit).With(Platform.X64));
            Add(baseConfig.With(CoreRuntime.Core31).With(Jit.RyuJit).With(Platform.X64));

            Add(MarkdownExporter.GitHub);
            Add(CsvExporter.Default);
            Add(MemoryDiagnoser.Default);
        }
    }
}