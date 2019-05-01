using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace HolidaysSpan
{
  [CoreJob, MemoryDiagnoser]
  public class BenchmarkFile
  {
    private const string File = "holidays.csv";
    private HolidayProcessorClassicBenchmark _classic;
    private HolidayProcessorBenchmark _span;

    [GlobalSetup]
    public void Setup()
    {
      _classic = new HolidayProcessorClassicBenchmark(File);
      _span = new HolidayProcessorBenchmark(File);
    }

    [Benchmark(Baseline = true)]
    public string Classic() => _classic.Run();

    [Benchmark]
    public Task<string> Span() => _span.Run();
  }
}
