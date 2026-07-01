using Atlas.Backend.Application.IRepositories.Relationships;
using Atlas.Backend.Infrastructure.Neo4j;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Atlas.Backend.Infrastructure.Repositories.Neo4j;

/// <summary>
/// Repository Neo4j per operazioni CRUD e batch sulle relazioni v2.
/// </summary>
public class RelationshipsV2Repository : IRelationshipsV2Repository
{
    private const string AddOperation = "add";
    private const string RemoveOperation = "remove";
    private const string UpdateOperation = "update";

    private static readonly IReadOnlyDictionary<string, RelationshipV2Definition> Definitions =
        new Dictionary<string, RelationshipV2Definition>(StringComparer.OrdinalIgnoreCase)
        {
            // Direct relations
            ["asset/depends-on-asset"] = new("asset", "depends-on-asset", "Asset", "Asset", "Name", "DEPENDS_ON", true),
            ["asset/composed-by-service"] = new("asset", "composed-by-service", "Asset", "Service", "Name", "COMPOSED_BY", true),
            ["asset/has-setting"] = new("asset", "has-setting", "Asset", "Setting", "Name", "HAS_SETTING", false),
            ["asset/has-login-type"] = new("asset", "has-login-type", "Asset", "LoginType", "Name", "HAS_LOGIN_TYPE", false),
            ["division/uses-asset"] = new("division", "uses-asset", "Division", "Asset", "Name", "USES", false),
            ["division/owns-asset"] = new("division", "owns-asset", "Division", "Asset", "Name", "OWNS", false),
            ["service/depends-on-service"] = new("service", "depends-on-service", "Service", "Service", "Name", "DEPENDS_ON", true),
            ["service/has-setting"] = new("service", "has-setting", "Service", "Setting", "Name", "HAS_SETTING", false),
            ["service/has-contract"] = new("service", "has-contract", "Service", "Contract", "Name", "HAS_CONTRACT", false),
            ["contract/provided-by-supplier"] = new("contract", "provided-by-supplier", "Contract", "Supplier", "Name", "PROVIDED_BY", false),
            ["cloudprovider/hosts-service"] = new("cloudprovider", "hosts-service", "CloudProvider", "Service", "Name", "HOSTS", false),
            ["virtualmachine/hosts-service"] = new("virtualmachine", "hosts-service", "VirtualMachine", "Service", "Name", "HOSTS", false),
            ["process/involves-asset"] = new("process", "involves-asset", "Process", "Asset", "Name", "INVOLVES", false),
            ["process/involves-service"] = new("process", "involves-service", "Process", "Service", "Name", "INVOLVES", false),
            ["process/has-setting"] = new("process", "has-setting", "Process", "Setting", "Name", "HAS_SETTING", false),
            ["division/uses-process"] = new("division", "uses-process", "Division", "Process", "Name", "USES", false),
            ["division/owns-process"] = new("division", "owns-process", "Division", "Process", "Name", "OWNS", false),
            ["process/classified-as"] = new("process", "classified-as", "Process", "AcnMacroArea", "Name", "CLASSIFIED_AS", false),

            // Reverse relations
            ["service/required-by-asset"] = new("service", "required-by-asset", "Service", "Asset", "Name", "COMPOSED_BY", true, true),
            ["asset/depends-by-asset"] = new("asset", "depends-by-asset", "Asset", "Asset", "Name", "DEPENDS_ON", true, true),
            ["setting/used-by-asset"] = new("setting", "used-by-asset", "Setting", "Asset", "Name", "HAS_SETTING", false, true),
            ["setting/used-by-service"] = new("setting", "used-by-service", "Setting", "Service", "Name", "HAS_SETTING", false, true),
            ["logintype/used-by-asset"] = new("logintype", "used-by-asset", "LoginType", "Asset", "Name", "HAS_LOGIN_TYPE", false, true),
            ["asset/used-by-division"] = new("asset", "used-by-division", "Asset", "Division", "Name", "USES", false, true),
            ["asset/owned-by-division"] = new("asset", "owned-by-division", "Asset", "Division", "Name", "OWNS", false, true),
            ["contract/used-by-service"] = new("contract", "used-by-service", "Contract", "Service", "Name", "HAS_CONTRACT", false, true),
            ["supplier/provides-contract"] = new("supplier", "provides-contract", "Supplier", "Contract", "Name", "PROVIDED_BY", false, true),
            ["service/depends-by-service"] = new("service", "depends-by-service", "Service", "Service", "Name", "DEPENDS_ON", true, true),
            ["service/hosted-by-cloudprovider"] = new("service", "hosted-by-cloudprovider", "Service", "CloudProvider", "Name", "HOSTS", false, true),
            ["service/hosted-by-virtualmachine"] = new("service", "hosted-by-virtualmachine", "Service", "VirtualMachine", "Name", "HOSTS", false, true),
            ["asset/involved-by-process"] = new("asset", "involved-by-process", "Asset", "Process", "Name", "INVOLVES", false, true),
            ["service/involved-by-process"] = new("service", "involved-by-process", "Service", "Process", "Name", "INVOLVES", false, true),
            ["setting/used-by-process"] = new("setting", "used-by-process", "Setting", "Process", "Name", "HAS_SETTING", false, true),
            ["process/used-by-division"] = new("process", "used-by-division", "Process", "Division", "Name", "USES", false, true),
            ["process/owned-by-division"] = new("process", "owned-by-division", "Process", "Division", "Name", "OWNS", false, true),
            ["acnmacroarea/classifies"] = new("acnmacroarea", "classifies", "AcnMacroArea", "Process", "Name", "CLASSIFIED_AS", false, true)
        };

