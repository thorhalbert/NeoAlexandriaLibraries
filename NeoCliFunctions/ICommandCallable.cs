using MongoDB.Driver;

namespace NeoCliFunctions
{
    public interface ICommandCallable
    {
        IMongoDatabase db { get; set; }
        void RunCommand();
    }
}