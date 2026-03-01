using System;
using System.Diagnostics;
using System.Collections.Generic;

namespace SeedHunter.Utils
{
    /// <summary>
    /// Tracks patch call counts and performance during loading
    /// </summary>
    public static class PerformanceTracker
    {
        private static Dictionary<string, int> _callCounts = new Dictionary<string, int>();
        private static Dictionary<string, long> _totalTime = new Dictionary<string, long>();
        private static Stopwatch _sessionTimer = Stopwatch.StartNew();
        private static bool _enabled = true; // Enable for first 60 seconds of game

        public static void Track(string patchName, Action action)
        {
            if (!_enabled || _sessionTimer.ElapsedMilliseconds > 60000)
            {
                _enabled = false;
                action();
                return;
            }

            if (!_callCounts.ContainsKey(patchName))
            {
                _callCounts[patchName] = 0;
                _totalTime[patchName] = 0;
            }

            var sw = Stopwatch.StartNew();
            action();
            sw.Stop();

            _callCounts[patchName]++;
            _totalTime[patchName] += sw.ElapsedMilliseconds;
        }

        public static void LogStats()
        {
            if (_callCounts.Count == 0) return;

            SeedHunterPlugin.Log.LogWarning("=== PERFORMANCE TRACKER (First 60 seconds) ===");

            foreach (var kvp in _callCounts)
            {
                var avgMs = _totalTime[kvp.Key] / (double)kvp.Value;
                SeedHunterPlugin.Log.LogWarning(
                    $"{kvp.Key}: {kvp.Value} calls, {_totalTime[kvp.Key]}ms total, {avgMs:F2}ms avg"
                );
            }

            SeedHunterPlugin.Log.LogWarning("==============================================");
        }

        public static void Reset()
        {
            _callCounts.Clear();
            _totalTime.Clear();
            _sessionTimer.Restart();
            _enabled = true;
        }
    }
}