    private readonly IDriver _driver;
    private readonly string _db;

    public RelationshipsV2Repository(IDriver driver, Neo4jSettings settings)
    {
        _driver = driver;
        _db = settings.Database;
    }

    public bool TryResolveDefinition(string sourceType, string relationType, out RelationshipV2Definition definition)
    {
        return Definitions.TryGetValue($"{sourceType}/{relationType}", out definition!);
    }

    public async Task<IReadOnlyList<RelationshipV2Item>> GetActiveRelationsAsync(
        RelationshipV2Definition definition,
        string sourceId,
        CancellationToken ct = default)
    {
        using var session = _driver.AsyncSession(options => options.WithDatabase(_db));
        await EnsureNodeExistsAsync(session, definition.SourceLabel, sourceId, SourceNodeNotFoundMessage(definition, sourceId));

        var query = definition.ReverseDirection
            ? $@"
                MATCH (t:{definition.TargetLabel})-[r:{definition.Neo4jRelationType}]->(src:{definition.SourceLabel} {{Id: $sourceId}})
                RETURN
                    coalesce(r.RelationId, elementId(r)) AS relationId,
                    t.Id AS targetId,
                    coalesce(t.{definition.TargetLabelProperty}, '') AS targetLabel,
                    {(definition.SupportsCritical ? "coalesce(r.IsCritical, false)" : "null")} AS isCritical
                ORDER BY toLower(targetLabel), targetId"
                            : $@"
                MATCH (src:{definition.SourceLabel} {{Id: $sourceId}})-[r:{definition.Neo4jRelationType}]->(t:{definition.TargetLabel})
                RETURN
                    coalesce(r.RelationId, elementId(r)) AS relationId,
                    t.Id AS targetId,
                    coalesce(t.{definition.TargetLabelProperty}, '') AS targetLabel,
                    {(definition.SupportsCritical ? "coalesce(r.IsCritical, false)" : "null")} AS isCritical
                ORDER BY toLower(targetLabel), targetId";

        var result = await session.RunAsync(query, new { sourceId });
        return await result.ToListAsync(record => MapRelation(definition, record));
    }

