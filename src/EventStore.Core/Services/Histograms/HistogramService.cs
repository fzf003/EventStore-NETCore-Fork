﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using HdrHistogram;

namespace EventStore.Core.Services.Histograms
{
    //histogram service is just a static class used by everyone else if histograms are enabled.
    public static class HistogramService
    {
        private const long NUMBEROFNS = 1000000000L;
        private static readonly Stopwatch _stopwatch = new Stopwatch();
        private static readonly ConcurrentDictionary<string, LongHistogram> Histograms = new ConcurrentDictionary<string, LongHistogram>();

        static HistogramService()
        {
            _stopwatch.Start();
        }

        public static LongHistogram GetHistogram(string name)
        {
            Histograms.TryGetValue(name, out LongHistogram ret);
            return ret;
        }

        public static Measurement Measure(string name)
        {
            var histogram = GetHistogram(name);
            return new Measurement { watch = _stopwatch, Start = _stopwatch.ElapsedTicks, Histogram = histogram };
        }

        public static void SetValue(string name, long value)
        {
            if (value >= NUMBEROFNS) return;
            if (name == null) return;
            if (!Histograms.TryGetValue(name, out LongHistogram hist)) { return; }
            lock (hist)
            {
                hist.RecordValue(value);
            }
        }

        public static void CreateHistogram(string name)
        {
            Histograms.TryAdd(name, new LongHistogram(NUMBEROFNS, 3));
        }

        public static void CreateHistograms()
        {
            CreateHistogram("writer-flush");
            CreateHistogram("chaser-wait");
            CreateHistogram("chaser-flush");
            CreateHistogram("reader-readevent");
            CreateHistogram("reader-streamrange");
            CreateHistogram("reader-allrange");
            CreateHistogram("request-manager");
            CreateHistogram("tcp-send");
            CreateHistogram("http-send");
        }

        public static void StartJitterMonitor()
        {
            CreateHistogram("jitter");
            Task.Factory.StartNew(() =>
            {
                var spinner = new SpinWait();
                var watch = new Stopwatch();
                watch.Start();
                while (true)
                {
                    using (Measure("jitter"))
                    {
                  //Thread.Sleep(1);
                  spinner.SpinOnce();
                    }
                }
            }, CancellationToken.None, TaskCreationOptions.DenyChildAttach | TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }
    }


    public struct Measurement : IDisposable
    {
        public Stopwatch watch;
        public LongHistogram Histogram;
        public long Start;

        public void Dispose()
        {
            if (Histogram == null) return;
            lock (Histogram)
            {
                var valueToRecord = (((double)watch.ElapsedTicks - Start) / Stopwatch.Frequency) * 1000000000;
                if (valueToRecord < HighestPowerOf2(Histogram.HighestTrackableValue))
                {
                    Histogram.RecordValue((long)valueToRecord);
                }
            }
        }
        private static long HighestPowerOf2(long x)
        {
            x--;
            x |= (x >> 1);
            x |= (x >> 2);
            x |= (x >> 4);
            x |= (x >> 8);
            x |= (x >> 16);
            return (x + 1);
        }

    }
}
