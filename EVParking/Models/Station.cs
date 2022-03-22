using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.Caching;

namespace EVParking
{
    public class Station :DataBase
    {
        public enum PlugType
        {
            None = 0,
            Type1 = 1, 
            Type2 = 2,
            BringYourOwnCable = 3,
        }

        public enum State
        {
            Broken = 0,
            PluggedInButIdle  = 1,  //plugged in not charging
            Charging = 2,
            Available = 3
        }
        private readonly IMongoCollection<Station> stationCollection;
        private ObjectCache cache = MemoryCache.Default;

        [BsonId]
        [BsonElement("_id")]
        public Guid Id { get; set; }
        public string Name { get; set; }  
        public int Number { get; set; }  
        public PlugType Plug { get; set; }
        public bool IsActive { get; set; }
        public State Status { get; set; }

        public Station()
        {
            stationCollection = db.GetCollection<Station>("Stations");
            Name = string.Empty;

        }

        public async Task<List<Station>> Get()
        {
            try
            {
                List<Station> list = new();
                if (cache.Get("Stations") != null)
                {
                    list = (List<Station>)cache.Get("Stations");
                    return list;
                }
                Station i = new();
                var result = await i.stationCollection.FindAsync(FilterDefinition<Station>.Empty);
                list = result.ToList()
                    .OrderBy(o => o.Number)
                    .ToList();

                if (list.Count == 0)
                {
                    CreateStations();
                    Get();
                }
                
                cache.Add("Stations", list, cacheItemPolicy);
                return list;
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
            return new List<Station>();
        }
        public async Task<bool> Add(Station station)
        {
            await stationCollection.InsertOneAsync(station);
            cache.Remove("Stations");
            return true;
        }

        private void CreateStations()
        {
            try
            {
                for(int i = 1; i <= 20; i++)
                {
                    Station s = new();
                    s.Number = i;
                    s.Name = $"Station {i}";
                    s.Plug = PlugType.Type2;
                    s.Id = Guid.NewGuid();
                    s.Status = State.Available;
                    Add(s);
                }
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
        }
    }
}
