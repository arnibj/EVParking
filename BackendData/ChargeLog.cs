﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Caching;
using System.Text;
using System.Threading.Tasks;
using static BackendData.Station;
using static System.Collections.Specialized.BitVector32;

namespace BackendData
{
    public class ChargeLog : DataBase
    {
        /// <summary>
        /// Class used to store and display history of charges
        /// </summary>
        private readonly IMongoCollection<ChargeLog> _collection;
        private ObjectCache cache = MemoryCache.Default;
        private readonly Station station;

        [BsonId]
        [BsonElement("_id")]
        public Guid Id { get; set; }
        public DateTime ChargeStartDateTime { get; set; } = DateTime.Now;
        public DateTime? ChargeEndDateTime { get; set; }
        public Guid UserId { get; set; }
        public Guid ChargerId { get; set; }

        public ChargeLog()
        {
            _collection = db.GetCollection<ChargeLog>("ChargeLog");
            station = new();
        }

        /// <summary>
        /// Get records from collection or cache
        /// </summary>
        /// <returns></returns>
        public async Task<List<ChargeLog>> GetCharges(Guid? userId, Guid? chargerId)
        {
            try
            {
                List<ChargeLog> list = new();
                if (cache.Get("ChargeLogs") != null)
                {
                    list = (List<ChargeLog>)cache.Get("ChargeLogs");

                    //filter the list
                    if (userId != null && userId != new Guid())
                    {
                        list = list.Where(v => v.UserId == userId).ToList();
                    }
                    if (chargerId != null && chargerId != new Guid())
                    {
                        list = list.Where(c => c.ChargerId == chargerId).ToList();
                    }

                    return list;
                }
                ChargeLog c = new();
                var result = await c._collection.FindAsync(FilterDefinition<ChargeLog>.Empty);
                list = result.ToList();

                //add all items to cache
                cache.Add("ChangeLogs", list, cacheItemPolicy);

                //filter the list
                if (userId != null && userId != new Guid())
                {
                    list = list.Where(v => v.UserId == userId).ToList();
                }
                if (chargerId != null && chargerId != new Guid())
                {
                    list = list.Where(c => c.ChargerId == chargerId).ToList();
                }

                return list;
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
            }
            return new List<ChargeLog>();
        }

        /// <summary>
        /// Add charger to collection
        /// </summary>
        /// <param name="vehicle">Vehicle object</param>
        /// <param name="charger">Charger object</param>
        /// <returns>true if successful</returns>
        public async Task<bool> AddLog(Guid userId, Charger charger)
        {
            if (charger == null)
            {
                return false;
            }

            ChargeLog chargeLog = new()
            {
                UserId = userId,
                ChargeStartDateTime = DateTime.Now,
                ChargerId = charger.ChargerId
            };
            await _collection.InsertOneAsync(chargeLog);
            cache.Remove("ChargeLogs");

            //update status of charger as busy
            await station.UpdateChargerStatus(charger.ChargerId, State.Charging);

            return true;
        }

        /// <summary>
        /// Updates log based on Id
        /// </summary>
        /// <param name="chargeLog">ChargeLog object</param>
        /// <returns>true if successful</returns>
        public async Task<bool> UpdateLog(ChargeLog chargeLog)
        {
            try
            {
                FilterDefinition<ChargeLog> filter = Builders<ChargeLog>.Filter.Eq(item => item.Id, chargeLog.Id);
                var updateDefinition = Builders<ChargeLog>.Update
                    .Set(s => s.UserId, chargeLog.UserId)
                    .Set(s => s.ChargeStartDateTime, chargeLog.ChargeStartDateTime)
                    .Set(s => s.ChargeEndDateTime, chargeLog.ChargeEndDateTime)
                    .Set(s => s.ChargerId, chargeLog.ChargerId);
                await _collection.UpdateOneAsync(filter, updateDefinition);

                //update charger status, charge is being ended
                if (chargeLog.ChargeEndDateTime != null)
                {
                    await station.UpdateChargerStatus(chargeLog.ChargerId, State.Available);
                }
            }
            catch (Exception ex)
            {
                utility.LogException(ex);
                return false;
            }

            return true;
        }



    }
}
