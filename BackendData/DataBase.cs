using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using System.Runtime.Caching;
using System.Security.Claims;

namespace BackendData
{
    public class DataBase
    {
        public readonly MongoClientSettings mongoConnection;
        public readonly MongoClient client;
        public readonly IMongoDatabase db;
        public readonly Utilities utility;
        public readonly CacheItemPolicy cacheItemPolicy;
        public DataBase()
        {
            #region Remove defaults registry
            ConventionRegistry.Remove("__defaults__");
            var pack = new ConventionPack();
            var defaultConventions = DefaultConventionPack.Instance.Conventions;
            pack.AddRange(defaultConventions.Except(
                defaultConventions.OfType<ImmutableTypeClassMapConvention>()
                ));
            ConventionRegistry.Register(
                "__defaults__",
                pack,
                t => true);
            #endregion

            cacheItemPolicy = new CacheItemPolicy
            {
                AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(60),
            };

            //Setup the db connection


            mongoConnection = MongoClientSettings.FromConnectionString("mongodb://localhost:27017");
            mongoConnection.ConnectTimeout = TimeSpan.FromSeconds(5);
            mongoConnection.ServerSelectionTimeout = TimeSpan.FromSeconds(5);
            mongoConnection.AllowInsecureTls = true;
            mongoConnection.ApplicationName = "EVIdentity";

            //Setup the client
            client = new MongoClient(mongoConnection);
            //Setup connection to database
            db = client.GetDatabase("EVIdentity");

            utility = new Utilities();
        }

        /// <summary>
        /// Add capped collection, that can only contain x number of documents
        /// </summary>
        /// <param name="collectionName">Name of new collection</param>
        /// <param name="maxDocs">Max number of documents</param>
        public async void AddCappedCollection(string collectionName, int maxDocs)
        {
            var collections = (await db.ListCollectionsAsync()).ToList();
            if (!collections.Any(c => c == collectionName))
            {
                await db.CreateCollectionAsync(collectionName,
                    new CreateCollectionOptions()
                    {
                        Capped = true,
                        MaxDocuments = maxDocs,
                        MaxSize = 10000
                    });
            }
        }

        /// <summary>
        /// Add collection (no cap)
        /// </summary>
        /// <param name="collectionName">Name of new collection</param>
        public async void AddCollection(string collectionName)
        {
            var collections = (await db.ListCollectionsAsync()).ToList();
            if (!collections.Any(c => c == collectionName))
            {
                await db.CreateCollectionAsync(collectionName);
            }
        }

        /// <summary>
        /// Remove collection
        /// </summary>
        /// <param name="collectionName">Name of collection to remove</param>
        public async void DropCollection(string collectionName)
        {
            await db.DropCollectionAsync(collectionName);
        }

        /// <summary>
        /// Helper function to acquire value from the user's claims property list
        /// </summary>
        /// <param name="identity">The users claims identity object</param>
        /// <param name="type">Name of property requested</param>
        /// <returns>Value of requested property if it exist, blank string otherwise</returns>
        public static string ReturnUserClaimTypeValue(ClaimsIdentity identity, string type)
        {
            try
            {
                if (identity != null && identity.Claims != null && !string.IsNullOrEmpty(type))
                {
                    var v = identity.Claims.Where(c => c.Type == type).SingleOrDefault();
                    if (v != null)
                        return v.Value;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            return String.Empty;
        }
    }
}
