using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using NeoAlexandriaManager.TechnicalClasses;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace NeoAlexandriaManager.Data
{
    public class DiskStatusBroadcasts : Hub
    {
        private readonly OplogTailer watcherTail;

        public DiskStatusBroadcasts()
        {
            watcherTail = new OplogTailer("NeoAlexandria.DiskWatcher");
            watcherTail.Start();
        }

        public ChannelReader<BsonDocument> GenerateWatchStream()
        {
            var channel = Channel.CreateUnbounded<BsonDocument>();

            _ = WriteItemsAsync(channel.Writer);

            return channel.Reader;
        }

        private async Task WriteItemsAsync(ChannelWriter<BsonDocument> writer)
        {
            await foreach (var d in watcherTail.ProcessEnumeration())
            {
                await writer.WriteAsync(d);
            }
        }

        //public async IAsyncEnumerable<BsonDocument> ReturnWatchRecord()
        //{
        //    await foreach (var d in watcherTail.ProcessEnumeration())
        //    {
        //        yield return d;
        //    }
        //}
    }
}
