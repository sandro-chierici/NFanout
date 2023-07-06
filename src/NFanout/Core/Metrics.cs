// MIT License
// 
// Copyright (c) 2023 Sandro Chierici <sandro.chierici@gmail.com>
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.


using System.Collections.Concurrent;

namespace NFanout.Core;

/// <summary>
/// All time in millis
/// </summary>
public class MetricsData
{
    public long JobsCompleted { get; set; }
    public void AddJobCompleted() => JobsCompleted++;

    public long JobsQueued { get; private set; }
    public void AddJobQueued() => JobsQueued++;

    public long JobsStarted { get; set; }
    public void AddJobStarted() => JobsStarted++;

    public DateTime? LastJobUTC { get; set; }
    public long LastJobDuration { get; set; }
    public double AverageJobDuration => JobsCompleted > 0 ? TotalJobTime / JobsCompleted : 0;
    public long TotalJobTime { get; private set; }
    public long AddJobTime(long jobTime) => TotalJobTime += jobTime;
}

/// <summary>
/// Metrics gathered
/// </summary>
public class Metrics
{
    private readonly ConcurrentDictionary<string, MetricsData> _metrics;
    public Metrics()
    {
        _metrics = new ConcurrentDictionary<string, MetricsData>();
    }

    public MetricsData GetMetric(string name)
    {
        return _metrics.GetOrAdd(name, (nm) => new MetricsData());
    }
}
