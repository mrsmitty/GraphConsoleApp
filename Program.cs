using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json.Linq;

namespace aad
{
    class Program
    {
        public const string ResourceUrl = "https://graph.windows.net";
        private static string ClientId = "<provide>";
        private static string ClientSecret = "<provide>";

        public const string AuthResource = "https://login.microsoftonline.com/";
        public const string TenantId = "<provide>";
        public const string AuthString = AuthResource + TenantId;
        public static string GraphResource = "https://graph.windows.net/" + TenantId;
        //Get-AzureRmADApplication -DisplayNameStartWith Tenant | Select-Object -Property ApplicationId
        //Tenant Schema Extension App
        //remove "-" from GUID
        public static string TenantAppId = "";
        public static HttpClient _client;

        static void Main(string[] args)
        {
            Authenticate();
            GetRoleAssignments("me@client.com.au");
            GetRoleAssignments("troy.sutherland@royhill.com.au");
        }

        private static void Authenticate()
        {
            var token = GetToken().Result;
            _client = new HttpClient();
            _client.DefaultRequestHeaders.Add("Authorization", $"{token.AccessTokenType} {token.AccessToken}");
        }

        private static void GetUser(string userName)
        {
            var responseObj = GetResponse(GraphResource + $"/users/{userName}?api-version=1.6");
            if (responseObj != null)
            {
                Console.WriteLine(responseObj);
                var userId = responseObj["extension_fae445cb031543e1a5466ff1f2156da9_employeeID"];
                Console.WriteLine($"UserId: {userId}");
            }
        }

        private static void GetRoleAssignments(string userName)
        {
            var responseObj = GetResponse(GraphResource + $"/users/{userName}/appRoleAssignments?api-version=1.6");
            Console.WriteLine(responseObj);
        }

        private static JObject GetResponse(string request)
        {
            JObject jobject = null;
            try
            {
                var response = _client.GetAsync(request).Result;
                var content = response.Content.ReadAsStringAsync().Result;
                jobject = Newtonsoft.Json.JsonConvert.DeserializeObject<JObject>(content);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            return jobject;
        }

        private static async Task<AuthenticationResult> GetToken()
        {
            var context = new AuthenticationContext(AuthString, false);
            var credentials = new ClientCredential(ClientId, ClientSecret);
            return await context.AcquireTokenAsync(ResourceUrl, credentials);
        }
    }
}
