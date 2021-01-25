using System;
using System.Text.Json;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace TiebaSignIn.Util
{
    public class Request
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;
        private readonly Domain.Cookie _cookie;
        public Request()
        {
            _loggerFactory = LoggerFactory.Create(conf => { conf.AddConsole(); });
            _logger = _loggerFactory.CreateLogger<Run>();
            _cookie = Domain.Cookie.GetInstance();
        }

        public async Task<JsonElement> GetAsync(string url)
        {
            using HttpClient client = new();
            HttpRequestMessage requestMessage = new(HttpMethod.Get, url);
            requestMessage.Headers.Add("connection", "keep-alive");
            requestMessage.Headers.Add("charset", "UTF-8");
            requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36");
            requestMessage.Headers.Add("Cookie", _cookie.ToString());
            try
            {
                var responseContent = await (await client.SendAsync(requestMessage)).Content.ReadAsStreamAsync(); ;
                return (await JsonDocument.ParseAsync(responseContent)).RootElement;

            }
            catch (Exception ex)
            {
                _logger.LogInformation("Get 请求错误 -- {0}", ex);
                throw;
            }
        }

        public async Task<JsonElement> PostAsync(string url, string body)
        {
            using HttpClient client = new();
            HttpRequestMessage requestMessage = new(HttpMethod.Post, url);
            requestMessage.Headers.Add("connection", "keep-alive");
            requestMessage.Headers.Add("Host", "tieba.baidu.com");
            requestMessage.Headers.Add("charset", "UTF-8");
            requestMessage.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/39.0.2171.71 Safari/537.36");
            requestMessage.Headers.Add("Cookie", _cookie.ToString());
            requestMessage.Content = new StringContent(body);
            try
            {
                var responseContent = await (await client.SendAsync(requestMessage)).Content.ReadAsStreamAsync();
                return (await JsonDocument.ParseAsync(responseContent)).RootElement;
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Post 请求错误 -- {0}", ex);
                throw;
            }
        }
    }
}