using System;
using System.Buffers;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.IO.Pipelines;
using System.Text;
using System.Threading.Tasks;

namespace HolidaysSpan
{
  public class HolidayProcessorIncremental
  {
    private readonly byte NewLineByte;
    private readonly byte CommaByte;
    private readonly byte SlashByte;
    private readonly Dictionary<DayOfWeek, int> _days = new Dictionary<DayOfWeek, int>(5);

    public HolidayProcessorIncremental()
    {
      NewLineByte = Encoding.UTF8.GetBytes(Environment.NewLine)[0];
      CommaByte = Encoding.UTF8.GetBytes(",")[0];
      SlashByte = Encoding.UTF8.GetBytes("/")[0];
      _days.Add(DayOfWeek.Monday, 0);
      _days.Add(DayOfWeek.Tuesday, 0);
      _days.Add(DayOfWeek.Wednesday, 0);
      _days.Add(DayOfWeek.Thursday, 0);
      _days.Add(DayOfWeek.Friday, 0);
    }

    public async Task Run()
    {
      var pipe = new Pipe(new PipeOptions(null, null, null, 4096, 2048, 1024, true));
      var readingTask = ReadFile(pipe.Writer);
      var processingTask = ProcessFile(pipe.Reader);
      await Task.WhenAll(readingTask, processingTask);
      PrintResult();
    }

    private async Task ReadFile(PipeWriter writer)
    {
      using (var file = File.Open("holidays.csv", FileMode.Open, FileAccess.Read, FileShare.Read))
      {
        while (true)
        {
          var memory = writer.GetMemory();
          int count = file.Read(memory.Span);
          if (count == 0)
            break;
          writer.Advance(count);
          var result = await writer.FlushAsync();
          if (result.IsCompleted)
            break;
        }
        writer.Complete();
      }
    }

    private async Task ProcessFile(PipeReader reader)
    {
      while (true)
      {
        var readResult = await reader.ReadAsync();
        ReadOnlySequence<byte> buffer = readResult.Buffer;
        SequencePosition? endOfLinePos = buffer.PositionOf(NewLineByte);
        while (endOfLinePos.HasValue)
        {
          ProcessLine(buffer.Slice(0, endOfLinePos.Value));
          buffer = buffer.Slice(buffer.GetPosition(1, endOfLinePos.Value));
          endOfLinePos = buffer.PositionOf(NewLineByte);
        }
        reader.AdvanceTo(buffer.Start, buffer.End);

        if (readResult.IsCompleted)
          break;
      }
      reader.Complete();
    }

    private void ProcessLine(ReadOnlySequence<byte> data)
    {
      if (data.IsSingleSegment)
      {
        ProcessLine(data.First.Span);
      }
      else
      {
        byte[] rentedStorage = ArrayPool<byte>.Shared.Rent((int)data.Length);
        data.CopyTo(rentedStorage);
        ProcessLine(rentedStorage.AsSpan(0, (int)data.Length));
        ArrayPool<byte>.Shared.Return(rentedStorage);
      }
    }

    private void ProcessLine(ReadOnlySpan<byte> span)
    {
      var lastComma = span.LastIndexOf(CommaByte);
      var endDate = ParseDate(span.Slice(lastComma + 1));
      span = span.Slice(0, lastComma);
      lastComma = span.LastIndexOf(CommaByte);
      var startDate = ParseDate(span.Slice(lastComma + 1));
      ProcessDates(startDate, endDate);
    }

    private DateTime ParseDate(ReadOnlySpan<byte> span)
    {
      span = ParseNumber(span, out int year);
      span = ParseNumber(span, out int month);
      ParseNumber(span, out int day);
      return new DateTime(year, month, day);
    }

    private ReadOnlySpan<byte> ParseNumber(ReadOnlySpan<byte> span, out int parsed)
    {
      var slashIndex = span.LastIndexOf(SlashByte);
      Utf8Parser.TryParse(span.Slice(slashIndex + 1), out parsed, out var dummy);
      if (slashIndex < 0)
        return Span<byte>.Empty;
      return span.Slice(0, slashIndex);
    }

    private void ProcessDates(DateTime startDate, DateTime endDate)
    {
      while (startDate.Date <= endDate.Date)
      {
        if (startDate.Date.DayOfWeek != DayOfWeek.Saturday && startDate.Date.DayOfWeek != DayOfWeek.Sunday)
          _days[startDate.Date.DayOfWeek]++;
        startDate = startDate.AddDays(1);
      }
    }

    private void PrintResult()
    {
      StringBuilder sb = new StringBuilder();
      foreach (var entry in _days)
      {
        sb.Append(entry.Key);
        sb.Append(": ");
        sb.Append(entry.Value);
        sb.AppendLine();
      }
      Console.WriteLine(sb.ToString());
    }
  }
}
