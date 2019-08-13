using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PartitionKeyAdvisor.Models
{
    public class CosmosSettings
    {
        public Uri EndpointUri { get; set; }
        public string PrimaryKey { get; set; }

        public string Database { get; set; }
    }
}
