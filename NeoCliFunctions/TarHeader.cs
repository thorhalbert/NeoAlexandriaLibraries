using PenguinSanitizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeoCliFunctions;

/// <summary>
/// Abstract a 512 byte tar header
/// Only has to handle simple tar files from the baked engine
/// Need to be able to detect if header is a header or not (for salvage)
/// </summary>
public class TarHeader
{
    public enum FileTypes : byte
    {
        REGTYPE = (byte) '0',
        AREGTYPE = 0,
        LNKTYPE = (byte) '1',
        SYMTYPE = (byte) '2',
        CHRTYPE = (byte) '3',
        BLKTYPE = (byte) '4',
        DIRTYPE = (byte) '5',
        FIFOTYPE = (byte) '6'
    }

    //        struct posix_header
    //        {                              /* byte offset */
    //            char name[100];               /*   0 */
    //            char mode[8];                 /* 100 */
    //            char uid[8];                  /* 108 */
    //            char gid[8];                  /* 116 */
    //            char size[12];                /* 124 */
    //            char mtime[12];               /* 136 */
    //            char chksum[8];               /* 148 */
    //            char typeflag;                /* 156 */
    //            char linkname[100];           /* 157 */
    //            char magic[6];                /* 257 */
    //            char version[2];              /* 263 */
    //            char uname[32];               /* 265 */
    //            char gname[32];               /* 297 */
    //            char devmajor[8];             /* 329 */
    //            char devminor[8];             /* 337 */
    //            char prefix[155];             /* 345 */
    //            /* 500 */
    //        };

    //#define TMAGIC   "ustar"        /* ustar and a null */
    //#define TMAGLEN  6
    //#define TVERSION "00"           /* 00 and no null */
    //#define TVERSLEN 2

    //        /* Values used in typeflag field.  */
    //#define REGTYPE  '0'            /* regular file */
    //#define AREGTYPE '\0'           /* regular file */
    //#define LNKTYPE  '1'            /* link */
    //#define SYMTYPE  '2'            /* reserved */
    //#define CHRTYPE  '3'            /* character special */
    //#define BLKTYPE  '4'            /* block special */
    //#define DIRTYPE  '5'            /* directory */
    //#define FIFOTYPE '6'            /* FIFO special */
    //#define CONTTYPE '7'            /* reserved */

    //#define XHDTYPE  'x'            /* Extended header referring to the
    //        next file in the archive */
    //#define XGLTYPE  'g'            /* Global extended header */

    //        /* Bits used in the mode field, values in octal.  */
    //#define TSUID    04000          /* set UID on execution */
    //#define TSGID    02000          /* set GID on execution */
    //#define TSVTX    01000          /* reserved */
    //        /* file permissions */
    //#define TUREAD   00400          /* read by owner */
    //#define TUWRITE  00200          /* write by owner */
    //#define TUEXEC   00100          /* execute/search by owner */
    //#define TGREAD   00040          /* read by group */
    //#define TGWRITE  00020          /* write by group */
    //#define TGEXEC   00010          /* execute/search by group */
    //#define TOREAD   00004          /* read by other */
    //#define TOWRITE  00002          /* write by other */
    //#define TOEXEC   00001          /* execute/search by other */


    public int File { get; private set; }
    public ulong FileBlock { get; private set; }
    public ulong RunBlock { get; private set; }

    // Never assume names and links can be converted to unicode strings without error --
    //   Filenames will cut you
    public byte[] Name { get; private set; }
    public ulong Mode { get; private set; }
    public ulong Uid { get; private set; }
    public ulong Gid { get; private set; }
    public ulong Size { get; private set; }
    public ulong MTime { get; private set; }
    public ulong ChkSum { get; private set; }
    public FileTypes Type { get; private set; }
    public byte[] LinkName { get; private set; }
    public string Magic { get; private set; }
    public ulong Version { get; private set; }
    public string UName { get; private set; }
    public string GName { get; private set; }
    public ulong DevMajor { get; private set; }
    public ulong DevMinor { get; private set; }
    public byte[] Prefix { get; private set; }

    public ulong ActualChecksum { get; private set; }




    public bool Valid { get; private set; }

