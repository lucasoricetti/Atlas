using System;
using System.Collections.Generic;
using System.Text;

namespace Atlas.Backend.Infrastructure.Neo4j
{
    public class Neo4jSettings
    {
        public required string Uri { get; set; }
        public required string User { get; set; }
        public required string Password { get; set; }
        public string Database { get; set; } = "neo4j";
    }
}
