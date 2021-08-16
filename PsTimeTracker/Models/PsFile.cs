﻿using System;
using PropertyChanged;

namespace PSTimeTracker.Models
{
    [AddINotifyPropertyChangedInterface]
    public class PsFile
    {
        public string? FileName { get; set; }
        public int TrackedSeconds { get; set; }
        public DateTimeOffset FirstActiveTime { get; set; }
        public DateTimeOffset LastActiveTime { get; set; }
        public bool IsCurrentlyActive { get; set; }
        public bool IsSelected { get; set; }
    }
}
