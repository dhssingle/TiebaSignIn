using System.Collections.Generic;
using System.Threading.Tasks;
using TiebaSignIn.Domain;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace TiebaSignIn
{
    class Program
    {
        async static Task Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(conf => { conf.AddConsole(); });
            ILogger logger = loggerFactory.CreateLogger<Program>();
            
            if (args.Length == 0)
                logger.LogWarning("请在Secrets中填写BDUSS");

            Cookie cookie = Cookie.GetInstance();
            cookie.BDUSS = args[0];

            List<string> exclude = new();
            if (args.Length == 3)
                exclude = args[2].Split(',').ToList();

            Run run = new();
            await run.GetTbs();
            await run.GetFollow(exclude);
            await run.RunSign();
            
            if (args.Length >= 2)
            {
                await run.Send(args[1]);
            }
        }

    }
}
