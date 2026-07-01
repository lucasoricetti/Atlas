import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { LucideIcon } from "lucide-react";
import api from "../api/client";
import { ENTITY_ICONS } from "../constants/entityIcons";
import EntityIcon from "./EntityIcon";

interface Props {
    title: string;
    sourceType:
    | "asset"
    | "service"
    | "process"
    | "division"
    | "cloudprovider"
    | "virtualmachine"
    | "setting"
    | "logintype"
    | "contract"
    | "supplier"
    | "acnmacroarea"
    ;
    sourceId: string;
    relationType:
    | "depends-on-asset"
    | "depends-on-service"
    | "composed-by-service"
    | "required-by-asset"
    | "uses-asset"
    | "used-by-asset"
    | "used-by-service"
    | "used-by-division"
    | "owns-asset"
    | "owned-by-division"
    | "has-setting"
    | "has-login-type"
    | "has-contract"
    | "provided-by-supplier"
    | "provides-contract"
    | "hosts-service"
    | "hosted-by-cloudprovider"
    | "hosted-by-virtualmachine"
    | "involves-asset"
    | "involves-service"
    | "involved-by-process"
    | "used-by-process"
    | "uses-process"
    | "owns-process"
    | "classified-as"
    | "classifies"
    ;
    supportsCritical?: boolean;
}

interface RelationshipItem {
    relationId: string;
    targetId: string;
    targetLabel: string;
    isCritical?: boolean;
}

interface CandidateItem {
    targetId: string;
    targetLabel: string;
}

interface CandidatesResponse {
    items: CandidateItem[];
    nextCursor?: string | null;
}

interface DraftAdd {
    targetLabel: string;
    isCritical: boolean;
}

type ApiFriendlyError = {
    friendlyMessage?: string;
};

type PendingOperation =
    | { op: "add"; targetId: string; isCritical?: boolean }
    | { op: "remove"; relationId: string }
    | { op: "update"; relationId: string; isCritical: boolean };

const PAGE_ICON_BY_SOURCE_TYPE: Record<Props["sourceType"], LucideIcon> = {
    asset: ENTITY_ICONS.assets,
    service: ENTITY_ICONS.services,
    division: ENTITY_ICONS.divisions,
    cloudprovider: ENTITY_ICONS.cloudProviders,
    virtualmachine: ENTITY_ICONS.virtualMachines,
    setting: ENTITY_ICONS.settings,
    logintype: ENTITY_ICONS.loginTypes,
    contract: ENTITY_ICONS.contracts,
    supplier: ENTITY_ICONS.suppliers,
    process: ENTITY_ICONS.processes,
    acnmacroarea: ENTITY_ICONS.acnMacroAreas,
};

