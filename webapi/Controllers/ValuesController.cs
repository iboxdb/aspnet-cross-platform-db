using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2", DateTime.Now.ToString() };
        }

        // GET api/values/5
        ///<summary>
        /// Action Result 88
        ///</summary>
        /// <remarks>Awesomeness 88!</remarks>
        [HttpGet("{id}")]
        public ActionResult<string> Get(int id)
        {
            return "value " + id;
        }

        // POST api/values
        [HttpPost]
        public string Post([FromBody] string value)
        {
            return "FROM BODY " + value;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
