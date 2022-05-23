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
            Available = 3,
            OverTimeLimit = 4
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
            public string DisplayClass { get; set; } = "alert-primary";

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
        public string? Details { get; set; } = string.Empty;


        /// <summary>
        /// Constructor
        /// </summary>
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

                foreach(Station s in list)
                {
                    foreach(Charger charger in s.Chargers)
                    {
                        if(charger.Status == State.Available)
                        {
                            charger.DisplayClass = "alert-success";
                        }
                        else if(charger.Status == State.OverTimeLimit)
                        {
                            charger.DisplayClass = "alert-danger";
                        }
                    }
                }

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
            List<Station> existingStations = (List<Station>)cache.Get("Stations");
            bool add = true;
            if(existingStations != null)
            {
                if (existingStations.Any(s => s.Number == station.Number))
                {
                    add = false;
                }
            }
            if (add)
            {
                await stationCollection.InsertOneAsync(station);
                cache.Remove("Stations");
                return true;
            }
            return false;
        }

        /// <summary>
        /// Locate station by Id
        /// </summary>
        /// <param name="id">Guid ID</param>
        /// <returns>Station object</returns>
        public async Task<Station?> GetStationByIdAsync(Guid id)
        {
            List<Station> items = await Get();
            return items.SingleOrDefault(l => l.Id == id);
        }

        /// <summary>
        /// Locate station by number
        /// </summary>
        /// <param name="number">Station number</param>
        /// <returns>Station object</returns>
        public async Task<Station?> GetStationByNumberAsync(int number)
        {
            List<Station> items = await Get();
            return items.SingleOrDefault(l => l.Number == number);
        }

        /// <summary>
        /// Station updater
        /// </summary>
        /// <param name="station">Station object</param>
        /// <returns>Station object after update</returns>
        public async Task<Station?> Update(Station station)
        {
            FilterDefinition<Station> filter = Builders<Station>.Filter.Eq(f => f.Id, station.Id);
            var updateDefinition = Builders<Station>.Update
                .Set(s => s.Name, station.Name)
                .Set(s => s.Details, station.Details)
                .Set(s => s.Number, station.Number)
                .Set(s => s.Chargers[0], station.Chargers[0])
                .Set(s => s.Chargers[1], station.Chargers[1]);

            await stationCollection.UpdateOneAsync(filter, updateDefinition);
            return await GetStationByIdAsync(station.Id);
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
                    s.Details = "Austurhraun 9";

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

                //test update
                Station? st1 = await GetStationByNumberAsync(1);
                if (st1 != null)
                {
                    st1.Chargers[0].Plug = PlugType.Type1;
                    st1.Chargers[1].Plug = PlugType.Type1;
                    st1.Chargers[1].Status = State.OverTimeLimit;
                    await Update(st1);
                }

                Station? st5 = await GetStationByNumberAsync(5);
                if (st5 != null)
                {
                    st5.Chargers[0].Plug = PlugType.BringYourOwnCable;
                    st5.Chargers[0].Status = State.OverTimeLimit;
                    st5.Chargers[1].Plug = PlugType.BringYourOwnCable;
                    st5.Chargers[1].Status = State.Charging;
                    await Update(st5);
                }

            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
        }
    }
}
