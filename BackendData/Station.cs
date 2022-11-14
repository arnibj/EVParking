using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System.Runtime.Caching;

namespace BackendData
{
    /// <summary>
    /// The Station class constructs objects that represent charging stations
    /// </summary>
    public class Station : DataBase
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
        /// Used to indicate the state a plug is in i.e. Available, Charging etc.
        /// </summary>
        public enum State
        {
            Broken = 0,
            PluggedInButIdle = 1,  //plugged in not charging
            Charging = 2,
            Available = 3,
            OverTimeLimit = 4
        }

        /// <summary>
        /// Used to specify on what side of a station a plug is located i.e. Left or Right
        /// </summary>
        public enum SidePosition
        {
            Undefined = 0,
            Right = 1,
            Left = 2
        }

        public enum Section
        {
            Undefined = 0,
            Austurhraun9N = 1,
            Austurhraun9S = 2,
            Austurhraun7 = 3,
        }

        /// <summary>
        /// A sub-class of a station that describes a charging port
        /// </summary>
        public class Charger
        {
            public Guid ChargerId { get; set; }
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
        public Section ParkingSection { get; set; } = Section.Undefined;


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
                    .OrderBy(o => o.ParkingSection)
                    .ToList();

                foreach (Station s in list)
                {
                    foreach (Charger charger in s.Chargers)
                    {
                        if (charger.Status == State.Available)
                        {
                            charger.DisplayClass = "alert-success";
                        }
                        else if (charger.Status == State.OverTimeLimit)
                        {
                            charger.DisplayClass = "alert-warning";
                        }
                        else if (charger.Status == State.Broken)
                        {
                            charger.DisplayClass = "alert-danger";
                        }
                        else
                        {
                            charger.DisplayClass = "alert-info";
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
            if (existingStations != null)
            {
                if (existingStations.Any(s => s.Number == station.Number && s.ParkingSection == station.ParkingSection))
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
        public async Task<Station?> GetStationByNumberAndSectionAsync(int number, Section section)
        {
            List<Station> items = await Get();
            return items.SingleOrDefault(l => l.Number == number && l.ParkingSection == section);
        }
        
        /// <summary>
        /// Gets charger by its Id
        /// </summary>
        /// <param name="chargerid">ChargerId guid</param>
        /// <returns>Charger object</returns>
        public async Task<Charger> GetChargerByIdAsync(Guid chargerid)
        {
            Charger charger = new();
            List<Station> items = await Get();
            Station? station = items.SingleOrDefault(l => l.Chargers[0].ChargerId == chargerid || l.Chargers[1].ChargerId == chargerid);
            if (station != null)
            {
                if (station.Chargers[0].ChargerId == chargerid)
                    charger = station.Chargers[0];
                else if (station.Chargers[1].ChargerId == chargerid)
                    charger = station.Chargers[1];
            }
            return charger;
        }

        /// <summary>
        /// Get station by chargerId
        /// </summary>
        /// <param name="chargerid"></param>
        /// <returns></returns>
        public async Task<Station?> GetStationByChargerIdAsync(Guid chargerid)
        {
            Charger charger = new();
            List<Station> items = await Get();
            Station? station = items.SingleOrDefault(l => l.Chargers[0].ChargerId == chargerid || l.Chargers[1].ChargerId == chargerid);
            return station;
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
            cache.Remove("Stations");
            return await GetStationByIdAsync(station.Id);
        }

        /// <summary>
        /// Creates 20 dummy stations
        /// </summary>
        private async void CreateStations()
        {
            try
            {
                for (int i = 1; i <= 10; i++)
                {
                    if (i != 8 || i != 9)
                    {
                        Station s = new();
                        s.Number = i;
                        s.Name = $"Station {i}";
                        s.Details = "Austurhraun 9";
                        s.ParkingSection = Section.Austurhraun9N;
                        s.Id = Guid.NewGuid();

                        Charger left = new()
                        {
                            ChargerId = Guid.NewGuid(),
                            Plug = PlugType.Type2,
                            Status = State.Available,
                            Side = SidePosition.Left,
                            IsActive = true
                        };
                        s.Chargers.Add(left);

                        Charger right = new()
                        {
                            ChargerId = Guid.NewGuid(),
                            Plug = PlugType.Type2,
                            Status = State.Available,
                            Side = SidePosition.Right,
                            IsActive = true
                        };
                        s.Chargers.Add(right);

                        await Add(s);
                    }
                }

                //Updates
                Station? st1 = await GetStationByNumberAndSectionAsync(1, Section.Austurhraun9N);
                if (st1 != null)
                {
                    st1.Chargers[0].Plug = PlugType.BringYourOwnCable;
                    st1.Chargers[1].Plug = PlugType.BringYourOwnCable;
                    st1.Chargers[1].Status = State.OverTimeLimit;
                    await Update(st1);
                }

                Station? st2 = await GetStationByNumberAndSectionAsync(2, Section.Austurhraun9N);
                if (st2 != null)
                {
                    st2.Chargers[0].Plug = PlugType.BringYourOwnCable;
                    st2.Chargers[1].Plug = PlugType.BringYourOwnCable;
                    await Update(st2);
                }

                Station? st5 = await GetStationByNumberAndSectionAsync(5, Section.Austurhraun9N);
                if (st5 != null)
                {
                    st5.Chargers[0].Plug = PlugType.Type1;
                    st5.Chargers[0].Status = State.OverTimeLimit;
                    st5.Chargers[1].Plug = PlugType.Type1;
                    st5.Chargers[1].Status = State.Charging;
                    await Update(st5);
                }

                Station? st7 = await GetStationByNumberAndSectionAsync(7, Section.Austurhraun9N);
                if (st7 != null)
                {
                    st7.Chargers[0].Plug = PlugType.Type1;
                    st7.Chargers[1].Plug = PlugType.Type1;
                    await Update(st7);
                }

                for (int i = 5; i <= 8; i++)
                {
                    Station s = new();
                    s.Number = i;
                    s.Name = $"Station {i}";
                    s.Details = "Austurhraun 9";
                    s.ParkingSection = Section.Austurhraun9S;

                    Charger left = new()
                    {
                        ChargerId = Guid.NewGuid(),
                        Plug = PlugType.Type2,
                        Status = State.Available,
                        Side = SidePosition.Left,
                        IsActive = true
                    };
                    s.Chargers.Add(left);

                    Charger right = new()
                    {
                        ChargerId = Guid.NewGuid(),
                        Plug = PlugType.Type2,
                        Status = State.Available,
                        Side = SidePosition.Right,
                        IsActive = true
                    };
                    s.Chargers.Add(right);

                    s.Id = Guid.NewGuid();

                    await Add(s);
                }

                Station? st6 = await GetStationByNumberAndSectionAsync(6, Section.Austurhraun9S);
                if (st6 != null)
                {
                    st6.Chargers[0].Plug = PlugType.Type1;
                    st6.Chargers[1].Plug = PlugType.Type1;
                    await Update(st6);
                }

                for (int i = 1; i <= 4; i++)
                {
                    Station s = new();
                    s.Number = i;
                    s.Name = $"Station {i}";
                    s.Details = "Austurhraun 7";
                    s.ParkingSection = Section.Austurhraun7;

                    Charger left = new()
                    {
                        ChargerId = Guid.NewGuid(),
                        Plug = PlugType.Type2,
                        Status = State.Available,
                        Side = SidePosition.Left,
                        IsActive = true
                    };
                    s.Chargers.Add(left);

                    Charger right = new()
                    {
                        ChargerId = Guid.NewGuid(),
                        Plug = PlugType.Type2,
                        Status = State.Available,
                        Side = SidePosition.Right,
                        IsActive = true
                    };
                    s.Chargers.Add(right);

                    s.Id = Guid.NewGuid();

                    await Add(s);
                }
                Station? st3 = await GetStationByNumberAndSectionAsync(3, Section.Austurhraun7);
                if (st3 != null)
                {
                    st3.Chargers[0].Plug = PlugType.Type1;
                    st3.Chargers[1].Plug = PlugType.Type1;
                    await Update(st3);
                }
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
        }


        /// <summary>
        /// Update charger status
        /// </summary>
        /// <param name="chargerId"></param>
        /// <param name="newState"></param>
        /// <returns></returns>
        public async Task<bool> UpdateChargerStatus(Guid chargerId, State newState)
        {
            Station? station = new();
            station = await station.GetStationByChargerIdAsync(chargerId);
            if (station != null)
            {
                Charger? charger = station.Chargers.SingleOrDefault(c => c.ChargerId == chargerId);
                if (charger != null)
                {
                    charger.Status = newState;
                }
                await station.Update(station);
            }
            return true;
        }
    }
}