    public async Task<RelationshipV2CandidatesPage> GetCandidatesAsync(
        RelationshipV2Definition definition,
        string sourceId,
        string? search,
        string? cursor,
        int limit,
        string sortDir,
        bool excludeLinked,
        bool includeTotal,
        CancellationToken ct = default)
    {
        using var session = _driver.AsyncSession(options => options.WithDatabase(_db));
        await EnsureNodeExistsAsync(session, definition.SourceLabel, sourceId, SourceNodeNotFoundMessage(definition, sourceId));

        var (cursorLabel, cursorId) = DecodeCursor(cursor);
        var normalizedSearch = (search ?? string.Empty).Trim().ToLowerInvariant();
        var normalizedSortDir = string.Equals(sortDir, "desc", StringComparison.OrdinalIgnoreCase) ? "desc" : "asc";
        var comparisonOperator = normalizedSortDir == "asc" ? ">" : "<";
        var orderDirection = normalizedSortDir == "asc" ? "ASC" : "DESC";

        var linkClause = definition.ReverseDirection
            ? $"NOT (t)-[:{definition.Neo4jRelationType}]->(src)"
            : $"NOT (src)-[:{definition.Neo4jRelationType}]->(t)";

        var query = $@"
            MATCH (src:{definition.SourceLabel} {{Id: $sourceId}})
            MATCH (t:{definition.TargetLabel})
            WHERE
                t.Id <> $sourceId
                AND ($search = '' OR toLower(coalesce(t.{definition.TargetLabelProperty}, '')) CONTAINS $search)
                AND ($excludeLinked = false OR {linkClause})
                AND ($cursorLabel = '' OR (
                    toLower(coalesce(t.{definition.TargetLabelProperty}, '')) {comparisonOperator} $cursorLabel
                    OR (toLower(coalesce(t.{definition.TargetLabelProperty}, '')) = $cursorLabel AND t.Id {comparisonOperator} $cursorId)
                ))
            RETURN
                t.Id AS targetId,
                coalesce(t.{definition.TargetLabelProperty}, '') AS targetLabel
            ORDER BY toLower(targetLabel) {orderDirection}, targetId {orderDirection}
            LIMIT $limitPlusOne";

        var result = await session.RunAsync(query, new
        {
            sourceId,
            search = normalizedSearch,
            excludeLinked,
            cursorLabel,
            cursorId,
            limitPlusOne = limit + 1
        });

        var rows = await result.ToListAsync(record => new RelationshipV2CandidateItem(
            record["targetId"].As<string>(),
            record["targetLabel"].As<string>()));

        var hasMore = rows.Count > limit;
        var pageItems = hasMore ? rows.Take(limit).ToList() : rows;
        var nextCursor = hasMore
            ? EncodeCursor(pageItems[^1].TargetLabel.ToLowerInvariant(), pageItems[^1].TargetId)
            : null;

        long? totalApprox = null;

        if (includeTotal)
        {
            totalApprox = await CountCandidatesAsync(
                session,
                definition,
                sourceId,
                normalizedSearch,
                excludeLinked);
        }

        return new RelationshipV2CandidatesPage(pageItems, nextCursor, totalApprox);
    }

    public async Task<RelationshipV2Item> AddAsync(
        RelationshipV2Definition definition,
        string sourceId,
        string targetId,
        bool? isCritical,
        CancellationToken ct = default)
    {
        using var session = _driver.AsyncSession(options => options.WithDatabase(_db));

        return await session.ExecuteWriteAsync(async tx =>
        {
            await EnsureNodeExistsAsync(tx, definition.SourceLabel, sourceId, SourceNodeNotFoundMessage(definition, sourceId));
            await EnsureNodeExistsAsync(tx, definition.TargetLabel, targetId, $"Target con id '{targetId}' non trovato.");

            if (sourceId == targetId)
            {
                throw new ArgumentException("Self relation non consentita.");
            }

            if (await RelationshipExistsByTargetAsync(tx, definition, sourceId, targetId))
            {
                throw new DuplicateNameException("Relazione già esistente.");
            }

            return await CreateRelationAsync(tx, definition, sourceId, targetId, isCritical);
        });
    }

    public async Task<RelationshipV2Item> UpdateAsync(
        RelationshipV2Definition definition,
        string sourceId,
        string relationId,
        bool? isCritical,
        CancellationToken ct = default)
    {
        if (!definition.SupportsCritical)
        {
            throw new ArgumentException("La relazione selezionata non supporta IsCritical.");
        }

        using var session = _driver.AsyncSession(options => options.WithDatabase(_db));

        return await session.ExecuteWriteAsync(async tx =>
        {
            await EnsureNodeExistsAsync(tx, definition.SourceLabel, sourceId, SourceNodeNotFoundMessage(definition, sourceId));

            var query = definition.ReverseDirection
                ? $@"
                    MATCH (t:{definition.TargetLabel})-[r:{definition.Neo4jRelationType}]->(src:{definition.SourceLabel} {{Id: $sourceId}})
                    WHERE coalesce(r.RelationId, elementId(r)) = $relationId
                    SET r.IsCritical = $isCritical
                    RETURN
                        coalesce(r.RelationId, elementId(r)) AS relationId,
                        t.Id AS targetId,
                        coalesce(t.{definition.TargetLabelProperty}, '') AS targetLabel,
                        coalesce(r.IsCritical, false) AS isCritical"
                                    : $@"
                    MATCH (src:{definition.SourceLabel} {{Id: $sourceId}})-[r:{definition.Neo4jRelationType}]->(t:{definition.TargetLabel})
                    WHERE coalesce(r.RelationId, elementId(r)) = $relationId
                    SET r.IsCritical = $isCritical
                    RETURN
                        coalesce(r.RelationId, elementId(r)) AS relationId,
                        t.Id AS targetId,
                        coalesce(t.{definition.TargetLabelProperty}, '') AS targetLabel,
                        coalesce(r.IsCritical, false) AS isCritical";

            var result = await tx.RunAsync(query, new { sourceId, relationId, isCritical });
            var record = await result.SingleOrDefaultAsync()
                ?? throw new KeyNotFoundException($"Relazione '{relationId}' non trovata.");

            return MapRelation(definition, record);
        });
    }

