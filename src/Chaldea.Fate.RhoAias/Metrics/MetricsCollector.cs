using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace Chaldea.Fate.RhoAias;

internal interface IMetricsCollector
{
    List<Measurement<float>> GetSystemUsage();
}

internal class CollectorDefault : IMetricsCollector
{
    private readonly Process _process;

    public CollectorDefault()
    {
        _process = Process.GetCurrentProcess();
    }

    public List<Measurement<float>> GetSystemUsage()
    {
        var memInfo = GC.GetGCMemoryInfo();
        var cpuUsage = (float)_process.TotalProcessorTime.TotalSeconds;
        var memUsage = _process.WorkingSet64 / (float)memInfo.TotalAvailableMemoryBytes * 100;
        return new List<Measurement<float>>
        {
            new(cpuUsage, new KeyValuePair<string, object?>("label", "cpu")),
            new(memUsage, new KeyValuePair<string, object?>("label", "memory"))
        };
    }
}

[SupportedOSPlatform("windows")]
internal class CollectorWindows : IMetricsCollector
{
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _memCounter;

    public CollectorWindows()
    {
        _cpuCounter = new("Processor", "% Idle Time", "_Total");
        _memCounter = new("Memory", "% Committed Bytes In Use");
    }

    public List<Measurement<float>> GetSystemUsage()
    {
        var idle = _cpuCounter.NextValue();
        var cpuUsage = idle == 0 ? 0 : 100 - idle;
        return new List<Measurement<float>>
        {
            new(cpuUsage, new KeyValuePair<string, object?>("label", "cpu")),
            new(_memCounter.NextValue(), new KeyValuePair<string, object?>("label", "memory"))
        };
    }
}

[SupportedOSPlatform("linux")]
internal class CollectorLinux : IMetricsCollector
{
    private long prevIdle = 0L;
    private long prevTotal = 0L;

    public List<Measurement<float>> GetSystemUsage()
    {
        var memInfo = GetMemoryInfo();
        var memUsage = (memInfo.MemTotal - memInfo.MemFree) / (float)memInfo.MemTotal * 100;
        var cpuUsage = GetCpuUsage();
        return new List<Measurement<float>>
        {
            new(cpuUsage, new KeyValuePair<string, object?>("label", "cpu")),
            new(memUsage, new KeyValuePair<string, object?>("label", "memory"))
        };
    }

    private MemInfo GetMemoryInfo()
    {
        var lines = File.ReadAllLines("/proc/meminfo");
        var memInfo = new MemInfo();
        foreach (var line in lines)
        {
            var m = Regex.Match(line, @"(\w+):\s+(\d+)");
            if (m is { Success: true, Groups.Count: > 2 })
            {
                if (long.TryParse(m.Groups[2].ToString(), out var value))
                {
                    memInfo.Set(m.Groups[1].ToString(), value);
                }
            }
        }

        return memInfo;
    }

    private float GetCpuUsage()
    {
        var lines = File.ReadAllLines("/proc/stat");
        foreach (var line in lines)
        {
            if (line.StartsWith("cpu "))
            {
                var cols = lines[0].Split(" ")
                    .Skip(1)
                    .Where(x => x != string.Empty)
                    .Select(s => Convert.ToInt64(s.Trim()))
                    .ToArray();
                /*
                 * 0: user
                 * 1: nice
                 * 2: system
                 * 3: idle
                 * 4: iowait
                 * 5: irq
                 * 6: softirq
                 * 7: steal
                 */
                var currentIdle = cols[3] + cols[4];
                var currentUsed = cols[0] + cols[1] + cols[2] + cols[5] + cols[6] + cols[7];
                var currentTotal = currentIdle + currentUsed;
                if (prevIdle == 0)
                {
                    prevIdle = currentIdle;
                    prevTotal = currentTotal;
                    return 0;
                }

                var idle = currentIdle - prevIdle;
                var total = currentTotal - prevTotal;
                var idleP = idle / (float)total * 100;
                prevIdle = currentIdle;
                prevTotal = currentTotal;
                return 100 - idleP;
            }
        }

        return 0;
    }

    class MemInfo
    {
        private readonly Dictionary<string, long> _dic = new Dictionary<string, long>();

        public long MemTotal
        {
            get => _dic[nameof(MemTotal)];
            set => _dic[nameof(MemTotal)] = value;
        }

        public long MemFree
        {
            get => _dic[nameof(MemFree)];
            set => _dic[nameof(MemFree)] = value;
        }

        public long MemAvailable
        {
            get => _dic[nameof(MemAvailable)];
            set => _dic[nameof(MemAvailable)] = value;
        }

        public void Set(string key, long value)
        {
            _dic[key] = value;
        }
    }
}