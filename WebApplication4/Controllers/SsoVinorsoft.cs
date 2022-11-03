using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebApplication4.Controllers
{
    public static class TypeConverterExtension
    {
        public static byte[] ToByteArray(this string value) =>
               Convert.FromBase64String(value);
    }
    public class Token
    {
        [JsonPropertyName("access_token")]
        public string AcessToken { get; set; }


        [JsonPropertyName("refresh_token")]
        public string RefreshToken { get; set; }


        [JsonPropertyName("id_token")]
        public string IdToken { get; set; }


        [JsonPropertyName("status")]
        public bool Status { get; set; }
    }

    class SsoVinorsoft
    {

        String clientId;
        String secret;
        String callbackUrl;
        String realmUrl;
        String pubkey = null;
        private string modulus;
        private string exponent;

        public SsoVinorsoft(String realmUrl, String clientId, String secret, String callbackUrl, String modulus, string exponent)
        {
            this.realmUrl = realmUrl;
            this.clientId = clientId;
            this.secret = secret;
            this.callbackUrl = callbackUrl;
            this.modulus = modulus;
            this.exponent = exponent;
        }

        public SsoVinorsoft(String realmUrl, String clientId, String secret, String callbackUrl, String pubkey)
        {
            this.realmUrl = realmUrl;
            this.clientId = clientId;
            this.secret = secret;
            this.callbackUrl = callbackUrl;
            this.pubkey = pubkey;
        }
        public SsoVinorsoft(String realmUrl, String clientId, String secret, String callbackUrl)
        {
            this.realmUrl = realmUrl;
            this.clientId = clientId;
            this.secret = secret;
            this.callbackUrl = callbackUrl;
        }

        //...
        static byte[] FromBase64Url(string base64Url)
        {
            string padded = base64Url.Length % 4 == 0
                ? base64Url : base64Url + "====".Substring(base64Url.Length % 4);
            string base64 = padded.Replace("_", "/")
                                  .Replace("-", "+");
            return Convert.FromBase64String(base64);
        }
        public bool validateToken(string accessToken)
        {
            //
            // https://jpassing.com/2021/12/07/exporting-rsa-public-keys-in-dotnet-and-dotnet-framework/

            // pem2jwk
            // https://stackoverflow.com/questions/34403823/verifying-jwt-signed-with-the-rs256-algorithm-using-public-key-in-c-sharp
            // https://irrte.ch/jwt-js-decode/pem2jwk.html
            // https://learn.microsoft.com/en-us/dotnet/api/system.security.cryptography.rsapkcs1signaturedeformatter.verifysignature?view=net-6.0
            // 

            string modulus = this.modulus;
            string exponent = this.exponent;
            try
            {
                string tokenStr = accessToken;
                string[] tokenParts = tokenStr.Split('.');

                RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                rsa.ImportParameters(
                  new RSAParameters()
                  {
                      Modulus = FromBase64Url(modulus),
                      Exponent = FromBase64Url(exponent)
                  });

                SHA256 sha256 = SHA256.Create();
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(tokenParts[0] + '.' + tokenParts[1]));

                RSAPKCS1SignatureDeformatter rsaDeformatter = new RSAPKCS1SignatureDeformatter(rsa);
                rsaDeformatter.SetHashAlgorithm("SHA256");
                if (rsaDeformatter.VerifySignature(hash, FromBase64Url(tokenParts[2])))
                    return true;
            } catch (Exception _ex)
            {
                Console.WriteLine(_ex.StackTrace);
                return false;
            }

            return false;

            
        }
        public async Task<Token> CreatePostApi(String pathUrl, Dictionary<string, string> data)
        {
            Token token = new Token();
            token.Status = false;
            HttpClient client = new HttpClient();
            
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/x-www-form-urlencoded"));
            

            var req = new HttpRequestMessage(HttpMethod.Post, this.realmUrl + pathUrl)
            {
                Content = new FormUrlEncodedContent(data)
            };

            try
            {
                // var response = await client.SendAsync(req); // dotnet Core

                var response = client.SendAsync(req).Result;
                
                var respStream = await response.Content.ReadAsStreamAsync();

                token = await System.Text.Json.JsonSerializer.DeserializeAsync<Token>(
                    respStream,
                    new System.Text.Json.JsonSerializerOptions
                    {
                        //IgnoreNullValues = true,
                        PropertyNameCaseInsensitive = true
                    });
                token.Status = true;

            }
            catch (Exception _ex)
            {
                Console.WriteLine(_ex.Message);

                token.Status = false;
            }
            return token;
        }



        public Token GetCode(String code)
        {
            if (code == null) return new Token() { Status = false };

            Console.WriteLine(code);
            var dict = new Dictionary<string, string>();
            
            dict.Add("code", code);
            dict.Add("client_id", this.clientId);
            
            dict.Add("client_secret", this.secret);
            dict.Add("grant_type", "authorization_code");
            dict.Add("redirect_uri", this.callbackUrl);
            
            dict.Add("scope", "openid email profile");

            return CreatePostApi("protocol/openid-connect/token", dict)
                .GetAwaiter().GetResult();
        }

        public Token RefreshToken(String rfToken)
        {
            if (rfToken == null) return new Token() { Status = false };
            var dict = new Dictionary<string, string>();
            dict.Add("refresh_token", rfToken);
            dict.Add("grant_type", "refresh_token");

            return CreatePostApi("protocol/openid-connect/token", dict).GetAwaiter().GetResult();
        }
    }
}