    public async Task<bool> RemoveAsync(
        RelationshipV2Definition definition,
        string sourceId,
        string relationId,
        CancellationToken ct = default)
    {
        using var session = _driver.AsyncSession(options => options.WithDatabase(_db));

        return await session.ExecuteWriteAsync(async tx =>
        {
            await EnsureNodeExistsAsync(tx, definition.SourceLabel, sourceId, SourceNodeNotFoundMessage(definition, sourceId));
            return await RemoveRelationAsync(tx, definition, sourceId, relationId);
        });
    }

    public async Task<RelationshipV2BatchResult> ExecuteBatchAsync(
        RelationshipV2Definition definition,
        string sourceId,
        IReadOnlyList<RelationshipV2BatchOperation> operations,
        CancellationToken ct = default)
    {
        using var session = _driver.AsyncSession(options => options.WithDatabase(_db));

        return await session.ExecuteWriteAsync(async tx =>
        {
            await EnsureNodeExistsAsync(tx, definition.SourceLabel, sourceId, SourceNodeNotFoundMessage(definition, sourceId));

            var added = 0;
            var removed = 0;
            var updated = 0;
            var skipped = 0;

            foreach (var operation in operations)
            {
                switch (operation.Op)
                {
                    case AddOperation:
                    {
                        var targetId = operation.TargetId!;
                        await EnsureNodeExistsAsync(tx, definition.TargetLabel, targetId, $"Target con id '{targetId}' non trovato.");

                        if (sourceId == targetId)
                        {
                            throw new ArgumentException("Self relation non consentita.");
                        }

                        if (await RelationshipExistsByTargetAsync(tx, definition, sourceId, targetId))
                        {
                            skipped++;
                            break;
                        }

                        await CreateRelationAsync(tx, definition, sourceId, targetId, operation.IsCritical);
                        added++;
                        break;
                    }
                    case RemoveOperation:
                    {
                        var didRemove = await RemoveRelationAsync(tx, definition, sourceId, operation.RelationId!);
                        if (didRemove)
                        {
                            removed++;
                        }
                        else
                        {
                            skipped++;
                        }

                        break;
                    }
                    case UpdateOperation:
                    {
                        if (!definition.SupportsCritical)
                        {
                            throw new ArgumentException("IsCritical non è supportato per il relationType selezionato.");
                        }

                        var didUpdate = await UpdateRelationAsync(
                            tx,
                            definition,
                            sourceId,
                            operation.RelationId!,
                            operation.IsCritical!.Value);

                        if (didUpdate)
                        {
                            updated++;
                        }
                        else
                        {
                            skipped++;
                        }

                        break;
                    }
                    default:
                        throw new ArgumentException($"Operazione non supportata: '{operation.Op}'.");
                }
            }

            var currentRelations = await GetActiveRelationsAsync(tx, definition, sourceId);
            var summary = new RelationshipV2BatchSummary(added, removed, updated, skipped);
            return new RelationshipV2BatchResult(summary, currentRelations);
        });
    }

    private static RelationshipV2Item MapRelation(RelationshipV2Definition definition, IRecord record)
    {
        return new RelationshipV2Item(
            record["relationId"].As<string>(),
            record["targetId"].As<string>(),
            record["targetLabel"].As<string>(),
            definition.SupportsCritical ? record["isCritical"].As<bool>() : null);
    }

