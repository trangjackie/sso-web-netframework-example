using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace WebApplication4.Controllers
{
    public class ValuesController : ApiController
    {
        // GET api/values
        public IEnumerable<string> Get()
        {
            var x1 = HeaderNames.Authorization;
            IEnumerable<String> accessTokenHeader;
            Request.Headers.TryGetValues(x1, out accessTokenHeader);
            if(accessTokenHeader== null)
            {
                return new string[] { "no permit" };
            }
            string accessToken = accessTokenHeader.First().Replace("Bearer ", "");
            if (ssoHelper.validateToken(accessToken))
                return new string[] { "value1", "value2" };
            else return new string[] { "error" };
        }

        // GET api/values/5
        public string Get(int id)
        {
            return "value";
        }

        public class Code
        {
            public string code { get; set; }
        }

        //
        // 
        static SsoVinorsoft ssoHelper = new SsoVinorsoft(
            "http://117.4.247.68:10825/realms/demo/",
            "testWAppNet",
            "LbWo3MiAzoa2yX767mtEuVdyH4FVfGmK",
            "http://localhost:52716/Content/sso.html",
            "MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAw3aw/gJ4yxdqRAJfreE4+BsVZwcBQI72bUJTGz76edJHPyT48XK6B6nnsNLfcstum9uo7eDetPx/n72RJ4cWnnUGQUh1i9vCk2B0/PZGmdgCdbHS0yViI0WUIrynLgcTL8teeH7StV39gFm+y+iSpXXvLAkuKP9gOayqR7VmZWuZKx9PI2LcVwDdKtZz6lzBg/zVJc0QnC/eeHLD1NDQnWjwAS8h+v+m4vubvUrw4th7a6/1GhGCgt/lZAxdl8WO+jcHbsIXlS8SAZYHNzHefq4qBTWRxYCyeA0SlUknYBPeOaUw+//C+CvuDJ95VDy4Y4DtJq72G3tL7LtpD8SQjwIDAQAB"
        );

        // POST api/values
        public Object Post([FromBody] Code code)
        {
            return Ok(ssoHelper.GetCode(code.code));
        }

        // PUT api/values/5
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
