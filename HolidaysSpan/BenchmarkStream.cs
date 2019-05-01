using System.IO;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;

namespace HolidaysSpan
{
  [CoreJob, MemoryDiagnoser]
  public class BenchmarkStream
  {
    private const string File = "holidays.csv";
    private HolidayProcessorClassicBenchmark _classic;
    private HolidayProcessorBenchmark _span;
    private Stream _fileStream;

    [GlobalSetup]
    public void Setup()
    {
      _fileStream = System.IO.File.Open(File, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    [IterationSetup]
    public void ResetStream()
    {
      _fileStream.Position = 0;
      var stream = new MemoryStream();
      _fileStream.CopyTo(stream);
      stream.Position = 0;
      _classic = new HolidayProcessorClassicBenchmark(stream);
      _span = new HolidayProcessorBenchmark(stream);
    }

    [Benchmark(Baseline = true)]
    public string Classic() => _classic.Run();

    [Benchmark]
    public Task<string> Span() => _span.Run();
  }
}
