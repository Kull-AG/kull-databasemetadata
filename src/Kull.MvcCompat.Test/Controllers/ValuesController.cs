using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace Kull.MvcCompat.Test.Controllers
{
    public class ValuesController : ApiController
    {
        private readonly DisposeTracker disposeTracker;
        private readonly DisposeTracker2 disposeTracker2;

        public ValuesController(DisposeTracker disposeTracker, DisposeTracker2 disposeTracker2)
        {
            this.disposeTracker = disposeTracker;
            this.disposeTracker2 = disposeTracker2;
        }

        // GET api/<controller>
        public IEnumerable<string> Get()
        {
            return new string[] { disposeTracker.GetGuidString(), disposeTracker2.GetGuidString() };
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<controller>/5
        public void Delete(int id)
        {
        }
    }
}