using EO.WebBrowser;
using Microsoft.Extensions.Logging;
using Neo4j.Driver;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteerMyWheel.Workers.Git;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace SteerMyWheel.Connectivity
{
    public class BitbucketClient : IClientProvider<HttpClient>
    {
        private readonly ILogger<BitbucketClient> _logger;
        private readonly GlobalConfig _config;
        private readonly Encoding _encoding = Encoding.UTF8;
        private string accessToken = "";
        private string refreshToken = "";
        private readonly string accessTokenRegex = "(?=#access_token=).*(?:&scopes)";

        public BitbucketClient(GlobalConfig config,ILogger<BitbucketClient> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async void Connect()
        {
           
            using(var client = new HttpClient())
            {
                var authCode = "";
                string formDataBoundary = String.Format("----------{0:N}", Guid.NewGuid());
                var data = new Dictionary<string, string>
                {
                    {"grant_type","authorization_code" },
                    {"code",$"{authCode}" }
                };
                var authString = $"{_config.bitbucketKey}:{_config.bitbucketSecret}";
                authString = Convert.ToBase64String(Encoding.Default.GetBytes(authString));


                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _config.bitbucketAccessTokenURI);
                
                request.Headers.Add("User-Agent", "automigration");
                request.Headers.Authorization = AuthenticationHeaderValue.Parse("Basic " + authString);
                request.Method = HttpMethod.Post;

                var content = new MultipartFormDataContent(formDataBoundary);
                foreach (var key in data.Keys)
                {
                    content.Add(new StringContent(data.GetValueOrDefault(key).ToString()),key.ToString());
                }
                request.Content = content;


                var response = await client.SendAsync(request);
                var req = request.Content.ReadAsStringAsync().Result;

                var reader = new StreamReader(response.Content.ReadAsStreamAsync().Result);
                dynamic responseBody = JsonConvert.DeserializeObject<ExpandoObject>(reader.ReadToEnd(),new ExpandoObjectConverter());
                foreach (KeyValuePair<String,object> kv in responseBody)
                {
                   if (kv.Key == "access_token") accessToken = Convert.ToString(kv.Value); 
                   if (kv.Key == "refresh_token") refreshToken = Convert.ToString(kv.Value);
                }



            }
        }

        public string getAccessCode()
        {
            string code = "";
            ThreadRunner threadRunner = new ThreadRunner();
            WebView webView = new WebView();
            threadRunner.Send(() =>
            {
                webView.LoadUrlAndWait(_config.bitbucketCodeURI);
               

                var toke = code;
            });
            
            return code;
                
        }

        public HttpClient GetConnection()
        {
            throw new NotImplementedException();
        }

        public bool createRepository(string repositoryName)
        {
            var create = $"curl -u {_config.bitbucketUsername}:\"{_config.bitbucketPassword}\" -X POST -H \"Content-Type:application/json\" -d";
            create += "'{" +
                $"\"slug\":\"{repositoryName}\"" +
                "\"forkable\": false" +
                "\"project\":{" +
                $"\"key\":\"{_config.bitbucketScriptsProject}\"" +
                "}'"
                + $" https://bitbucket.org/rest/api/1.0/projects/{_config.bitbucketScriptsProject}/repos/{repositoryName}";
            WinAPI.system(create);
            return true;

        }

        public bool isConnected()
        {
            if (accessToken != "") return true;
            return false;
        }
    }
}
