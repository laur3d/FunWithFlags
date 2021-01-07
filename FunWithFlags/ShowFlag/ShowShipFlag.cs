using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace FunWithFlags.ShowFlag
{
    using System.Linq;
    using Dynamitey;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Microsoft.FeatureManagement;

    public class ShowShipFlag
    {
        private readonly IFeatureManager _featureManager;
        private readonly IConfigurationRefresher _refresher;

        public ShowShipFlag(IFeatureManager featureManager, IConfigurationRefresherProvider refresherProvider)
        {
            _featureManager = featureManager;
            _refresher = refresherProvider.Refreshers.First();
        }

        [FunctionName("ShowShipFlag")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)]
            HttpRequest req, ILogger log)
        {

            await _refresher.TryRefreshAsync();
            var shipFlag = "The India Company";
            var usePirateShip = await this._featureManager.IsEnabledAsync("pirate-flag");

            if (usePirateShip)
            {
                shipFlag = "Pirate";
            }

            return (ActionResult) new OkObjectResult($"The ship has a {shipFlag} flag ");

        }
    }


}
