using CommandLine;
using MongoDB.Driver;
using NeoBakedVolumes.Mongo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCliFunctions;

/// <summary>
/// This is mostly to test baked files, but not reason we can't shake the teeth out of the
/// two physical file types too just to make sure.
/// 
/// Make sure to do a checksum first of beginning to end of both files to make sure the test is valid
/// 
/// Start zero, length end - compute both checksums
/// 
/// Pick random between beginning and end, and an end from start to end - checksum both
/// </summary>

[Verb("excercise-seek", HelpText = "Do random seeks on real file and virtual file and comprare")]
public class ExerciseSeek : ICommandCallable
{
    public IMongoDatabase db { get; set; }

    [Value(0)]
    public string RealFile { get; set; }
    [Value(1)]
    public string VirtualFile { get; set; }

    [Option('i', "iterations", Required = false, Default = 100, HelpText = "Number of test iterations")]
    public int  Iterations { get; set; }

    Random rand;

    public void RunCommand()
    {
        var real = new FileInfo(RealFile);
        var virt = new FileInfo(VirtualFile);

        if (real.Length != virt.Length)
            throw new ArgumentException($"Real Length={real.Length}, Virtual={virt.Length} - mismatch");

        rand = new Random(DateTime.Now.Millisecond);

        Console.WriteLine($"Real: {RealFile}");
        Console.WriteLine($"Virtual: {VirtualFile}");

        var realOpen = new BinaryReader(real.OpenRead());
        var realStream = realOpen.BaseStream;

        var virtOpen = new BinaryReader(virt.OpenRead());
        var virtStream = virtOpen.BaseStream;

        for (var iteration = 0; iteration < Iterations; iteration++)
            procIteration(iteration, realStream, virtStream);
    }

    private void procIteration(int iteration, Stream real, Stream virt)
    {
        var len = real.Length;

        var start = 0;
        var length = len;

        switch (iteration)
        {
            case 0:
                start = 0;
                length = len;
                break;
            case 1:
                start = 0;
                length = 16384;
                break;
            case 2:
                start = 2592768;
                length = 4096;
                break;
            case 3:
                start = 2596864;
                length = 4096;
                break;
            case 4:
                start = 2576384;
                length = 4096;
                break;
            case 5:
                start = 16384;
                length = 32768;
                break;

            default:
                start = rand.Next(0, (int) len);    // may or may not reverse on itself
                var end = rand.Next(start, (int) len);

                length = end - start;

                break;
        }

        if (iteration > 0)
        {
           
        }

        var realResult = checkRandom(real, start, length);
        var virtResult = checkRandom(virt, start, length);

        var match = (realResult == virtResult);

        Console.WriteLine($"{iteration}: Start={start} Length={length} Match={match} r={realResult} v={virtResult}");
    }

    /// <summary>
    /// Open the file and seek to start, read length bytes, and compute a sha1
    /// </summary>
    /// <param name="file"></param>
    /// <param name="start"></param>
    /// <param name="length"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private string checkRandom(Stream VolumeStream, int start, long length)
    {
     
        // Seek to start
        VolumeStream.Seek(start, SeekOrigin.Begin);

        string catchHash = null;
        using (var hasher = System.Security.Cryptography.SHA1.Create())
        {
            var buffer = new byte[4096];
            var r = buffer.Length;

            try
            {
                while (r > 0)
                {
                    var mx = (int) Math.Min(length, buffer.Length);
                    if (mx < 1) break;

                    r = VolumeStream.Read(buffer, 0, mx);
                    if (r > 0)
                        hasher.TransformBlock(buffer, 0, (int) r, buffer, 0);
                }

                hasher.TransformFinalBlock(buffer, 0, 0);

                catchHash = Convert.ToHexString(hasher.Hash).ToLowerInvariant();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Got error hashing: {ex.Message}");
                return "BAD-HASH";
            }
        }

        return catchHash;

    }
}