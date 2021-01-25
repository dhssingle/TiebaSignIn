using System.Net.Http;
using System.Text;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using TiebaSignIn.Util;
using TibaSignIn;

namespace TiebaSignIn
{
    public class Run
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly ILogger _logger;

        /// <summary>
        /// 存储用户所关注的贴吧
        /// </summary>
        private List<string> follow = new();
        /// <summary>
        /// 签到成功的贴吧列表
        /// </summary>
        private List<string> success = new();
        /// <summary>
        /// 用户的tbs
        /// </summary>
        private string tbs = "";
        /// <summary>
        /// 用户所关注的贴吧数量
        /// </summary>
        private static int followNum = 0;
        public Run()
        {
            _loggerFactory = LoggerFactory.Create(conf => { conf.AddConsole(); });
            _logger = _loggerFactory.CreateLogger<Run>();
        }
        /// <summary>
        /// 进行登录，获得 tbs ，签到的时候需要用到这个参数
        /// </summary>
        /// <returns></returns>
        public async Task GetTbs()
        {
            try
            {
                Request request = new();
                var result = await request.GetAsync(Constants.TbsUrl);
                if (result.GetProperty("is_login").GetInt16() == 1)
                {
                    _logger.LogInformation("获取tbs成功");
                    tbs = result.GetProperty("tbs").GetString();
                }
                else
                {
                    _logger.LogInformation("获取tbs失败 -- {0}", result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("获取tbs部分出现错误 -- {0}", ex);
                throw;
            }
        }

        public async Task GetFollow(List<string> exclude)
        {
            try
            {
                Request request = new();
                var result = await request.GetAsync(Constants.LikeUrl);
                _logger.LogInformation("获取贴吧列表成功");
                foreach (var item in result.GetProperty("data").GetProperty("like_forum").EnumerateArray())
                {
                    var forumName = item.GetProperty("forum_name").GetString();
                    if (!exclude.Contains(forumName))
                    {
                        if (item.GetProperty("is_sign").GetInt16() == 0)
                        {
                            follow.Add(forumName);
                        }
                        else
                        {
                            success.Add(forumName);
                        }
                    }
                }
                followNum = follow.Count + success.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError("获取贴吧列表部分出现错误 -- {0}", ex);
                throw;
            }
        }

        public async Task RunSign()
        {
            int flag = 5;
            try
            {
                Request request = new();
                while (success.Count < followNum && flag > 0)
                {
                    _logger.LogInformation("-----第 {0} 轮签到开始-----", 5 - flag + 1);
                    _logger.LogInformation("还剩 {0} 贴吧需要签到", followNum - success.Count);
                    foreach (var item in follow)
                    {
                        var body = $"kw={item}&tbs={tbs}&sign={Encryption.EncodeMD5($"kw={item}tbs={tbs}tiebaclient!!!")}";
                        var result = await request.PostAsync(Constants.SignUrl, body);
                        if (result.GetProperty("error_code").GetString() == "0")
                        {
                            success.Add(item);
                            _logger.LogInformation("{0}:签到成功", item);
                        }
                        else
                        {
                            _logger.LogInformation("{0}:签到失败", item);
                        }
                    }
                    success.ForEach(t => follow.Remove(t));
                    if (success.Count != followNum)
                    {
                        // 为防止短时间内多次请求接口，触发风控，设置每一轮签到完等待 5 分钟
                        await Task.Delay(1000 * 60 * 5);
                        await GetTbs();
                    }
                    flag--;
                }
                _logger.LogInformation("共 {0} 个贴吧 - 成功: {1} - 失败: {2}", followNum, success.Count, followNum - success.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError("签到部分出现错误 -- {0}", ex);
                throw;
            }
        }

        public async Task Send(string sckey)
        {
            var text = new StringBuilder();
            var desp = new StringBuilder();

            text.Append($"总：{followNum} - ")
            .Append($"成功: {success.Count} 失败：{followNum - success.Count}");

            desp.Append("TiebaSignIn运行结果\n\n")
            .Append($"共 {followNum} 贴吧\n\n")
            .Append($"成功：{success.Count} 失败：{followNum - success.Count}");

            var body = $"text={text}&desp={desp}";
            try
            {
                using HttpClient client = new();
                var result = await client.PostAsync($"https://sc.ftqq.com/{sckey}.send", new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded"));

                if (result.IsSuccessStatusCode)
                    _logger.LogInformation("server酱推送正常");
            }
            catch (Exception ex)
            {
                _logger.LogError("server酱发送失败 -- {0}", ex);
                throw;
            }
        }
    }
}