export default function RelationPage({
    title,
    sourceType,
    sourceId,
    relationType,
    supportsCritical = false,
}: Props) {
    const pageIcon = PAGE_ICON_BY_SOURCE_TYPE[sourceType];
    const nextCursorRef = useRef<string | null>(null);
    const [items, setItems] = useState<RelationshipItem[]>([]);
    const [candidates, setCandidates] = useState<CandidateItem[]>([]);
    const [nextCursor, setNextCursor] = useState<string | null>(null);
    const [query, setQuery] = useState("");
    const [loading, setLoading] = useState(true);
    const [loadingCandidates, setLoadingCandidates] = useState(false);
    const [error, setError] = useState("");
    const [saving, setSaving] = useState(false);
    const [draftAdds, setDraftAdds] = useState<Record<string, DraftAdd>>({});
    const [draftRemovals, setDraftRemovals] = useState<Record<string, boolean>>({});
    const [draftCriticalUpdates, setDraftCriticalUpdates] = useState<Record<string, boolean>>({});

    const baseEndpoint = useMemo(() => {
        return `/api/v2/${sourceType}/${sourceId}/${relationType}`;
    }, [sourceType, sourceId, relationType]);

    const loadRelations = useCallback(async () => {
        const response = await api.get(baseEndpoint);
        setItems(response.data ?? []);
    }, [baseEndpoint]);

    const loadCandidates = useCallback(async (reset: boolean) => {
        setLoadingCandidates(true);
        try {
            const params: Record<string, string | number | boolean> = {
                search: query,
                limit: 12,
                sortBy: "name",
                sortDir: "asc",
                excludeLinked: true,
            };

            if (!reset && nextCursorRef.current) {
                params.cursor = nextCursorRef.current;
            }

            const response = await api.get<CandidatesResponse>(`${baseEndpoint}/candidates`, { params });
            const payload = response.data;
            setCandidates((prev) => (reset ? payload.items ?? [] : [...prev, ...(payload.items ?? [])]));
            const newCursor = payload.nextCursor ?? null;
            nextCursorRef.current = newCursor;
            setNextCursor(newCursor);
        } finally {
            setLoadingCandidates(false);
        }
    }, [baseEndpoint, query]);

    const load = useCallback(async () => {
        setLoading(true);
        setError("");

        try {
            await Promise.all([loadRelations(), loadCandidates(true)]);
        } catch (e: unknown) {
            setError((e as ApiFriendlyError)?.friendlyMessage ?? "Error while loading.");
        } finally {
            setLoading(false);
        }
    }, [loadCandidates, loadRelations]);

    const resetDraft = useCallback(() => {
        setDraftAdds({});
        setDraftRemovals({});
        setDraftCriticalUpdates({});
    }, []);

    useEffect(() => {
        if (!sourceId) return;
        resetDraft();
        void load();
    }, [load, resetDraft, sourceId]);

    useEffect(() => {
        if (!sourceId) return;
        const id = window.setTimeout(() => {
            loadCandidates(true).catch((e: unknown) => {
                setError((e as ApiFriendlyError)?.friendlyMessage ?? "Error while loading candidates.");
            });
        }, 250);

        return () => window.clearTimeout(id);
    }, [loadCandidates, sourceId, query]);

    const existingIds = useMemo(() => {
        return new Set(items.map((it) => String(it.targetId)));
    }, [items]);

    const visibleSuggestions = useMemo(() => {
        return candidates.filter((o) => !existingIds.has(String(o.targetId)));
    }, [candidates, existingIds]);

    const relationById = useMemo(() => {
        const map = new Map<string, RelationshipItem>();
        items.forEach((i) => map.set(String(i.relationId), i));
        return map;
    }, [items]);

    function toggleCandidateSelection(candidate: CandidateItem, checked: boolean) {
        const key = String(candidate.targetId);
        setDraftAdds((prev) => {
            if (!checked) {
                const next = { ...prev };
                delete next[key];
                return next;
            }

            return {
                ...prev,
                [key]: prev[key] ?? {
                    targetLabel: candidate.targetLabel,
                    isCritical: false,
                },
            };
        });
    }

    function toggleCandidateCritical(targetId: string, checked: boolean) {
        setDraftAdds((prev) => {
            const existing = prev[targetId];
            if (!existing) return prev;
            return {
                ...prev,
                [targetId]: {
                    ...existing,
                    isCritical: checked,
                },
            };
        });
    }

    function toggleDraftRemoval(relationId: string, checked: boolean) {
        setDraftRemovals((prev) => ({
            ...prev,
            [relationId]: checked,
        }));
    }

    function toggleDraftCriticalUpdate(relationId: string, checked: boolean) {
        const original = relationById.get(relationId)?.isCritical ?? false;
        setDraftCriticalUpdates((prev) => {
            if (checked === original) {
                const next = { ...prev };
                delete next[relationId];
                return next;
            }
            return {
                ...prev,
                [relationId]: checked,
            };
        });
    }

    const pendingOperations = useMemo<PendingOperation[]>(() => {
        const addOps = Object.entries(draftAdds).map(([targetId, draft]) => ({
            op: "add",
            targetId,
            isCritical: supportsCritical ? draft.isCritical : undefined,
        } as const));

        const removeOps = Object.entries(draftRemovals)
            .filter(([, checked]) => Boolean(checked))
            .map(([relationId]) => ({
                op: "remove",
                relationId,
            } as const));

        const updateOps = Object.entries(draftCriticalUpdates)
            .filter(([relationId]) => !draftRemovals[relationId])
            .map(([relationId, isCritical]) => ({
                op: "update",
                relationId,
                isCritical,
            } as const));

        return [...addOps, ...updateOps, ...removeOps];
    }, [draftAdds, draftRemovals, draftCriticalUpdates, supportsCritical]);

    const pendingAddCount = Object.keys(draftAdds).length;
    const pendingRemoveCount = Object.values(draftRemovals).filter(Boolean).length;
    const pendingUpdateCount = Object.keys(draftCriticalUpdates).filter(
        (relationId) => !draftRemovals[relationId]
    ).length;
    const hasPending = pendingOperations.length > 0;

    async function saveDraftChanges() {
        if (!hasPending) return;

        try {
            setSaving(true);
            await api.post(`${baseEndpoint}/batch`, {
                operations: pendingOperations,
            });
            resetDraft();
            await Promise.all([loadRelations(), loadCandidates(true)]);
        } catch (e: unknown) {
            setError((e as ApiFriendlyError)?.friendlyMessage ?? "Error while saving changes.");
        } finally {
            setSaving(false);
        }
    }

    if (!sourceId) {
        return <div className="alert alert-error">Invalid entity ID.</div>;
    }

    return (
        <div className="relation-page">
            <div className="relation-header">
                <h3 className="page-title-with-icon">
                    <EntityIcon icon={pageIcon} size={24} className="page-title-icon" />
                    <span>{title}</span>
                </h3>
                <p>Select the changes and save in a single step.</p>
            </div>

            {error && <div className="alert alert-error">{error}</div>}

            <div className="relation-top-grid">
                <section className="card relation-list-card">
                    <h4>Existing relationships</h4>

                    {loading ? (
                        <p className="relation-muted">Loading...</p>
                    ) : items.length === 0 ? (
                        <p className="relation-muted">No relationship present.</p>
                    ) : (
                        <div className="relation-list">
                            {items.map((i) => {
                                const label = i.targetLabel;
                                const relationId = i.relationId;
                                const markedToRemove = Boolean(draftRemovals[relationId]);
                                const criticalValue =
                                    draftCriticalUpdates[relationId] ?? Boolean(i.isCritical);

                                return (
                                    <div
                                        key={relationId}
                                        className={`relation-item ${markedToRemove ? "is-pending-remove" : ""}`}
                                    >
                                        <div className="relation-item-main">
                                            <strong>{label}</strong>
                                            <div className="relation-item-actions">
                                                <label className="relation-suggestion-critical relation-remove-check">
                                                    <input
                                                        type="checkbox"
                                                        checked={markedToRemove}
                                                        disabled={saving}
                                                        onChange={(e) => toggleDraftRemoval(relationId, e.target.checked)}
                                                    />
                                                    Remove
                                                </label>
                                                {supportsCritical && (
                                                    <label className="relation-suggestion-critical">
                                                        <input
                                                            type="checkbox"
                                                            checked={criticalValue}
                                                            disabled={saving || markedToRemove}
                                                            onChange={(e) => toggleDraftCriticalUpdate(relationId, e.target.checked)}
                                                        />
                                                        Critical
                                                    </label>
                                                )}
                                            </div>
                                        </div>
                                    </div>
                                );
                            })}
                        </div>
                    )}
                </section>

                <section className="card relation-add-card">
                    <h4>Candidates</h4>

                    <div className="relation-search-wrapper">
                        <input
                            placeholder={`Find ${title.toLowerCase()}...`}
                            value={query}
                            onChange={(e) => setQuery(e.target.value)}
                        />

                        {visibleSuggestions.length > 0 && (
                            <div className="relation-suggestions" role="listbox">
                                {visibleSuggestions.map((o) => {
                                    const optionId = String(o.targetId);
                                    const isSelected = Boolean(draftAdds[optionId]);
                                    const isCriticalForRow = Boolean(draftAdds[optionId]?.isCritical);

                                    return (
                                        <div key={o.targetId} className="relation-suggestion">
                                            <div className="relation-suggestion-main">
                                                <label className="relation-row-select">
                                                    <input
                                                        type="checkbox"
                                                        checked={isSelected}
                                                        onChange={(e) => toggleCandidateSelection(o, e.target.checked)}
                                                    />
                                                    <span>{o.targetLabel}</span>
                                                </label>
                                                {supportsCritical && (
                                                    <label className="relation-suggestion-critical">
                                                        <input
                                                            type="checkbox"
                                                            checked={isCriticalForRow}
                                                            disabled={!isSelected}
                                                            onChange={(e) =>
                                                                toggleCandidateCritical(optionId, e.target.checked)
                                                            }
                                                        />
                                                        Critical
                                                    </label>
                                                )}
                                            </div>
                                        </div>
                                    );
                                })}

                                {nextCursor && (
                                    <button
                                        type="button"
                                        className="relation-load-more"
                                        onClick={() => loadCandidates(false)}
                                        disabled={loadingCandidates}
                                    >
                                        {loadingCandidates ? "Loading..." : "Load more"}
                                    </button>
                                )}
                            </div>
                        )}
                    </div>

                    {visibleSuggestions.length === 0 && (
                        <p className="relation-empty-hint">No results available or already linked.</p>
                    )}
                </section>
            </div>

            <section className="card relation-batch-card">
                <div className="relation-batch-summary">
                    <strong>Draft changes</strong>
                    <span>
                        +{pendingAddCount} additions • ~{pendingUpdateCount} updates • -{pendingRemoveCount} removals
                    </span>
                </div>
                <div className="relation-batch-actions">
                    <button type="button" onClick={resetDraft} disabled={!hasPending || saving}>
                        Discard drafts
                    </button>
                    <button
                        type="button"
                        className="primary"
                        disabled={!hasPending || saving}
                        onClick={saveDraftChanges}
                    >
                        {saving ? "Saving..." : "Save changes"}
                    </button>
                </div>
            </section>
        </div>
    );
}
