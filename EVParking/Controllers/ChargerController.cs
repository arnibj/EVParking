using BackendData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static BackendData.Station;

namespace EVParkingWeb.Controllers
{
    public class ChargerController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        public ActionResult Details(string id)
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        [Route("api/charge/start")]
        public async Task<string> Start(Guid chargerId)
        {
            if (User == null)
            {
                return string.Empty;
            }

            if (User.Identity is not ClaimsIdentity identity)
            {
                return string.Empty;
            }

            string user = DataBase.ReturnUserClaimTypeValue(identity: identity, "preferred_username");
            User u = new();
            var dbUser = await u.GetUserByEmailAsync(user);
            if (dbUser == null)
            {
                return "Error locating user";
            }
            Station s = new();
            Charger charger = await s.GetChargerByIdAsync(chargerId);
            ChargeLog chargeLog = new();
            await chargeLog.AddLog(dbUser.Id, charger);

            return string.Empty;
        }

        [Authorize]
        [HttpPost]
        [Route("api/charge/stop")]
        public async Task<string> Stop(Guid chargerId)
        {
            if (User == null)
            {
                return string.Empty;
            }

            if (User.Identity is not ClaimsIdentity identity)
            {
                return string.Empty;
            }

            Utilities utility = new();
            string user = DataBase.ReturnUserClaimTypeValue(identity: identity, "preferred_username");
            User u = new();
            var dbUser = await u.GetUserByEmailAsync(user);
            if (dbUser == null)
            {
                utility.LogTrace($"Error locating user {u} for charger {chargerId}");
                return $"Error locating user {u} for charger {chargerId}";
            }
            ChargeLog cl = new();
            List<ChargeLog> charges = await cl.GetCharges(dbUser.Id, chargerId);
            ChargeLog? activeCharge = charges.FirstOrDefault(d => d.ChargeEndDateTime == null);
            if (activeCharge != null)
            {
                activeCharge.ChargeEndDateTime = DateTime.Now;
                await cl.UpdateLog(activeCharge);
            }
            else
            {
                Station station = new();
                await station.UpdateChargerStatus(chargerId, State.Available);
                utility.LogTrace($"Error locating active charge for charger {chargerId}");
                //return $"Error locating active charge for charger {chargerId}";
            }

            return string.Empty;
        }
    }
}
