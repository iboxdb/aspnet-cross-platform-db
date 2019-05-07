using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iBoxDB.LocalServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace webapi.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class MasterAController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2", DateTime.Now.ToString() };
        }

        [HttpPost("Insert")]
        public string Insert([FromBody] string value)
        {
            return CommitResult.OK.ToString();
        }


        // GET api/values/5
        ///<summary>
        /// Action Result 88
        ///</summary>
        /// <remarks>Awesomeness 88!</remarks>
        [HttpGet("Log/{id}")]
        public String Log(long id)
        {
            return "value " + id;
        }

        [HttpPost("Replicate")]
        public string Replicate([FromBody] string log)
        {
            return "FROM BODY " + log;
        }




    }
}
