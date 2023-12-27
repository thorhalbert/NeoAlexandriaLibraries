
//using FFMpegCore;
//using FFMpegCore.Enums;
//using FFMpegCore.Pipes;

using Microsoft.AspNetCore.SignalR;
using System;
//using Xabe.FFmpeg;


namespace BlazorVideoStreaming;


public class VideoStreamingHub : Hub
{
    public async IAsyncEnumerable<byte[]> GetVideoStream(int start, int count, int delay)
    {

        var videoPath = "/cf/MoviesR/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP..mkv";
        using var outputStream = new FileStream(videoPath, FileMode.Open, FileAccess.Read);

        //using var outputStream = new MemoryStream();

        // Read the transcoded video stream in chunks and send it to the client

        Console.WriteLine("Start up GetVideoStream");

        byte[] buffer = new byte[4096];
        int bytesRead;
        while ((bytesRead = await outputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        {
            Console.WriteLine($"[Send Buffer {buffer.Length}]");
            yield return buffer;  // This isn't right - it should be bytesRead in length - bit this should do for the hack
        }
    }
}

//public class VideoStreamingService : BackgroundService
//{
//    private readonly IHubContext<VideoStreamingHub> _hubContext;

//    public VideoStreamingService(IHubContext<VideoStreamingHub> hubContext)
//    {
//        _hubContext = hubContext;
//        Console.WriteLine($"VideoStreamingService constructed");
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            // Read the video file
//            var videoPath = "/cf/MoviesR/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP..mkv";
//            using var outputStream = new FileStream(videoPath, FileMode.Open, FileAccess.Read);

//            //using var outputStream = new MemoryStream();



//            // Read the transcoded video stream in chunks and send it to the client

//            byte[] buffer = new byte[1024];
//            int bytesRead;
//            while ((bytesRead = await outputStream.ReadAsync(buffer, 0, buffer.Length, stoppingToken)) > 0)
//            {
//                Console.WriteLine($"Chunk: {bytesRead}");

//                await _hubContext.Clients.All.SendAsync("ReceiveVideoChunk", buffer, bytesRead);
//            }
//        }
//    }
//}

        //IConversion? conversionResult = null;
        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        if (conversionResult==null)
        //        {
        //            Thread.Sleep(1000);
        //            continue;
        //        }

        //        // Read the video file
        //        var videoPath = "/cf/MoviesR/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP..mkv";
        //        //using var fileStream = new FileStream(videoPath, FileMode.Open, FileAccess.Read);

        //        //Console.WriteLine($"Open: {videoPath}");

        //        //                       .FromFileInput(videoPath, true, options =>
        //        //           options.ForceFormat("hevc")        // Transcode - use GPU if we've got one
        //        //           .WithHardwareAcceleration(HardwareAccelerationDevice.Auto))

        //        using var outputStream = new MemoryStream();

        //        string output = "/dev/null";

        //        FFmpeg.SetExecutablesPath("/usr/bin/", "ffmpeg", "ffprobe");

        //        var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);

        //        conversionResult = FFmpeg.Conversions.New();
        //        conversionResult.OnDataReceived += ConversionResult_OnDataReceived;
        //        conversionResult = (IConversion?) await conversionResult
        //            .AddStream(mediaInfo.Streams)
        //            .SetOutput(output)
        //            .Start();

        //        Console.WriteLine("Stream Open");

        //        // Read the transcoded video stream in chunks and send it to the client
        //        //byte[] buffer = new byte[1024];
        //        //int bytesRead;
        //        //while ((bytesRead = await outputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //        //{
        //        //    Console.WriteLine($"Chunk: {bytesRead}");

        //        //    await _hubContext.Clients.All.SendAsync("ReceiveVideoChunk", buffer, bytesRead);
        //        //}


        //    }
        //}

        //private void ConversionResult_OnDataReceived(object sender, System.Diagnostics.DataReceivedEventArgs e)
        //{
        //    if (e.Data == null) return;
        //    Console.WriteLine($"Chunk: {e.Data.Length}");
        //     _hubContext.Clients.All.SendAsync("ReceiveVideoChunk", e.Data, e.Data.Length);
        //}

        // FFMpegCore fails due to some serialization problem (probably associated with core 8)
        //protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        //{
        //    while (!stoppingToken.IsCancellationRequested)
        //    {
        //        // Read the video file
        //        var videoPath = "/cf/MoviesR/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP/Blade.Runner.2049.2017.4K.UltraHD.BluRay.2160p.x264.TrueHD.Atmos.7.1.AC3-POOP..mkv";
        //        using var fileStream = new FileStream(videoPath, FileMode.Open, FileAccess.Read);

        //        Console.WriteLine($"Open: {videoPath}");

        //        //                       .FromFileInput(videoPath, true, options =>
        //        //           options.ForceFormat("hevc")        // Transcode - use GPU if we've got one
        //        //           .WithHardwareAcceleration(HardwareAccelerationDevice.Auto))

        //        using var outputStream = new MemoryStream();
        //        await FFMpegArguments
        //            .FromPipeInput(new StreamPipeSource(fileStream))
        //            .OutputToPipe(new StreamPipeSink(outputStream))
        //            .ProcessAsynchronously();

        //        Console.WriteLine("Stream Open");

        //        // Read the transcoded video stream in chunks and send it to the client
        //        byte[] buffer = new byte[1024];
        //        int bytesRead;
        //        while ((bytesRead = await outputStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
        //        {
        //            Console.WriteLine($"Chunk: {bytesRead}");

        //            await _hubContext.Clients.All.SendAsync("ReceiveVideoChunk", buffer, bytesRead);
        //        }


        //    }
        //}
    