    private async Task<IReadOnlyList<RelationshipV2Item>> GetActiveRelationsAsync(
        IAsyncQueryRunner tx,
        RelationshipV2Definition definition,
        string sourceId)
    {
        var query = definition.ReverseDirection
            ? $@"
                MATCH (t:{definition.TargetLabel})-[r:{definition.Neo4jRelationType}]->(src:{definition.SourceLabel} {{Id: $sourceId}})
                RETURN
                    coalesce(r.RelationId, elementId(r)) AS relationId,
                    t.Id AS targetId,
                    coalesce(t.{definition.TargetLabelProperty}, '') AS targetLabel,
                    {(definition.SupportsCritical ? "coalesce(r.IsCritical, false)" : "null")} AS isCritical
                ORDER BY toLower(targetLabel), targetId"
                            : $@"
                MATCH (src:{definition.SourceLabel} {{Id: $sourceId}})-[r:{definition.Neo4jRelationType}]->(t:{definition.TargetLabel})
                RETURN
                    coalesce(r.RelationId, elementId(r)) AS relationId,
                    t.Id AS targetId,
                    coalesce(t.{definition.TargetLabelProperty}, '') AS targetLabel,
                    {(definition.SupportsCritical ? "coalesce(r.IsCritical, false)" : "null")} AS isCritical
                ORDER BY toLower(targetLabel), targetId";

        var result = await tx.RunAsync(query, new { sourceId });
        return await result.ToListAsync(record => MapRelation(definition, record));
    }

    private static async Task<long> CountCandidatesAsync(
        IAsyncSession session,
        RelationshipV2Definition definition,
        string sourceId,
        string normalizedSearch,
        bool excludeLinked)
    {
        var linkClause = definition.ReverseDirection
            ? $"NOT (t)-[:{definition.Neo4jRelationType}]->(src)"
            : $"NOT (src)-[:{definition.Neo4jRelationType}]->(t)";

        var countQuery = $@"
            MATCH (src:{definition.SourceLabel} {{Id: $sourceId}})
            MATCH (t:{definition.TargetLabel})
            WHERE
                t.Id <> $sourceId
                AND ($search = '' OR toLower(coalesce(t.{definition.TargetLabelProperty}, '')) CONTAINS $search)
                AND ($excludeLinked = false OR {linkClause})
            RETURN count(t) AS total";

        var countResult = await session.RunAsync(countQuery, new
        {
            sourceId,
            search = normalizedSearch,
            excludeLinked
        });

        var countRecord = await countResult.SingleAsync();
        return countRecord["total"].As<long>();
    }

    private static async Task EnsureNodeExistsAsync(IAsyncQueryRunner runner, string label, string id, string errorMessage)
    {
        var query = $"MATCH (n:{label} {{Id: $id}}) RETURN count(n) > 0 AS exists";
        var result = await runner.RunAsync(query, new { id });
        var record = await result.SingleAsync();

        if (!record["exists"].As<bool>())
        {
            throw new KeyNotFoundException(errorMessage);
        }
    }

    private static async Task<bool> RelationshipExistsByTargetAsync(
        IAsyncQueryRunner tx,
        RelationshipV2Definition definition,
        string sourceId,
        string targetId)
    {
        var query = definition.ReverseDirection
            ? $@"
                MATCH (t:{definition.TargetLabel} {{Id: $targetId}})-[r:{definition.Neo4jRelationType}]->(src:{definition.SourceLabel} {{Id: $sourceId}})
                RETURN count(r) > 0 AS exists"
                            : $@"
                MATCH (src:{definition.SourceLabel} {{Id: $sourceId}})-[r:{definition.Neo4jRelationType}]->(t:{definition.TargetLabel} {{Id: $targetId}})
                RETURN count(r) > 0 AS exists";

        var result = await tx.RunAsync(query, new { sourceId, targetId });
        var record = await result.SingleAsync();
        return record["exists"].As<bool>();
    }

