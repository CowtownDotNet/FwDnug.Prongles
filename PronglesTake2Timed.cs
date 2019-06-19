using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Flurl;
using Flurl.Http;
using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace FwDnug.Prongles
{
    public static class PronglesTake2Timed
    {
        private const int VotesToCast = 100;
        private const int IdToVoteFor = 0;
        private const string RateLimitRemainingHeader = "X-Ratelimit-Remaining";

        [FunctionName("PronglesTake2Timed")]
        public static async void Run([TimerTrigger("*/30 * * * * *")]TimerInfo myTimer, ILogger log)
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

                var responseString = await response.Content.ReadAsStringAsync();
                log.LogInformation($"Reponse: {responseString}");
                log.LogInformation($"Response Status Code: {response.StatusCode}");

                var headerPresent = response
                    .Headers
                    .TryGetValues(RateLimitRemainingHeader, out IEnumerable<string> rateLimitHeader);

                foreach (var header in response.Headers)
                {
                    log.LogDebug($"Header {header.Key}: {header.Value.FirstOrDefault()}");
                }

                if(headerPresent && rateLimitHeader.Any())
                {
                    string nativeHeader = rateLimitHeader.FirstOrDefault();
                    int.TryParse(nativeHeader, out rateLimit );
                }

                if (response.StatusCode == HttpStatusCode.OK && rateLimit > 0)
                {
                    count += VotesToCast;
                }

            } while ( rateLimit > 0 );

            log.LogInformation($"Total Votes Cast: {count}");
        }
    }
}
