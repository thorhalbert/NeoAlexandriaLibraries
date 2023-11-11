//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using MongoDB.Driver;
//using Microsoft.AspNetCore.Components.Web.Virtualization;

//public class MongoItemsProvider<T> : ItemsProvider
//{
//    private readonly IMongoCollection<T> collection;

//    public MongoItemsProvider(IMongoCollection<T> collection)
//    {
//        this.collection = collection;
//    }

//    public async Task<IEnumerable<T>> ItemsProvider(int startIndex, int count)
//    {
//        var filter = Builders<T>.Filter.Empty;
//        var options = new FindOptions<T>
//        {
//            Skip = startIndex,
//            Limit = count,
//            Sort = Builders<T>.Sort.Ascending("_id")
//        };

//        var cursor = await collection.FindAsync(filter, options);

//        var items = new List<T>();

//        while (await cursor.MoveNextAsync())
//        {
//            foreach (var item in cursor.Current)
//            {
//                items.Add(item);
//            }
//        }

//        return items;
//    }
//}