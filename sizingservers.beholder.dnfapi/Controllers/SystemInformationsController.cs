using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace sizingservers.beholder.dnfapi.Controllers
{
    public class SystemInformationsController : ApiController
    {
        // GET: api/SystemInformations
        public IEnumerable<string> Get()
        {
            return new string[] { "value1", "value2" };
        }

        // GET: api/SystemInformations/5
        public string Get(int id)
        {
            return "value";
        }

        // POST: api/SystemInformations
        public void Post([FromBody]string value)
        {
        }

        // PUT: api/SystemInformations/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE: api/SystemInformations/5
        public void Delete(int id)
        {
        }
    }
}