    private async Task<RelationshipV2Item> CreateRelationAsync(
        IAsyncQueryRunner tx,
        RelationshipV2Definition definition,
        string sourceId,
        string targetId,
        bool? isCritical)
    {
        var relationId = Guid.NewGuid().ToString("N");

        var criticalPart = definition.SupportsCritical
            ? ", IsCritical: $isCritical"
            : string.Empty;

        var returnCritical = definition.SupportsCritical ? "coalesce(r.IsCritical, false)" : "null";

        var query = definition.ReverseDirection
            ? $@"
                MATCH (src:{definition.SourceLabel} {{Id: $sourceId}})
                MATCH (t:{definition.TargetLabel} {{Id: $targetId}})
                CREATE (t)-[r:{definition.Neo4jRelationType} {{
                    RelationId: $relationId{criticalPart}
                }}]->(src)
                RETURN
                    r.RelationId AS relationId,
                    t.Id AS targetId,
                    coalesce(t.{definition.TargetLabelProperty}, '') AS targetLabel,
                    {returnCritical} AS isCritical"
                            : $@"
                MATCH (src:{definition.SourceLabel} {{Id: $sourceId}})
                MATCH (t:{definition.TargetLabel} {{Id: $targetId}})
                CREATE (src)-[r:{definition.Neo4jRelationType} {{
                    RelationId: $relationId{criticalPart}
                }}]->(t)
                RETURN
                r.RelationId AS relationId,
                t.Id AS targetId,
                coalesce(t.{definition.TargetLabelProperty}, '') AS targetLabel,
                {returnCritical} AS isCritical";

        var result = await tx.RunAsync(query, new
        {
            sourceId,
            targetId,
            relationId,
            isCritical = isCritical ?? false
        });

        var record = await result.SingleAsync();
        return MapRelation(definition, record);
    }

    private async Task<bool> UpdateRelationAsync(
        IAsyncQueryRunner tx,
        RelationshipV2Definition definition,
        string sourceId,
        string relationId,
        bool isCritical)
    {
        var query = definition.ReverseDirection
            ? $@"
                MATCH (t:{definition.TargetLabel})-[r:{definition.Neo4jRelationType}]->(src:{definition.SourceLabel} {{Id: $sourceId}})
                WHERE coalesce(r.RelationId, elementId(r)) = $relationId
                SET r.IsCritical = $isCritical
                RETURN coalesce(r.RelationId, elementId(r)) AS relationId"
                            : $@"
                MATCH (src:{definition.SourceLabel} {{Id: $sourceId}})-[r:{definition.Neo4jRelationType}]->(t:{definition.TargetLabel})
                WHERE coalesce(r.RelationId, elementId(r)) = $relationId
                SET r.IsCritical = $isCritical
                RETURN coalesce(r.RelationId, elementId(r)) AS relationId";

        var result = await tx.RunAsync(query, new { sourceId, relationId, isCritical });
        var record = await result.SingleOrDefaultAsync();
        return record is not null;
    }

    private static async Task<bool> RemoveRelationAsync(
        IAsyncQueryRunner tx,
        RelationshipV2Definition definition,
        string sourceId,
        string relationId)
    {
        var query = definition.ReverseDirection
            ? $@"
                MATCH (t:{definition.TargetLabel})-[r:{definition.Neo4jRelationType}]->(src:{definition.SourceLabel} {{Id: $sourceId}})
                WHERE coalesce(r.RelationId, elementId(r)) = $relationId
                DELETE r
                RETURN 1 AS removed"
                            : $@"
                MATCH (src:{definition.SourceLabel} {{Id: $sourceId}})-[r:{definition.Neo4jRelationType}]->(t:{definition.TargetLabel})
                WHERE coalesce(r.RelationId, elementId(r)) = $relationId
                DELETE r
                RETURN 1 AS removed";

        var result = await tx.RunAsync(query, new { sourceId, relationId });
        var record = await result.SingleOrDefaultAsync();
        return record is not null;
    }

    private static string SourceNodeNotFoundMessage(RelationshipV2Definition definition, string sourceId)
    {
        return $"{definition.SourceType} con id '{sourceId}' non trovato.";
    }

    private static string? EncodeCursor(string label, string id)
    {
        if (string.IsNullOrWhiteSpace(label) || string.IsNullOrWhiteSpace(id))
        {
            return null;
        }

        var raw = $"{label}\t{id}";
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(raw));
    }

    private static (string Label, string Id) DecodeCursor(string? cursor)
    {
        if (string.IsNullOrWhiteSpace(cursor))
        {
            return (string.Empty, string.Empty);
        }

        try
        {
            var raw = Encoding.UTF8.GetString(Convert.FromBase64String(cursor));
            var tokens = raw.Split('\t');

            if (tokens.Length != 2)
            {
                throw new FormatException();
            }

            return (tokens[0], tokens[1]);
        }
        catch
        {
            throw new ArgumentException("Cursor non valido.");
        }
    }
}
