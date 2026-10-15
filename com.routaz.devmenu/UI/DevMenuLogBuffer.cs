using System.Collections.Generic;
using UnityEngine;

namespace DevMenuTool.UI
{
    public readonly struct DevMenuLogEntry
    {
        public readonly LogType Type;
        public readonly string Message;

        public DevMenuLogEntry(LogType type, string message)
        {
            Type = type;
            Message = message;
        }
    }

    /// <summary>Fixed-size ring buffer of the most recent log lines (first line of each message only).</summary>
    public sealed class DevMenuLogBuffer
    {
        public const int DefaultCapacity = 5;

        private readonly DevMenuLogEntry[] _entries;
        private int _start;

        public int Count { get; private set; }

        /// <summary>Changes on every <see cref="Add"/>, so a view can redraw only when something new arrived.</summary>
        public int Version { get; private set; }

        public DevMenuLogBuffer(int capacity = DefaultCapacity)
        {
            _entries = new DevMenuLogEntry[capacity];
        }

        public void Add(LogType type, string message)
        {
            var entry = new DevMenuLogEntry(type, FirstLine(message));

            if (Count < _entries.Length)
            {
                _entries[(_start + Count) % _entries.Length] = entry;
                Count++;
            }
            else
            {
                _entries[_start] = entry;
                _start = (_start + 1) % _entries.Length;
            }

            Version++;
        }

        public IReadOnlyList<DevMenuLogEntry> Snapshot()
        {
            var result = new DevMenuLogEntry[Count];
            for (int i = 0; i < Count; i++)
                result[i] = _entries[(_start + i) % _entries.Length];
            return result;
        }

        private static string FirstLine(string message)
        {
            if (string.IsNullOrEmpty(message)) return string.Empty;
            int end = message.IndexOfAny(new[] { '\r', '\n' });
            return end < 0 ? message : message.Substring(0, end);
        }
    }
}
