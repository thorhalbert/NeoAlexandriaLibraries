public interface IMediaSource : IDisposable
{
    // Ready state of the MediaSource
    MediaSourceReadyState ReadyState { get; }

    // Whether the source has ended
    bool Ended { get; }

    // Duration of the media (in seconds)
    double? Duration { get; set; }

    // SourceBuffers associated with the MediaSource
    ISourceBufferList SourceBuffers { get; }

    // Events
    event EventHandler<MediaSourceEvent> OnError;
    event EventHandler OnEnded;

    // Methods
    ISourceBuffer AddSourceBuffer(string type);
    void RemoveSourceBuffer(ISourceBuffer sourceBuffer);
    void Play();
    void Pause();
    void Seek(double time);
}

public enum MediaSourceReadyState
{
    None,
    Opening,
    Open,
    Closed,
    Ended
}

public class MediaSourceEvent : EventArgs
{
    public string Message { get; private set; }

    public MediaSourceEvent(string message)
    {
        Message = message;
    }
}

public interface ISourceBufferList
{
    // Number of source buffers
    int Length { get; }

    // Get a specific source buffer by index
    ISourceBuffer GetItem(int index);

    // Add a new source buffer
    ISourceBuffer Add(string type);

    // Remove a source buffer
    void Remove(ISourceBuffer sourceBuffer);
}

public interface ISourceBuffer
{
    // Append data to the buffer
    void AppendData(byte[] data);

    // Set a timestamp for the appended data
    void SetTimestamp(double timestamp);

    // Remove data from the buffer
    void Remove(double start, double end);
}

public class MediaSource  // Wrap an IMediaSource, but don't implement as interface
{
    private readonly IMediaSource _jsMediaSource;
    private readonly List<ISourceBuffer> _sourceBuffers;
    //private MediaSourceReadyState _readyState;
    //private bool _ended;
    //private double? _duration;

    public event EventHandler<MediaSourceEvent> OnError;
    public event EventHandler OnEnded;

    public MediaSource(IMediaSource jsSource)
    {
        _jsMediaSource = jsSource;

        _sourceBuffers = [];
        //_readyState = MediaSourceReadyState.None;
        //_ended = false;
        //_duration = null;
    }

    public MediaSourceReadyState ReadyState => _jsMediaSource.ReadyState; // _readyState;
    public bool Ended => _jsMediaSource.Ended;
    public double? Duration {
        get => _jsMediaSource.Duration;
        set {
            if (ReadyState != MediaSourceReadyState.Open)
            {
                throw new InvalidOperationException("Cannot set duration when MediaSource is not open.");
            }
            _jsMediaSource.Duration = value;
        }
    }
    public ISourceBufferList SourceBuffers => new SourceBufferList(_sourceBuffers);

    public void AddSourceBuffer(Byte[]? inbuff)
    {

    }


    public ISourceBuffer AddSourceBuffer(string type)
    {
        if (ReadyState != MediaSourceReadyState.Open)
        {
            throw new InvalidOperationException("Cannot add source buffers when MediaSource is not open.");
        }

        var sourceBuffer = new SourceBuffer();
        _sourceBuffers.Add(sourceBuffer);
        return sourceBuffer;
    }

    public void RemoveSourceBuffer(ISourceBuffer sourceBuffer)
    {
        if (ReadyState != MediaSourceReadyState.Open)
        {
            throw new InvalidOperationException("Cannot remove source buffers when MediaSource is not open.");
        }

        _sourceBuffers.Remove(sourceBuffer);
    }

    public void Play()
    {
        // Implement logic to start playback
        //ReadyState = MediaSourceReadyState.Ended;
        //_ended = true;

        _jsMediaSource.Play();
    }

    public void Pause()
    {
        // Implement logic to pause playback
        _jsMediaSource.Pause();
    }

    public void Seek(double time)
    {
        // Implement logic to seek to the specified time

        _jsMediaSource.Seek(time);
    }

    public void Dispose()
    {
        // Implement logic to release resources associated with the MediaSource
    }

    private class SourceBufferList : ISourceBufferList
    {
        private readonly List<ISourceBuffer> _sourceBuffers;

        public SourceBufferList(List<ISourceBuffer> sourceBuffers)
        {
            _sourceBuffers = sourceBuffers;
        }

        public int Length => _sourceBuffers.Count;

        public ISourceBuffer GetItem(int index) => _sourceBuffers[index];

        public ISourceBuffer Add(string type) => throw new NotImplementedException();

        public void Remove(ISourceBuffer sourceBuffer) => _sourceBuffers.Remove(sourceBuffer);
    }

    private class SourceBuffer : ISourceBuffer
    {
        public void AppendData(byte[] data)
        {
            // Implement logic to append data to the source buffer
        }

        public void SetTimestamp(double timestamp)
        {
            // Implement logic to set timestamps for the appended data
        }

        public void Remove(double start, double end)
        {
            // Implement logic to remove data from the source buffer
        }
    }
}