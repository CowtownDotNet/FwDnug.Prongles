using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Linq;

namespace FwDnug.Prongles
{
    public static class PronglesTake2Timed
    {
        private const int VotesToCast = 100;
        private const int IdToVoteFor = 0;
        private const string RateLimitRemainingHeader = "X-Ratelimit-Remaining";

        [FunctionName("PronglesTake2Timed")]
        public static async void Run([TimerTrigger("0/30 * * * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            int count = 0;
            int rateLimit = 0;

            do
            {
                var response = await "https://taters.herokuapp.com"
                    .AppendPathSegment("vote")
                    .PostUrlEncodedAsync(new {
                        id = IdToVoteFor,
                        val = VotesToCast,
                    });

                log.LogInformation($"Reponse: {response.Content}");

                var headerPresent = response
                    .Headers
                    .TryGetValues(RateLimitRemainingHeader, out IEnumerable<string> rateLimitHeader);

                foreach (var header in response.Headers)
                {
                    log.LogInformation($"Header {header.Key}: {header.Value.FirstOrDefault()}");
                }

                if(headerPresent && rateLimitHeader.Any())
                {
                    string nativeHeader = rateLimitHeader.FirstOrDefault();
                    int.TryParse(nativeHeader, out rateLimit );
                }

                count += VotesToCast;

            } while ( rateLimit > 0 );

            log.LogInformation($"Total Votes Cast: {count}");
        }
    }
}
