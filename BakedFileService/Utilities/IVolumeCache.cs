using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BakedFileService.Utilities
{
    interface IVolumeCache
    {
        T Get<T>(string key);
        void Add<T>(T o, string key);
        void Remove(string key);
    }
}
