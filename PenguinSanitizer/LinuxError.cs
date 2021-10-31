using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Tmds.Linux;

namespace PenguinSanitizer
{
    public class LinuxError : Exception
    {
        public static Dictionary<int, Errno> ErrorList { get; private set; }
        static LinuxError()
        {
            var errorList = new List<Errno>
            {
                new Errno(1, "EPERM", "Operation not permitted"),
                new Errno(2, "ENOENT", "No such file or directory"),
                new Errno(3, "ESRCH", "No such process"),
                new Errno(4, "EINTR", "Interrupted system call"),
                new Errno(5, "EIO", "I/O error"),
                new Errno(6, "ENXIO", "No such device or address"),
                new Errno(7, "E2BIG", "Argument list too long"),
                new Errno(8, "ENOEXEC", "Exec format error"),
                new Errno(9, "EBADF", "Bad file number"),
                new Errno(10, "ECHILD", "No child processes"),
                new Errno(11, "EAGAIN", "Try again"),
                new Errno(12, "ENOMEM", "Out of memory"),
                new Errno(13, "EACCES", "Permission denied"),
                new Errno(14, "EFAULT", "Bad address"),
                new Errno(15, "ENOTBLK", "Block device required"),
                new Errno(16, "EBUSY", "Device or resource busy"),
                new Errno(17, "EEXIST", "File exists"),
                new Errno(18, "EXDEV", "Cross-device link"),
                new Errno(19, "ENODEV", "No such device"),
                new Errno(20, "ENOTDIR", "Not a directory"),
                new Errno(21, "EISDIR", "Is a directory"),
                new Errno(22, "EINVAL", "Invalid argument"),
                new Errno(23, "ENFILE", "File table overflow"),
                new Errno(24, "EMFILE", "Too many open files"),
                new Errno(25, "ENOTTY", "Not a typewriter"),
                new Errno(26, "ETXTBSY", "Text file busy"),
                new Errno(27, "EFBIG", "File too large"),
                new Errno(28, "ENOSPC", "No space left on device"),
                new Errno(29, "ESPIPE", "Illegal seek"),
                new Errno(30, "EROFS", "Read-only file system"),
                new Errno(31, "EMLINK", "Too many links"),
                new Errno(32, "EPIPE", "Broken pipe"),
                new Errno(33, "EDOM", "Math argument out of domain of func"),
                new Errno(34, "ERANGE", "Math result not representable"),
                new Errno(35, "EDEADLK", "Resource deadlock would occur"),
                new Errno(36, "ENAMETOOLONG", "File name too long"),
                new Errno(37, "ENOLCK", "No record locks available"),
                new Errno(38, "ENOSYS", "Function not implemented"),
                new Errno(39, "ENOTEMPTY", "Directory not empty"),
                new Errno(40, "ELOOP", "Too many symbolic links encountered"),
                new Errno(42, "ENOMSG", "No message of desired type"),
                new Errno(43, "EIDRM", "Identifier removed"),
                new Errno(44, "ECHRNG", "Channel number out of range"),
                new Errno(45, "EL2NSYNC", "Level 2 not synchronized"),
                new Errno(46, "EL3HLT", "Level 3 halted"),
                new Errno(47, "EL3RST", "Level 3 reset"),
                new Errno(48, "ELNRNG", "Link number out of range"),
                new Errno(49, "EUNATCH", "Protocol driver not attached"),
                new Errno(50, "ENOCSI", "No CSI structure available"),
                new Errno(51, "EL2HLT", "Level 2 halted"),
                new Errno(52, "EBADE", "Invalid exchange"),
                new Errno(53, "EBADR", "Invalid request descriptor"),
                new Errno(54, "EXFULL", "Exchange full"),
                new Errno(55, "ENOANO", "No anode"),
                new Errno(56, "EBADRQC", "Invalid request code"),
                new Errno(57, "EBADSLT", "Invalid slot"),
                new Errno(59, "EBFONT", "Bad font file format"),
                new Errno(60, "ENOSTR", "Device not a stream"),
                new Errno(61, "ENODATA", "No data available"),
                new Errno(62, "ETIME", "Timer expired"),
                new Errno(63, "ENOSR", "Out of streams resources"),
                new Errno(64, "ENONET", "Machine is not on the network"),
                new Errno(65, "ENOPKG", "Package not installed"),
                new Errno(66, "EREMOTE", "Object is remote"),
                new Errno(67, "ENOLINK", "Link has been severed"),
                new Errno(68, "EADV", "Advertise error"),
                new Errno(69, "ESRMNT", "Srmount error"),
                new Errno(70, "ECOMM", "Communication error on send"),
                new Errno(71, "EPROTO", "Protocol error"),
                new Errno(72, "EMULTIHOP", "Multihop attempted"),
                new Errno(73, "EDOTDOT", "RFS specific error"),
                new Errno(74, "EBADMSG", "Not a data message"),
                new Errno(75, "EOVERFLOW", "Value too large for defined data type"),
                new Errno(76, "ENOTUNIQ", "Name not unique on network"),
                new Errno(77, "EBADFD", "File descriptor in bad state"),
                new Errno(78, "EREMCHG", "Remote address changed"),
                new Errno(79, "ELIBACC", "Can not access a needed shared library"),
                new Errno(80, "ELIBBAD", "Accessing a corrupted shared library"),
                new Errno(81, "ELIBSCN", ".lib section in a.out corrupted"),
                new Errno(82, "ELIBMAX", "Attempting to link in too many shared libraries"),
                new Errno(83, "ELIBEXEC", "Cannot exec a shared library directly"),
                new Errno(84, "EILSEQ", "Illegal byte sequence"),
                new Errno(85, "ERESTART", "Interrupted system call should be restarted"),
                new Errno(86, "ESTRPIPE", "Streams pipe error"),
                new Errno(87, "EUSERS", "Too many users"),
                new Errno(88, "ENOTSOCK", "Socket operation on non-socket"),
                new Errno(89, "EDESTADDRREQ", "Destination address required"),
                new Errno(90, "EMSGSIZE", "Message too long"),
                new Errno(91, "EPROTOTYPE", "Protocol wrong type for socket"),
                new Errno(92, "ENOPROTOOPT", "Protocol not available"),
                new Errno(93, "EPROTONOSUPPORT", "Protocol not supported"),
                new Errno(94, "ESOCKTNOSUPPORT", "Socket type not supported"),
                new Errno(95, "EOPNOTSUPP", "Operation not supported on transport endpoint"),
                new Errno(96, "EPFNOSUPPORT", "Protocol family not supported"),
                new Errno(97, "EAFNOSUPPORT", "Address family not supported by protocol"),
                new Errno(98, "EADDRINUSE", "Address already in use"),
                new Errno(99, "EADDRNOTAVAIL", "Cannot assign requested address"),
                new Errno(100, "ENETDOWN", "Network is down"),
                new Errno(101, "ENETUNREACH", "Network is unreachable"),
                new Errno(102, "ENETRESET", "Network dropped connection because of reset"),
                new Errno(103, "ECONNABORTED", "Software caused connection abort"),
                new Errno(104, "ECONNRESET", "Connection reset by peer"),
                new Errno(105, "ENOBUFS", "No buffer space available"),
                new Errno(106, "EISCONN", "Transport endpoint is already connected"),
                new Errno(107, "ENOTCONN", "Transport endpoint is not connected"),
                new Errno(108, "ESHUTDOWN", "Cannot send after transport endpoint shutdown"),
                new Errno(109, "ETOOMANYREFS", "Too many references: cannot splice"),
                new Errno(110, "ETIMEDOUT", "Connection timed out"),
                new Errno(111, "ECONNREFUSED", "Connection refused"),
                new Errno(112, "EHOSTDOWN", "Host is down"),
                new Errno(113, "EHOSTUNREACH", "No route to host"),
                new Errno(114, "EALREADY", "Operation already in progress"),
                new Errno(115, "EINPROGRESS", "Operation now in progress"),
                new Errno(116, "ESTALE", "Stale NFS file handle"),
                new Errno(117, "EUCLEAN", "Structure needs cleaning"),
                new Errno(118, "ENOTNAM", "Not a XENIX named type file"),
                new Errno(119, "ENAVAIL", "No XENIX semaphores available"),
                new Errno(120, "EISNAM", "Is a named type file"),
                new Errno(121, "EREMOTEIO", "Remote I/O error"),
                new Errno(122, "EDQUOT", "Quota exceeded"),
                new Errno(123, "ENOMEDIUM", "No medium found"),
                new Errno(124, "EMEDIUMTYPE", "Wrong medium type"),
                new Errno(125, "ECANCELED", "Operation Canceled"),
                new Errno(126, "ENOKEY", "Required key not available"),
                new Errno(127, "EKEYEXPIRED", "Key has expired"),
                new Errno(128, "EKEYREVOKED", "Key has been revoked"),
                new Errno(129, "EKEYREJECTED", "Key was rejected by service"),
                new Errno(130, "EOWNERDEAD", "Owner died"),
                new Errno(131, "ENOTRECOVERABLE", "State not recoverable")
            };

            ErrorList = new Dictionary<int, Errno>();
            foreach (var e in errorList)
                ErrorList.Add(e.Err, e);
        }

        public int Errno { get; private set; }
        public Errno? Error { get; private set; } = null;

        public LinuxError(int errno=-9999, Exception? inner=null) : base(setException(errno), innerException: inner)
        {
            if (errno == -9999)
                errno = LibC.errno;  // Just get it

            if (errno < 0) errno = -errno;
            Errno = errno;

            if (ErrorList.ContainsKey(errno))
            {
                Error = ErrorList[errno];
            }
        }

        private static string setException(int errno)
        {
            if (errno < 0) errno = -errno;
          
            if (ErrorList.ContainsKey(errno))
            {
               var e = ErrorList[errno];

                return $"LibC Errno: ({errno}) {e.Name} - {e.Desc}";
            }

            return $"LibC Errno: Unknown ({errno})";
        }
    }

    public struct Errno
    {   
        public int Err { get; private set; }
        public string Name { get; private set; }
        public string Desc { get; private set; }

        public Errno(int errno, string errName, string desc)
        {
            Err = errno;
            Name = errName;
            Desc = desc;
        }
    }
}
