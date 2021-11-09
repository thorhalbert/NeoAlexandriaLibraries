using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PenguinSanitizer
{
    [Flags] public enum NeoMode_T : uint
    {
        // No octal, but we still have binary

        // File type mask
        S_IFMT =    0b000_001_111_000_000_000_000,  // 00170000

        // File Types

        S_IFSOCK =  0b000_001_100_000_000_000_000,  // 0140000
        S_IFLNK =   0b000_001_010_000_000_000_000,  // 0120000
        S_IFREG =   0b000_001_000_000_000_000_000,  // 0100000
        S_IFBLK =   0b000_000_110_000_000_000_000,  // 0060000
        S_IFDIR =   0b000_000_100_000_000_000_000,  // 0040000
        S_IFCHR =   0b000_000_010_000_000_000_000,  // 0020000
        S_IFIFO =   0b000_000_001_000_000_000_000,  // 0010000

        // Full protection mode bits

        S_PRODMSK = 0b000_000_000_111_111_111_111,  // 0007777

        // Super user bits

        S_IFSUID =  0b000_000_000_100_000_000_000,  // 0004000
        S_IFSGID =  0b000_000_000_010_000_000_000,  // 0002000
        S_IFSVTX =  0b000_000_000_001_000_000_000,  // 0001000

        // Owner file mode bits

        S_IRWXU = 0b000_000_111_000_000, // 00700
        S_IRUSR = 0b000_000_100_000_000, // 00400
        S_IWUSR = 0b000_000_010_000_000, // 00200
        S_IXUSR = 0b000_000_001_000_000, // 00100

        // Group file mode bits

        S_IRWXG = 0b000_000_000_111_000, // 00070
        S_IRGRP = 0b000_000_000_100_000, // 00040
        S_IWGRP = 0b000_000_000_010_000, // 00020
        S_IXGRP = 0b000_000_000_001_000, // 00010

        // World file mode bits

        S_IRWXO = 0b000_000_000_000_111, // 00007
        S_IROTH = 0b000_000_000_000_100, // 00004
        S_IWOTH = 0b000_000_000_000_010, // 00002
        S_IXOTH = 0b000_000_000_000_001, // 00001
    }

    public enum NeoMode_T_FileTypes
    {
        SOCK = 0b000_001_100,  // 014
        LNK = 0b000_001_010,  // 012
        REG = 0b000_001_000,  // 010
        BLK = 0b000_000_110,  // 006
        DIR = 0b000_000_100,  // 004
        CHR = 0b000_000_010,  // 002
        FIFO = 0b000_000_001,  // 001
    }

    public static class NeoMode_Texts   // Some bit-whackin'
    {
        public static NeoMode_T_FileTypes FileType(this NeoMode_T mode)
        {
            var val = (uint) (mode & NeoMode_T.S_IFMT);
            val >>= 12;

            return (NeoMode_T_FileTypes) val;
        }

        public static NeoMode_T SetFileType(this NeoMode_T toSet, NeoMode_T_FileTypes inType)
        {
            toSet &= (~NeoMode_T.S_IFMT);   // Clear the bits

            var outV = (uint) toSet;

            outV |= ((uint) inType) << 12;
            return (NeoMode_T) outV;
        }
        public static NeoMode_T Set(this NeoMode_T toSet, NeoMode_T bits)
        {
            toSet |= bits;

            return (NeoMode_T) toSet;
        }

        public static NeoMode_T UnSet(this NeoMode_T toSet, NeoMode_T bits)
        {
            toSet &= ~bits;

            return (NeoMode_T) toSet;
        }

        public static NeoMode_T ChMod(this NeoMode_T toSet, uint bits)
        {
            toSet &= ~NeoMode_T.S_PRODMSK;
            toSet |= (NeoMode_T) bits & NeoMode_T.S_PRODMSK;  // Make sure bits is sane

            return (NeoMode_T) toSet;
        }

        [Flags] public enum ModBits : uint
        {
            // Settings for RWX bits
            NO =    0,
            X =     0b0001,
            W =     0b0010,
            WX =    0b0011,
            R =     0b0100,
            RX =    0b0101,
            RW =    0b0110,
            RWX =   0b0111,

            // Settings for high bit
            SUID = 0b0100,
            SGID = 0b0010,
            STICKY = 0b0001,
        }

        public static uint Chmod(uint high, uint own, uint grp, uint world)
        {
            return (uint) high << 9 | own << 6 | grp << 3 | world;
        }
        public static uint Chmod(ModBits high, ModBits own, ModBits grp, ModBits world)
        {
            return (uint) high << 9 | (uint) own << 6 | (uint) grp << 3 | (uint) world;
        }

        // Format up with 
        public static string ModMask(this NeoMode_T mode)
        {
            var bits = (uint) mode;

            var mods = new char[10];
            for (var i = 0; i < 10; i++)
                mods[i] = '-';

            // 0123456789
            // frwxrwxrwx

            void set(int bse, uint val)
            {
                val &= 7;  // Clear anything else

                if ((val & 4) != 0)
                    mods[bse + 0] = 'r';
                if ((val & 2) != 0)
                    mods[bse + 0] = 'w';
                if ((val & 1) != 0)
                    mods[bse + 0] = 'x';
            }
            void setHigh(NeoMode_T r, char c1, char c2, int p)
            {
                if (r == 0) return;

                if (mods[p] == 'x')
                    mods[p] = c1;
                else
                    mods[p] = c2;
            }

            set(6, bits);
            set(3, bits << 3);
            set(0, bits << 6);

            setHigh(mode & NeoMode_T.S_IFSUID, 's', 'S', 3);
            setHigh(mode & NeoMode_T.S_IFSGID, 'g', 'G', 6);
            setHigh(mode & NeoMode_T.S_IFSVTX, 't', 'T', 9);

            switch (mode.FileType())
            {
                case NeoMode_T_FileTypes.FIFO:
                    mods[0] = 'p'; break;
                case NeoMode_T_FileTypes.CHR:
                    mods[0] = 'c'; break;
                case NeoMode_T_FileTypes.DIR:
                    mods[0] = 'd'; break;
                case NeoMode_T_FileTypes.BLK:
                    mods[0] = 'b'; break;
                case NeoMode_T_FileTypes.LNK:
                    mods[0] = 'l'; break;
                case NeoMode_T_FileTypes.SOCK:
                    mods[0] = 's'; break;
            }

            return new string(mods);
        }
    }
}
