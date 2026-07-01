
using Neo4j.Driver;

namespace Atlas.Backend.Infrastructure.Neo4j
{
    public static class Neo4jDriverFactory
    {
        public static IDriver Create(Neo4jSettings config)
        {
            return GraphDatabase.Driver(
                config.Uri,
                AuthTokens.Basic(config.User, config.Password)
            );
        }
    }
}
