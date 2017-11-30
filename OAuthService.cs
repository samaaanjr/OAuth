using Newtonsoft.Json.Linq;
using Sabio.Data;
using Sabio.Data.Providers;
using Sabio.Models.Responses;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Facebook;
using System.Configuration;
using System.Web;

namespace Sabio.Services
{
    public class OAuthService : IOAuthService
    {
        readonly IDataProvider dataProvider;
        readonly OAuthKeys oAuthKeys;

        public OAuthService(IDataProvider dataProvider, OAuthKeys oAuthKeys)
        {
            this.dataProvider = dataProvider;
            this.oAuthKeys = oAuthKeys;
        }

        public bool CheckGoogleTokenId(string id_token, string oAuthId)
        {
            string endPoint = "https://www.googleapis.com/oauth2/v3/tokeninfo?id_token=" + HttpUtility.UrlEncode(id_token);
            string googleContentId = oAuthKeys.GoogleContentId;

            try
            {
                WebClient client = new WebClient();
                string reply = client.DownloadString(endPoint);
                var data = JObject.Parse(reply);
                string userId = data["sub"].ToString();
                string userEmail = data["email"].ToString();
                string gContentId = data["azp"].ToString();

                if (userId == oAuthId && gContentId == googleContentId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public bool CheckFacebookTokenId(string tokenToInspect, string oAuthId)
        {
            string app_Id = oAuthKeys.FacebookAppId;
            string app_secret = oAuthKeys.FacebookAppSecret;

            var fb = new FacebookClient();
            dynamic result = fb.Get("oauth/access_token", new
            {
                client_id = app_Id,
                client_secret = app_secret,
                grant_type = "client_credentials"
            });

            fb.AccessToken = result.access_token;

            string endPoint = string.Format("https://graph.facebook.com/debug_token?input_token={0}&access_token={1}", HttpUtility.UrlEncode(tokenToInspect), HttpUtility.UrlEncode(fb.AccessToken));

            try
            {
                WebClient client = new WebClient();
                var reply = client.DownloadString(endPoint);
                var data = JObject.Parse(reply);
                string appId = data["data"]["app_id"].ToString();
                string userId = data["data"]["user_id"].ToString();

                if (appId == app_Id && userId == oAuthId)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception)
            {
                return false;
                throw;
            }
        }

        public bool GetByEmailAndOAuhtId(string userEmail, string OAuthId)
        {
            OAuth_VerifyAvailabilityResponse response = null;
            dataProvider.ExecuteCmd("dbo.Users_OAuthValidation"
                , inputParamMapper: delegate (SqlParameterCollection paramCollection)
                {
                    paramCollection.AddWithValue("@userEmail", userEmail);
                    paramCollection.AddWithValue("@oAuthId", OAuthId);
                }
                , singleRecordMapper: delegate (IDataReader reader, short set)
                {
                    response = new OAuth_VerifyAvailabilityResponse();

                    response.IsRegistered = reader.GetSafeInt32(0) == 1 ? true : false;
                });

            return response.IsRegistered;
        }
    }
}
