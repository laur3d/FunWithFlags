using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace FunWithFlags.MoreComplexExample
{
    using System.Linq;
    using Microsoft.Extensions.Configuration.AzureAppConfiguration;
    using Microsoft.FeatureManagement;

    public class ShipDefenseOrchestrator
    {
        private readonly IFeatureManager _featureManager;
        private readonly IConfigurationRefresher _refresher;

        public ShipDefenseOrchestrator(IFeatureManager featureManager, IConfigurationRefresherProvider refresherProvider)
        {
            _featureManager = featureManager;
            _refresher = refresherProvider.Refreshers.First();
        }

        [FunctionName(nameof(ShipDefenseOrchestrator))]
        public  async Task<List<string>> Run(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var actions = new List<string>();

            await this._refresher.TryRefreshAsync();
            var isParanoid = await this._featureManager.IsEnabledAsync("IsParanoid");
            var flag = await context.CallActivityAsync<string>(nameof(CheckOtherShipsFlag), new { });
            if (flag == "Pirate")
            {
                actions.Add(await context.CallActivityAsync<string>(nameof(PrepareDefensiveManeuvers), null));
                actions.Add(await context.CallActivityAsync<string>(nameof(FireCanons), null));
            }

            if (isParanoid)
            {
                actions.Add(await context.CallActivityAsync<string>(nameof(PrepareDefensiveManeuvers), null));
            }

            return actions;
        }

        [FunctionName(nameof(CheckOtherShipsFlag))]
        public async Task<string> CheckOtherShipsFlag([ActivityTrigger] object obj)
        {
            await _refresher.TryRefreshAsync();
            var shipFlag = "The West India Company";
            var usePirateShip = await this._featureManager.IsEnabledAsync("pirate-flag");

            if (usePirateShip)
            {
                shipFlag = "Pirate";
            }

            return shipFlag;
        }

        [FunctionName(nameof(PrepareDefensiveManeuvers))]
        public async Task<string> PrepareDefensiveManeuvers([ActivityTrigger] object obj)
        {
            return "To battle stations!";
        }

        [FunctionName(nameof(FireCanons))]
        public async Task<string> FireCanons([ActivityTrigger] object obj)
        {
            return "BOOM!";
        }

        [FunctionName(nameof(IsParanoid))]
        public async Task<bool> IsParanoid([ActivityTrigger] object unit)
        {
            await this._refresher.TryRefreshAsync();
            return await this._featureManager.IsEnabledAsync("IsParanoid");
        }

        [FunctionName("ShipDefenseOrchestrator_HttpStart")]
        public async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")]
            HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync(nameof(ShipDefenseOrchestrator), null);

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}
