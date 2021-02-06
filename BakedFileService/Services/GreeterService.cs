using Grpc.Core;
using Microsoft.Extensions.Logging;
using NeoAlexandriaService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BakedFileService
{
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly ILogger<GreeterService> _logger;
        public GreeterService(ILogger<GreeterService> logger)
        {
            _logger = logger;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Service = "BakedFileService",
                Uptime = (ulong)(DateTimeOffset.Now - Startup.StartupTime).Ticks,
                HostName = Environment.MachineName,
                BakedFileServiceMaxBuffer = BakedVolumeService.BUFSIZ
            });
        }
    }
}
