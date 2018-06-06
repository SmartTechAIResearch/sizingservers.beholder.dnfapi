/*
 * 2018 Sizing Servers Lab
 * University College of West-Flanders, Department GKG
 * 
 */

using System;
using System.Linq;
using System.Web.Http;

namespace sizingservers.beholder.dnfapi.Controllers {
    public class SystemInformationsController : ApiController {
        private static DateTime _epochUtc = new DateTime(1970, 1, 1, 1, 1, 1, 1, DateTimeKind.Utc);

        /// <summary>
        /// Some sort of hack to get if authorization should be enabled or not (appsettings.json).
        /// </summary>
        public static bool Authorization { get; set; }

        /// <summary>
        /// GET "pong" if the api is reachable.
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public string Ping() {
            return "pong";
        }
        /// <summary>
        /// GET all stored system informations.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        [HttpGet]
        public Models.SystemInformation[] List(string apiKey = null) {
            if (!Authorize(apiKey))
                return null;

            return DA.SystemInformationDA.GetAll().ToArray();
        }
        /// <summary>
        /// To POST / store a new system information in the database or replace an existing one using the hostname.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="systemInformation"></param>
        /// <returns></returns>
        [HttpPost]
        public IHttpActionResult Report([FromBody]Models.SystemInformation systemInformation, string apiKey = null) {
            if (!Authorize(apiKey))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            systemInformation.timeStampInSecondsSinceEpochUtc = (long)(DateTime.UtcNow - _epochUtc).TotalSeconds;
            DA.SystemInformationDA.AddOrUpdate(systemInformation);

            return Created("list", typeof(string)); //Return a 201. Tell the client that the post did happen and were it can be requested.
        }
        /// <summary>
        /// Removes the system information having the given hostname, if any.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="hostname"></param>
        /// <returns></returns>
        [HttpDelete]
        public IHttpActionResult Remove(string hostname, string apiKey = null) {
            if (!Authorize(apiKey))
                return Unauthorized();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            DA.SystemInformationDA.Remove(hostname);

            return StatusCode(System.Net.HttpStatusCode.NoContent); //Http PUT response --> 200 OK or 204 NoContent. Latter equals done.
        }
        /// <summary>
        /// <para>Cleans up old system informations so the database represents reality.</para>
        /// <para>PUT url/api/cleanolderthan?days=#</para>
        /// </summary>
        /// <param name="days"></param>   
        /// <param name="apiKey"></param>     
        /// <returns></returns>
        [HttpPut]
        public IHttpActionResult CleanOlderThan(int days, string apiKey = null) {
            if (!Authorize(apiKey))
                return Unauthorized();

            if (!ModelState.IsValid && days < 1)
                return BadRequest("Given days should be an integer greater than 0.");

            long timeStampPastInSecondsSinceEpochUtc = (long)(DateTime.UtcNow.AddDays(days * -1) - _epochUtc).TotalSeconds;
            var toRemove = DA.SystemInformationDA.GetAll().Where(x => x.timeStampInSecondsSinceEpochUtc <= timeStampPastInSecondsSinceEpochUtc);
            DA.SystemInformationDA.Remove(toRemove.ToArray());

            return StatusCode(System.Net.HttpStatusCode.NoContent); //Http PUT response --> 200 OK or 204 NoContent. Latter equals done.
        }
        /// <summary>
        /// Clear (PUT) all system informations in the database.
        /// <param name="apiKey"></param>
        /// </summary>
        /// <returns></returns>
        [HttpPut]
        public IHttpActionResult Clear(string apiKey = null) {
            if (!Authorize(apiKey))
                return Unauthorized();

            DA.SystemInformationDA.Clear();

            return StatusCode(System.Net.HttpStatusCode.NoContent); //Http PUT response --> 200 OK or 204 NoContent. Latter equals done.
        }

        private bool Authorize(string apiKey) {
            if (!Authorization) return true;                        

            return DA.APIKeyDA.HasKey(apiKey);
        }
    }
}