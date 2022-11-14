using BackendData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using static BackendData.Station;
using static EVParking.Controllers.PushController;
using static System.Collections.Specialized.BitVector32;

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
            string user = PushClient.ReturnUserClaimTypeValue(identity: (ClaimsIdentity)User?.Identity, "preferred_username");
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
            Utilities utility = new();
            string user = PushClient.ReturnUserClaimTypeValue(identity: (ClaimsIdentity)User?.Identity, "preferred_username");
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
