using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using iBoxDB.LocalServer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http.Features;
using WebApi.Models;
using WebApi;

namespace webapi.Controllers
{
    [Route("/[controller]")]
    [ApiController]
    public class ArticleController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public ActionResult<List<Article>> Get()
        {
            using (var box = App.Cube())
            {
                return box.Select<Article>("from Article limit 0 , 10").ToList();
            }
        }

        [HttpPost("Insert")]
        public string Insert([FromBody] string value)
        {
            var ip =
               HttpContext.Features.Get<Microsoft.AspNetCore.Http.Features.IHttpConnectionFeature>()?.RemoteIpAddress?.ToString();
            using (var box = App.Cube())
            {
                var ar = new Article
                {
                    Id = Article.NextTimeId(),
                    Text = value,
                    Ip = ip
                };
                box["Article"].Insert(ar);

                return box.Commit().ToString();
            }
        }

    }
}
