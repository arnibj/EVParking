using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.Caching;

namespace BackendData
{
    /// <summary>
    /// The Station class constructs objects that represent charging stations
    /// </summary>
    public class Station :DataBase
    {
        #region Enums and Sub-classes
        /// <summary>
        /// Plug type to indicate what type of charging is available
        /// </summary>
        public enum PlugType
        {
            None = 0,
            Type1 = 1, 
            Type2 = 2,
            BringYourOwnCable = 3,
            HighSpeeed = 4
        }

        /// <summary>
        /// Used to indicate the state a plug is in ie. Available, Charging etc.
        /// </summary>
        public enum State
        {
            Broken = 0,
            PluggedInButIdle  = 1,  //plugged in not charging
            Charging = 2,
            Available = 3
        }

        /// <summary>
        /// Used to specify on what side of a station a plug is located ie Left or Right
        /// </summary>
        public enum SidePosition
        {
            Undefined = 0,
            Right = 1,
            Left = 2
        }

        /// <summary>
        /// A sub-class of a station that describes a charging port
        /// </summary>
        public class Charger
        {
            public SidePosition Side { get; set; }
            public PlugType Plug { get; set; }
            public bool IsActive { get; set; }
            public State Status { get; set; }

        }
        #endregion

        private readonly IMongoCollection<Station> stationCollection;
        private readonly ObjectCache cache = MemoryCache.Default;

        [BsonId]
        [BsonElement("_id")]
        public Guid Id { get; set; }
        public string Name { get; set; }  
        public int Number { get; set; }  
        public List<Charger> Chargers { get; set; }
        public string? Details { get; set; }



        public Station()
        {
            stationCollection = db.GetCollection<Station>("Stations");
            Name = string.Empty;
            Chargers = new();
        }

        /// <summary>
        /// Retrieves list of stations
        /// </summary>
        /// <returns>List of all stations, from cache or datastore</returns>
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
                    await Get();
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

        /// <summary>
        /// Adds station to the underlying datastore
        /// </summary>
        /// <param name="station">Station object</param>
        /// <returns>True if stored successfully</returns>
        public async Task<bool> Add(Station station)
        {
            await stationCollection.InsertOneAsync(station);
            cache.Remove("Stations");
            return true;
        }

        /// <summary>
        /// Creates 20 dummy stations
        /// </summary>
        private async void CreateStations()
        {
            try
            {
                for(int i = 1; i <= 20; i++)
                {
                    Station s = new();
                    s.Number = i;
                    s.Name = $"Station {i}";

                    Charger left = new();
                    left.Plug = PlugType.Type2;
                    left.Status = State.Available;
                    left.Side = SidePosition.Left;
                    left.IsActive = true;
                    s.Chargers.Add(left);

                    Charger right = new();
                    right.Plug = PlugType.Type2;
                    right.Status = State.Available;
                    right.Side = SidePosition.Right;
                    right.IsActive = true;
                    s.Chargers.Add(right);

                    s.Id = Guid.NewGuid();
                    
                    await Add(s);
                }
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
        }
    }
}
