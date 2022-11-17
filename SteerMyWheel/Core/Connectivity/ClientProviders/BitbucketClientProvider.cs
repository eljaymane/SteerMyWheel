using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SteerMyWheel.Configuration;
using SteerMyWheel.Domain.Connectivity.ClientProvider;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace SteerMyWheel.Core.Connectivity.ClientProviders
{
    public class BitbucketClientProvider : BaseClientProvider<HttpClient>
    {
        private readonly ILogger<BitbucketClientProvider> _logger;
        private readonly GlobalConfig _config;
        private readonly Encoding _encoding = Encoding.UTF8;
        private string accessToken = "";
        private string refreshToken = "";

        public BitbucketClientProvider(GlobalConfig config, ILogger<BitbucketClientProvider> logger)
        {
            _config = config;
            _logger = logger;
        }

        public override async Task Connect()
        {
            using (var client = new HttpClient())
            {
                var authCode = "";
                string formDataBoundary = string.Format("----------{0:N}", Guid.NewGuid());
                var data = new Dictionary<string, string>
                {
                    {"grant_type","client_credentials" },
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
                    content.Add(new StringContent(data.GetValueOrDefault(key).ToString()), key.ToString());
                }
                request.Content = content;


                var response = await client.SendAsync(request);
                var req = request.Content.ReadAsStringAsync().Result;

                var reader = new StreamReader(response.Content.ReadAsStreamAsync().Result);
                dynamic responseBody = JsonConvert.DeserializeObject<ExpandoObject>(reader.ReadToEnd(), new ExpandoObjectConverter());
                foreach (KeyValuePair<string, object> kv in responseBody)
                {
                    if (kv.Key == "access_token") accessToken = Convert.ToString(kv.Value);
                    if (kv.Key == "refresh_token") refreshToken = Convert.ToString(kv.Value);
                }



            }
        }

        public Task<bool> createRepositoryAsync(string repositoryName, bool isPrivate)
        {
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, _config.bitbucketScriptsAPI + repositoryName.ToLower());
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var data = new Dictionary<string, object>
            {
                {"scm","git" },
                {"project",new Dictionary<string, string>
                {
                    { "key", _config.bitbucketScriptsProject}
                }
                },
                {"is_private",isPrivate}

            };
            var content = new StringContent(JsonConvert.SerializeObject(data), _encoding, new MediaTypeHeaderValue("application/json"));
            request.Content = content;
            request.Method = HttpMethod.Post;

            using (var client = new HttpClient())
            {
                var response = client.SendAsync(request).Result;
                var reader = new StreamReader(response.Content.ReadAsStreamAsync().Result);
                if (response.IsSuccessStatusCode) return Task.FromResult(true);
            }

            return Task.FromResult(false);

        }

        public bool isConnected()
        {
            if (accessToken != "") return true;
            return false;
        }


        public override HttpClient GetConnection()
        {
            return new HttpClient();
        }

        public override void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }
}
