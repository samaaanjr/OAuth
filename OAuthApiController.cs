using Newtonsoft.Json.Linq;
using Sabio.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace Sabio.Web.Controllers.Api
{
    [RoutePrefix("api/oauth")]
    public class OAuthApiController : ApiController
    {
        readonly IOAuthService oAuthService;
        readonly IUserService userService;

        public OAuthApiController(IOAuthService oAuthService, IUserService userService)
        {
            this.oAuthService = oAuthService;
            this.userService = userService;
        }

        [AllowAnonymous]
        [Route("verifytheuser"), HttpGet]
        public HttpResponseMessage GetByEmailAndOAuthId(string userEmail = null, string id_token = null, string oAuthId = null, ProviderEnum? provider = null)
        {
            bool OAuthAvailability = false;

            if (provider == ProviderEnum.Google)
            {
                OAuthAvailability = oAuthService.CheckGoogleTokenId(id_token, oAuthId);
            }
            else if (provider == ProviderEnum.Facebook)
            {
                OAuthAvailability = oAuthService.CheckFacebookTokenId(id_token, oAuthId);
            }

            if (OAuthAvailability)
            {
                bool isRegistered = userService.GetByEmailAndOAuhtId(oAuthId);

                return Request.CreateResponse(HttpStatusCode.OK, isRegistered ? "REGISTERED" : "NOT_REGISTERED");
            }
            else
            {
                return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "BAD_REQUEST");
            }
        }
    }
}