    public TarHeader(byte[] buf, int file, ulong fileblock, ulong runblock)
    {
        if (buf == null)
            throw new ArgumentNullException(nameof(buf));

        var valid = true;

        // Current position

        File = file;    // File #
        FileBlock = fileblock;  // What block # (512-byte) is this record (in this file)
        RunBlock = runblock;   // Block from beginning (unless we're salvaging)

        ActualChecksum = 0;

        //            char name[100];               /*   0 */
        //            char mode[8];                 /* 100 */
        //            char uid[8];                  /* 108 */
        //            char gid[8];                  /* 116 */
        //            char size[12];                /* 124 */
        //            char mtime[12];               /* 136 */
        //            char chksum[8];               /* 148 */
        //            char typeflag;                /* 156 */
        //            char linkname[100];           /* 157 */
        //            char magic[6];                /* 257 */
        //            char version[2];              /* 263 */
        //            char uname[32];               /* 265 */
        //            char gname[32];               /* 297 */
        //            char devmajor[8];             /* 329 */
        //            char devminor[8];             /* 337 */
        //            char prefix[155];             /* 345 */

        Name = _nts(buf, 0, 100, ref valid);

        Mode = _nti(buf, 100, 8, ref valid);
        Uid = _nti(buf, 108, 8, ref valid);
        Gid = _nti(buf, 116, 8, ref valid);
        Size = _nti(buf, 124, 12, ref valid);
        MTime = _nti(buf, 136, 12, ref valid);

        ChkSum = _nti(buf, 148, 8, ref valid);

        var t = _ntb(buf, 156, 1);
        Type = (FileTypes) t[0];

        LinkName = _nts(buf, 157, 100, ref valid);
        Magic = _nta(buf, 257, 6, ref valid);
        Version = _nti(buf, 263, 2, ref valid);
        UName = _nta(buf, 265, 32, ref valid);
        GName = _nta(buf, 297, 32, ref valid);
        DevMajor = _nti(buf, 329, 8, ref valid);
        DevMinor = _nti(buf, 337, 8, ref valid);
        Prefix = _ntb(buf, 356, 155);

        Valid = valid;

        //Console.WriteLine($"Valid: {valid}");

        if (Valid)      // Don't spend time is already bad
        {
            // Calculate actual checksum
            for (var i = 0; i < 512; i++)
                ActualChecksum += buf[i];

            var orgC = ActualChecksum;  

            // Subtract out the checksum block (it's supposed to be zero)
            for (var i = 0; i < 8; i++)
            {
                //Console.WriteLine($"{148 + i}: {buf[148 + i]}");
                ActualChecksum -= buf[148 + i];
                ActualChecksum += 32;
            }

            if (ActualChecksum != ChkSum)
                Valid = false;

            //Console.WriteLine($"CheckSums:  Org={orgC} Calc={ActualChecksum} Req={ChkSum}");
        }
    }
    
    // Grab byte slice
    private byte[] _ntb(byte[] buf, int idx, int v)
    {
        var b = buf[idx..(idx + v)].ToList().ToArray();

        return b;
    }
    // Convert octal bytes to integer
    private ulong _nti(byte[] buf, int idx, int v, ref bool valid)
    {
        var rBuf = _nts(buf, idx, v, ref valid);

        ulong acc = 0;
        foreach (var b in rBuf)
        {
            if (b == 0) break;
            //if (b < (byte) '0' || b > (byte) '7')
            //{
            //    Console.WriteLine($"Buf: {Encoding.ASCII.GetString(rBuf)}");
            //    valid = false;
            //    return acc;
            //}

            acc <<= 3;   // *8
            var o = b & 7;
            acc += (ulong) o;
        }
        return acc;
    }

    // Convert to byte string
    private byte[] _ntss(byte[] buf, int idx, int v, ref bool valid)
    {
        var rBuf = _ntb(buf, idx, v);

        for (var i = 0; i < rBuf.Length; i++)
            if (rBuf[i] == 0) return rBuf[..i];

        return rBuf;   // For now they're already all null terminated
    }

    byte[] _nts(byte[] buf, int idx, int v, ref bool valid)
    {
        var ki = idx;
        var byt = _ntss(buf, idx, v, ref valid);

        //var str = Encoding.ASCII.GetString(byt);
        //Console.WriteLine($"{ki}: {str}\t\t({v})");

        return byt;
    }

    // Convert to a string (assuming it's ascii-ish)
    private string _nta(byte[] buf, int idx, int v, ref bool valid)
    {
        var rBuf = _nts(buf, idx, v, ref valid);
        try
        {
            return Encoding.ASCII.GetString(rBuf);
        }
        catch (Exception ex)
        {
            //Console.WriteLine($"{idx}/{v} - decode error - {ex.Message}");
            //valid = false;
            return String.Empty;
        }
    }

    public override string ToString()
    {
        NeoMode_T mode = (NeoMode_T) (int) Mode;

        var sb = new StringBuilder();

        sb.AppendLine($"File: {File}, Block: {FileBlock}, Run: {RunBlock} Valid: {Valid}");
        sb.AppendLine($"Name: {Encoding.UTF8.GetString(Name)}");
        sb.AppendLine($"Mode: {mode.ModMask()}/Type: {Type}");
        sb.AppendLine($"Uid: {Uid}/{UName}, Gid: {Gid}/{GName}");
        sb.AppendLine($"Size: {Size}, Mod: {DateTimeOffset.FromUnixTimeSeconds((long) MTime).ToString("R")}");
        //sb.AppendLine($"Size: {Size}, Mod: {MTime}");

        return sb.ToString();
    }

}

     
    
