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


namespace NFanout.Core;

public class MessageDTO
{
    /// <summary>
    /// Payload
    /// </summary>
    public object Payload { get; }
    /// <summary>
    /// Id coda di elaborazione
    /// </summary>
    public string? QueueKey { get; set; }
    /// <summary>
    /// Timestamp UTC inizio elaborazione
    /// </summary>
    public long StartUtc { get; }
    /// <summary>
    /// Total duration in millis
    /// </summary>
    public long DurationMillis { get; set; }

    public MessageDTO(object payload, long timestamp)
    {
        Payload = payload;
        StartUtc = timestamp;
    }
}

/// <summary>
/// Data passed to worker's action
/// </summary>
/// <param name="Payload"></param>
/// <param name="StartUTC"></param>
public record WorkDataRecord(string Key, object Payload, long StartUTC);
