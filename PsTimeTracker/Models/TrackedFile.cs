using Microsoft.Toolkit.Mvvm.ComponentModel;
using System;
using System.Timers;
using System.Windows.Threading;

namespace PSTimeTracker.Models;

/// <summary>
/// Represents a file that can be activated and updated.
/// </summary>
public class TrackedFile : ObservableObject
{
    /// <summary>
    /// Filename.
    /// </summary>
    public string FileName { get => _fileName; set { _fileName = value; OnPropertyChanged(nameof(FileName)); } }
    /// <summary>
    /// Indicates for how many seconds of time file was active.
    /// </summary>
    public int TrackedSeconds { get => _trackedSeconds; set { _trackedSeconds = value; OnPropertyChanged(nameof(TrackedSeconds)); } }
    /// <summary>
    /// Time when this file was created or first active.
    /// </summary>
    public DateTimeOffset AddedTime { get => _addedTime; set { _addedTime = value; OnPropertyChanged(nameof(AddedTime)); } }
    /// <summary>
    /// Time when this file was last active.
    /// </summary>
    public DateTimeOffset LastActiveTime { get => _lastActiveTime; set { _lastActiveTime = value; OnPropertyChanged(nameof(LastActiveTime)); } }
    /// <summary>
    /// Indicates that this file is active.
    /// </summary>
    public bool IsCurrentlyActive { get => _isCurrentlyActive; set { _isCurrentlyActive = value; OnPropertyChanged(nameof(IsCurrentlyActive)); } }

    private string _fileName;
    private int _trackedSeconds;
    private DateTimeOffset _addedTime;
    private DateTimeOffset _lastActiveTime;
    private bool _isCurrentlyActive;

    private readonly DispatcherTimer _activatedTimer;

    /// <summary>
    /// Empty tracked file.
    /// </summary>
    public static TrackedFile Empty { get; } = new TrackedFile("")
    {
        _trackedSeconds = -1,
        _addedTime = DateTimeOffset.MinValue,
        _lastActiveTime = DateTimeOffset.MinValue,
    };

    /// <summary>
    /// Creates an instance of a file with empty filename.
    /// </summary>
    public TrackedFile()
    {
        _activatedTimer = new DispatcherTimer();
        _activatedTimer.Interval = TimeSpan.FromSeconds(1);

        _fileName = "";
        _addedTime = DateTimeOffset.Now;
        _lastActiveTime = DateTimeOffset.MinValue;
    }

    /// <summary>
    /// Creates an instance of a file with a given filename.
    /// </summary>
    /// <param name="fileName"></param>
    public TrackedFile(string fileName) : this() { FileName = fileName; }

    /// <summary>
    /// Activates a file and updates its time for specified amount of seconds. File will be updated every second.
    /// </summary>
    /// <param name="seconds">File will be updated every second for this time. Value would be clamped to 0 if negative.</param>
    public void ActivateFor(int seconds)
    {
        IncrementTime();

        seconds = Math.Max(seconds, 0);
        _activatedTimer.Start();
        _activatedTimer.Tick += (_, _) =>
        {
            seconds--;
            if (seconds > 0)
                IncrementTime();
        };
    }

    /// <summary>
    /// Activates and updates a file.
    /// </summary>
    public void Activate()
    {
        IncrementTime();
    }

    /// <summary>
    /// Deactivates a file by setting <see cref="IsCurrentlyActive"/> to <see langword="false"/>.
    /// </summary>
    public void Deactivate()
    {
        _activatedTimer.Stop();
        IsCurrentlyActive = false;
    }

    /// <summary>
    /// Indicates that this tracked file is equal to empty.
    /// </summary>
    /// <returns><see langword="true"/> if FileName is null or empty or tracked seconds is less than 0 or added time is minValue. Otherwise <see langword="false"/>.</returns>
    public bool IsEmpty()
    {
        return string.IsNullOrWhiteSpace(FileName) || TrackedSeconds < 0 || AddedTime == DateTimeOffset.MinValue;
    }

    private void IncrementTime()
    {
        TrackedSeconds++;
        LastActiveTime = DateTime.Now;
        IsCurrentlyActive = true;
    }
}