/**
 * CrudPage — generic component for list + detail + create/edit/delete operations.
 *
 * Supports full customisation of fields, layout, actions and column visibility
 * through the props described below. See the individual type definitions for
 * per-field options.
 */

import { Fragment, useCallback, useEffect, useMemo, useRef, useState } from "react";
import type { ChangeEvent, FormEvent, ReactNode } from "react";
import type { LucideIcon } from "lucide-react";
import { useLocation, useNavigate } from "react-router-dom";
import api from "../api/client";
import { ENTITY_ICONS } from "../constants/entityIcons";
import EntityIcon from "./EntityIcon";

// ─────────────────────────────────────────────
//  Public types
// ─────────────────────────────────────────────

/**
 * Definition of a single form / table field.
 *
 * @property name                - Key that matches the API response object's property name.
 * @property label               - Human-readable label shown in the table header and form.
 * @property required            - Marks the field as required AND displays it in the summary
 *                                 row columns by default.
 * @property requiredInForm      - Marks the field as required only in the form (asterisk),
 *                                 without affecting default column visibility.
 * @property pinned              - When `true`, this column is ALWAYS shown in the collapsed
 *                                 summary row even if it is not in the "required" set and the
 *                                 user has not toggled it on. Useful for fields that give extra
 *                                 context at a glance (e.g. a category badge next to the name).
 * @property displayInMainSection- When `true` the field appears in the "Main information" block
 *                                 of the expanded detail panel instead of "Additional details".
 * @property numberNotText       - Treats the field value as a number: strips non-numeric input
 *                                 and serialises to a JS number in the API payload.
 * @property type                - Input type; defaults to "text".
 *                                   "textarea"    → multi-line text
 *                                   "checkbox"    → boolean toggle
 *                                   "select"      → single-value dropdown (requires optionsKey)
 *                                   "multiselect" → chip-style multi-value (requires optionsKey)
 *                                   "url"         → rendered as a clickable link in the table
 *                                   "url-array"   → list of URL inputs with add/remove
 *                                   "date"        → date picker (value sliced to YYYY-MM-DD)
 *                                   "number"      → numeric input
 * @property optionsKey          - Key into the /api/metadata/enums response used to populate
 *                                 "select" and "multiselect" option lists.
 */
export interface CrudField {
  name: string;
  label: string;
  required?: boolean;
  requiredInForm?: boolean;
  pinned?: boolean;
  displayInMainSection?: boolean;
  numberNotText?: boolean;
  type?:
    | "text"
    | "number"
    | "date"
    | "textarea"
    | "checkbox"
    | "select"
    | "multiselect"
    | "url"
    | "url-array";
  optionsKey?: string;
}

/** A resolved column entry used internally for the table header. */
export interface CrudColumn {
  key: string;
  label: string;
}

/**
 * Props for CrudPage.
 *
 * @property title        - Page heading (also used in the browser tab via the <h2>).
 * @property endpoint     - Base REST endpoint, e.g. "/api/assets". GET/POST/PUT/DELETE
 *                          requests are all derived from this value.
 * @property fields       - Ordered array of field definitions (see CrudField above).
 * @property extraActions - Render-prop for custom buttons shown inside the expanded detail
 *                          panel under "Relationship management". Receives the row item.
 * @property headerInfo   - Optional ReactNode rendered as a tooltip next to the "+ New"
 *                          button. Useful for classification hints or contextual help.
 */
interface Props {
  title: string;
  endpoint: string;
  fields: CrudField[];
  extraActions?: (item: CrudItem) => ReactNode;
  headerInfo?: ReactNode;
}

// ─────────────────────────────────────────────
//  Internal types
// ─────────────────────────────────────────────

type CrudItem = { id?: string; [key: string]: unknown };
type CrudFormValue = string | number | boolean | string[];
type CrudFormState = Record<string, CrudFormValue>;
type MetadataState = Record<string, string[]>;
type SortDirection = "asc" | "desc";
type SortState = { key: string; direction: SortDirection };
type ApiFriendlyError = {
  friendlyMessage?: string;
  response?: { data?: { errors?: Record<string, string[]> } };
};

// ─────────────────────────────────────────────
//  Static lookup tables
// ─────────────────────────────────────────────

const PAGE_ICON_BY_ENDPOINT: Record<string, LucideIcon> = {
  "/api/assets":          ENTITY_ICONS.assets,
  "/api/services":        ENTITY_ICONS.services,
  "/api/processes":       ENTITY_ICONS.processes,
  "/api/acn-macro-areas": ENTITY_ICONS.acnMacroAreas,
  "/api/divisions":       ENTITY_ICONS.divisions,
  "/api/virtual-machines":ENTITY_ICONS.virtualMachines,
  "/api/cloud-providers": ENTITY_ICONS.cloudProviders,
  "/api/contracts":       ENTITY_ICONS.contracts,
  "/api/suppliers":       ENTITY_ICONS.suppliers,
  "/api/login-types":     ENTITY_ICONS.loginTypes,
  "/api/settings":        ENTITY_ICONS.settings,
};

/** Maps API endpoint → subject type key expected by GraphExplorerPage. */
const SUBJECT_TYPE_BY_ENDPOINT: Record<string, string> = {
  "/api/assets":          "asset",
  "/api/services":        "service",
  "/api/divisions":       "division",
  "/api/virtual-machines":"virtualmachine",
  "/api/cloud-providers": "cloudprovider",
  "/api/contracts":       "contract",
  "/api/suppliers":       "supplier",
  "/api/login-types":     "logintype",
  "/api/settings":        "setting",
  "/api/processes":       "process",
  "/api/acn-macro-areas": "acnmacroarea",
};

// ─────────────────────────────────────────────
//  Pure helpers — data normalisation
// ─────────────────────────────────────────────

const toText = (value: unknown): string =>
  value === null || value === undefined ? "" : String(value);

/**
 * Extracts an array from various API envelope shapes:
 * plain array, { $values }, { items }, { results }, { data }.
 */
const normalizeCrudItems = (value: unknown): CrudItem[] => {
  if (Array.isArray(value)) return value as CrudItem[];
  if (value && typeof value === "object") {
    const r = value as Record<string, unknown>;
    for (const k of ["$values", "items", "results", "data"]) {
      if (Array.isArray(r[k])) return r[k] as CrudItem[];
    }
  }
  return [];
};

const normalizeStringArray = (value: unknown): string[] => {
  if (Array.isArray(value)) return value.map(String).filter(Boolean);
  if (value && typeof value === "object") {
    const r = value as Record<string, unknown>;
    for (const k of ["$values", "items", "results", "data"]) {
      if (Array.isArray(r[k])) return (r[k] as unknown[]).map(String).filter(Boolean);
    }
  }
  return [];
};

const normalizeMetadataState = (value: unknown): MetadataState => {
  if (!value || typeof value !== "object") return {};
  const top = value as Record<string, unknown>;
  const inner = top.data ?? top.results ?? top.items;
  const source = (inner && typeof inner === "object" ? inner : top) as Record<string, unknown>;
  return Object.fromEntries(
    Object.entries(source).map(([k, v]) => [k, normalizeStringArray(v)])
  );
};

/**
 * Resolves option list for a select/multiselect field.
 * Falls back to a case-insensitive key match when the exact key is not found.
 */
const resolveMetadataOptions = (metadata: MetadataState, optionsKey?: string): string[] => {
  if (!optionsKey) return [];
  const direct = normalizeStringArray(metadata[optionsKey]);
  if (direct.length > 0) return direct;
  const match = Object.keys(metadata).find(k => k.toLowerCase() === optionsKey.toLowerCase());
  return match ? normalizeStringArray(metadata[match]) : [];
};

// ─────────────────────────────────────────────
//  Pure helpers — session storage
// ─────────────────────────────────────────────

const readStoredStringArray = (key: string): string[] => {
  try {
    const raw = window.sessionStorage.getItem(key);
    if (!raw) return [];
    const parsed = JSON.parse(raw);
    return Array.isArray(parsed) ? parsed.map(String).filter(Boolean) : [];
  } catch {
    return [];
  }
};

const readStoredSortState = (key: string): SortState | null => {
  try {
    const raw = window.sessionStorage.getItem(key);
    if (!raw) return null;
    const parsed = JSON.parse(raw) as Partial<SortState>;
    if (typeof parsed.key !== "string") return null;
    if (parsed.direction !== "asc" && parsed.direction !== "desc") return null;
    return { key: parsed.key, direction: parsed.direction };
  } catch {
    return null;
  }
};

// ─────────────────────────────────────────────
//  Pure helpers — sorting
// ─────────────────────────────────────────────

const normalizeSortValue = (value: unknown, type?: CrudField["type"]) => {
  if (Array.isArray(value)) return value.map(String).join(", ");
  if (type === "number") return Number(value);
  if (type === "date") return new Date(String(value)).getTime();
  if (typeof value === "boolean") return value ? 1 : 0;
  return String(value).toLowerCase();
};

const compareItems = (
  a: CrudItem,
  b: CrudItem,
  key: string,
  direction: SortDirection,
  fields: CrudField[]
): number => {
  const aRaw = a[key], bRaw = b[key];
  const isEmpty = (v: unknown) =>
    v === null || v === undefined || v === "" || (Array.isArray(v) && v.length === 0);

  if (isEmpty(aRaw) && isEmpty(bRaw)) return 0;
  if (isEmpty(aRaw)) return 1;
  if (isEmpty(bRaw)) return -1;

  const fieldType = fields.find(f => f.name === key)?.type;
  const av = normalizeSortValue(aRaw, fieldType);
  const bv = normalizeSortValue(bRaw, fieldType);

  const cmp =
    typeof av === "number" && typeof bv === "number"
      ? av - bv
      : String(av).localeCompare(String(bv), undefined, { numeric: true, sensitivity: "base" });

  return direction === "asc" ? cmp : -cmp;
};

// ─────────────────────────────────────────────
//  Component
// ─────────────────────────────────────────────

export default function CrudPage({ title, endpoint, fields, extraActions, headerInfo }: Props) {
  const location = useLocation();
  const navigate = useNavigate();

  // Storage keys are scoped to the current route so each CrudPage keeps its own preferences.
  const selectedRowKey       = `crud:selected-row:${location.pathname}`;
  const visibleColsKey       = `crud:visible-summary-cols:${location.pathname}`;
  const sortStateKey         = `crud:sort-state:${location.pathname}`;

  // ── Remote data ──────────────────────────────
  const [items,    setItems]    = useState<CrudItem[]>([]);
  const [metadata, setMetadata] = useState<MetadataState>({});
  const [loading,  setLoading]  = useState(true);

  // ── User feedback ────────────────────────────
  const [error,   setError]   = useState("");
  const [success, setSuccess] = useState("");

  // ── Table UX ─────────────────────────────────
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedRow,        setSelectedRow]        = useState<string | null>(null);
  const [pendingScrollRowId, setPendingScrollRowId] = useState<string | null>(null);
  const [visibleSummaryCols, setVisibleSummaryCols] = useState<string[]>(() =>
    readStoredStringArray(visibleColsKey)
  );
  const [sortState, setSortState] = useState<SortState | null>(() =>
    readStoredSortState(sortStateKey)
  );

  // ── Modal ─────────────────────────────────────
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [selected,    setSelected]    = useState<CrudItem | null>(null);
  const [form,        setForm]        = useState<CrudFormState>({});
  const [fieldErrors, setFieldErrors] = useState<Record<string, string>>({});
  const [saving,      setSaving]      = useState(false);

  const modalRef = useRef<HTMLDivElement | null>(null);

  // ─────────────────────────────────────────────
  //  Derived / memoised values
  // ─────────────────────────────────────────────

  const pageIcon = useMemo(
    () => PAGE_ICON_BY_ENDPOINT[endpoint] ?? ENTITY_ICONS.dashboard,
    [endpoint]
  );

  const graphSubjectType = useMemo(
    () => SUBJECT_TYPE_BY_ENDPOINT[endpoint] ?? null,
    [endpoint]
  );

  /** All column keys derived from the union of all item keys + field definitions. */
  const tableColumns: CrudColumn[] = useMemo(() => {
    const keys = new Set<string>();
    if (items.length > 0) items.forEach(it => Object.keys(it).forEach(k => keys.add(k)));
    else fields.forEach(f => keys.add(f.name));
    return Array.from(keys)
      .filter(k => k.toLowerCase() !== "id")
      .map(k => ({ key: k, label: fields.find(f => f.name === k)?.label ?? k }));
  }, [items, fields]);

  const allCols = useMemo(() => tableColumns.map(c => c.key), [tableColumns]);

  /**
   * Columns shown by default: required fields first, then pinned fields.
   * If neither exists, falls back to the first 4 columns.
   */
  const defaultSummaryCols = useMemo(() => {
    const requiredCols = fields.filter(f => f.required).map(f => f.name).filter(n => allCols.includes(n));
    const pinnedCols   = fields.filter(f => f.pinned).map(f => f.name).filter(n => allCols.includes(n));
    const merged = [...new Set([...requiredCols, ...pinnedCols])];
    return merged.length > 0 ? merged : allCols.slice(0, Math.min(4, allCols.length));
  }, [allCols, fields]);

  /**
   * The actual set of visible summary columns.
   * If the user hasn't persisted a preference, defaultSummaryCols is used,
   * but pinned columns are always included regardless of the user's selection.
   */
const summaryColumns = useMemo(() => {
  const userCols = allCols.filter(c => visibleSummaryCols.includes(c));
  return userCols.length > 0 ? userCols : defaultSummaryCols;
}, [allCols, defaultSummaryCols, visibleSummaryCols]);

  /** The primary (boldest) column in a summary row — prefers name/supplierName/title. */
  const primarySummaryCol = useMemo(
    () => summaryColumns.find(c => ["name", "supplierName", "title"].includes(c)) ?? summaryColumns[0],
    [summaryColumns]
  );

  const descriptionCol = useMemo(
    () => allCols.find(c => c.toLowerCase() === "description"),
    [allCols]
  );

  /** The field used as the primary search target. */
  const searchableField = useMemo(() => {
    const preferred = ["name", "supplierName", "title"];
    return fields.find(f => preferred.includes(f.name)) ?? fields[0];
  }, [fields]);

  const requiredCols = useMemo(
    () => fields.filter(f => f.required).map(f => f.name).filter(n => allCols.includes(n)),
    [allCols, fields]
  );

  const emptyForm = useMemo(() => {
    const o: CrudFormState = {};
    fields.forEach(f => {
      if (f.type === "checkbox") o[f.name] = false;
      else if (f.type === "multiselect") o[f.name] = [];
      else o[f.name] = "";
    });
    return o;
  }, [fields]);

  const filteredItems = useMemo(() => {
    const term = searchTerm.trim().toLowerCase();
    if (!term || !searchableField) return items;
    return items.filter(item => {
      const v = item?.[searchableField.name];
      return v !== null && v !== undefined && String(v).toLowerCase().includes(term);
    });
  }, [items, searchTerm, searchableField]);

  const sortedItems = useMemo(() => {
    if (!sortState) return filteredItems;
    return [...filteredItems].sort((a, b) =>
      compareItems(a, b, sortState.key, sortState.direction, fields)
    );
  }, [filteredItems, sortState, fields]);

  // ─────────────────────────────────────────────
  //  Data loading
  // ─────────────────────────────────────────────

  const load = useCallback(async () => {
    setLoading(true);
    setError("");
    try {
      const [list, enums] = await Promise.all([
        api.get(endpoint),
        api.get("/api/metadata/enums"),
      ]);
      setItems(normalizeCrudItems(list.data));
      setMetadata(normalizeMetadataState(enums.data));
    } catch (err: unknown) {
      setError((err as ApiFriendlyError)?.friendlyMessage ?? "Error while loading.");
    } finally {
      setLoading(false);
    }
  }, [endpoint]);

  useEffect(() => { void load(); }, [load]);

  // ─────────────────────────────────────────────
  //  Session storage — selected row persistence
  // ─────────────────────────────────────────────

  useEffect(() => {
    const stored = window.sessionStorage.getItem(selectedRowKey);
    if (!stored) return;
    setSelectedRow(stored);
    setPendingScrollRowId(stored);
  }, [selectedRowKey]);

  useEffect(() => {
    if (!selectedRow) { window.sessionStorage.removeItem(selectedRowKey); return; }
    window.sessionStorage.setItem(selectedRowKey, selectedRow);
  }, [selectedRow, selectedRowKey]);

  // ─────────────────────────────────────────────
  //  Session storage — column + sort persistence
  // ─────────────────────────────────────────────

  useEffect(() => {
    if (allCols.length === 0) return;
    const storedCols  = readStoredStringArray(visibleColsKey).filter(c => allCols.includes(c));
    const nextCols    = storedCols.length > 0 ? storedCols : defaultSummaryCols;
    setVisibleSummaryCols(prev =>
      nextCols.length === prev.length && nextCols.every((c, i) => c === prev[i]) ? prev : nextCols
    );

    const storedSort = readStoredSortState(sortStateKey);
    const nextSort   = storedSort && allCols.includes(storedSort.key) ? storedSort : null;
    setSortState(prev => {
      if (!nextSort && !prev) return prev;
      if (nextSort && prev?.key === nextSort.key && prev.direction === nextSort.direction) return prev;
      return nextSort;
    });
  }, [allCols, defaultSummaryCols, sortStateKey, visibleColsKey]);

  useEffect(() => {
    if (visibleSummaryCols.length === 0) { window.sessionStorage.removeItem(visibleColsKey); return; }
    window.sessionStorage.setItem(visibleColsKey, JSON.stringify(visibleSummaryCols));
  }, [visibleSummaryCols, visibleColsKey]);

  useEffect(() => {
    if (!sortState) { window.sessionStorage.removeItem(sortStateKey); return; }
    window.sessionStorage.setItem(sortStateKey, JSON.stringify(sortState));
  }, [sortState, sortStateKey]);

  // ─────────────────────────────────────────────
  //  Auto-scroll to newly created / restored row
  // ─────────────────────────────────────────────

  useEffect(() => {
    if (loading || !selectedRow) return;
    if (!filteredItems.some(it => String(it.id) === selectedRow)) setSelectedRow(null);
  }, [filteredItems, loading, selectedRow]);

  useEffect(() => {
    if (!pendingScrollRowId) return;
    if (!filteredItems.some(it => String(it.id) === pendingScrollRowId)) return;
    const raf = window.requestAnimationFrame(() => {
      const safeId = window.CSS?.escape ? window.CSS.escape(pendingScrollRowId) : pendingScrollRowId;
      document.querySelector<HTMLTableRowElement>(`tr[data-row-id="${safeId}"]`)
        ?.scrollIntoView({ behavior: "smooth", block: "center" });
      setPendingScrollRowId(null);
    });
    return () => window.cancelAnimationFrame(raf);
  }, [filteredItems, pendingScrollRowId]);

  // ─────────────────────────────────────────────
  //  Modal lifecycle
  // ─────────────────────────────────────────────

  useEffect(() => {
    if (!isModalOpen) return;
    const prev = document.body.style.overflow;
    document.body.style.overflow = "hidden";
    const timer = window.setTimeout(() => {
      modalRef.current
        ?.querySelector<HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement | HTMLButtonElement>(
          "input, select, textarea, button"
        )
        ?.focus();
    }, 0);
    const onKey = (e: KeyboardEvent) => { if (e.key === "Escape") closeModal(); };
    window.addEventListener("keydown", onKey);
    return () => {
      document.body.style.overflow = prev;
      window.removeEventListener("keydown", onKey);
      window.clearTimeout(timer);
    };
  }, [isModalOpen]);

  // ─────────────────────────────────────────────
  //  CRUD actions
  // ─────────────────────────────────────────────

  function startCreate() {
    setSelected(null);
    setForm(emptyForm);
    setError(""); setSuccess(""); setFieldErrors({});
    setIsModalOpen(true);
  }

  function startEdit(item: CrudItem) {
    const f: CrudFormState = {};
    fields.forEach(field => {
      const raw = item[field.name];
      if (field.type === "checkbox")    f[field.name] = Boolean(raw);
      else if (field.type === "multiselect") f[field.name] = Array.isArray(raw) ? raw.map(String) : [];
      else if (field.type === "url-array")   f[field.name] = Array.isArray(raw) ? raw.map(String) : [""];
      else { const t = toText(raw); f[field.name] = field.type === "date" ? t.slice(0, 10) : t; }
    });
    setSelected(item);
    setForm(f);
    setFieldErrors({});
    setIsModalOpen(true);
  }

  function closeModal() {
    setIsModalOpen(false);
    setSelected(null);
    setError("");
    setFieldErrors({});
  }

  /** Strips empty / invalid values before sending to the API. */
  function buildPayload(raw: CrudFormState): CrudFormState {
    const out: CrudFormState = {};
    Object.entries(raw).forEach(([key, value]) => {
      const field = fields.find(f => f.name === key);
      if (field?.numberNotText) {
        if (value === "" || value === null || value === undefined) return;
        const n = Number(value);
        if (!isNaN(n)) out[key] = n;
        return;
      }
      if (value !== "" && value !== null && value !== undefined &&
          !(Array.isArray(value) && value.length === 0)) {
        out[key] = value;
      }
    });
    return out;
  }

  function formatApiErrors(err: ApiFriendlyError): string {
    const errs = err?.response?.data?.errors;
    if (!errs) return err?.friendlyMessage ?? "An unknown error occurred.";
    const lines = Object.entries(errs).flatMap(([field, msgs]) => {
      const pretty = field.replace(/([A-Z])/g, " $1").trim();
      return msgs.map(m => `• ${pretty}: ${m}`);
    });
    return `⚠️ There are fields to correct:\n${lines.join("\n")}`;
  }

  /** Tries to derive the id of the newly created item without a separate GET. */
  const resolveCreatedId = useCallback(async (
    createResponseData: unknown,
    previousItems: CrudItem[]
  ): Promise<string> => {
    if (typeof createResponseData === "object" && createResponseData !== null) {
      const maybeId = (createResponseData as CrudItem).id;
      if (maybeId !== null && maybeId !== undefined && maybeId !== "") return String(maybeId);
    }
    const existingIds = new Set(previousItems.map(it => String(it.id ?? "")).filter(Boolean));
    const latest = await api.get(endpoint);
    const latestItems = normalizeCrudItems(latest.data);
    const candidates = latestItems.map(it => String(it.id ?? "")).filter(id => id && !existingIds.has(id));
    return candidates.length === 1 ? candidates[0] : "";
  }, [endpoint]);

  async function save(e: FormEvent<HTMLFormElement>) {
    e.preventDefault();
    setSaving(true);
    setError(""); setSuccess("");
    try {
      const payload = buildPayload(form);
      if (selected?.id) {
        await api.put(`${endpoint}/${selected.id}`, payload);
        setSuccess("Updated successfully.");
        await load();
        setSearchTerm("");
        const updatedId = String(selected.id);
        setSelectedRow(updatedId);
        setPendingScrollRowId(updatedId);
      } else {
        const itemsBefore = [...items];
        const res = await api.post(endpoint, payload);
        const createdId = await resolveCreatedId(res.data, itemsBefore);
        setSuccess("Created successfully.");
        await load();
        setSearchTerm("");
        if (createdId) { setSelectedRow(createdId); setPendingScrollRowId(createdId); }
      }
      closeModal();
    } catch (err: unknown) {
      setError(formatApiErrors(err as ApiFriendlyError));
    } finally {
      setSaving(false);
    }
  }

  async function removeItem(id: string) {
    if (!confirm("Are you sure?")) return;
    try {
      await api.delete(`${endpoint}/${id}`);
      setSuccess("Item deleted.");
      await load();
    } catch (err: unknown) {
      setError((err as ApiFriendlyError)?.friendlyMessage ?? "Error while deleting.");
    }
  }

  function openInGraphExplorer(item: CrudItem) {
    if (!graphSubjectType) return;
    const itemId = String(item.id ?? "");
    if (!itemId) return;
    window.sessionStorage.setItem(
      "graph-explorer:state:/graph-explorer",
      JSON.stringify({ subjectType: graphSubjectType, subjectId: itemId })
    );
    navigate("/graph-explorer");
  }

  // ─────────────────────────────────────────────
  //  Table UX helpers
  // ─────────────────────────────────────────────

  const isExpanded = (id: string) => selectedRow === id;
  const toggleRow  = (id: string) => setSelectedRow(p => p === id ? null : id);
  const getLabel   = (key: string) => fields.find(f => f.name === key)?.label ?? key;
  const getTitle   = (item: CrudItem) =>
    toText(item.name) || toText(item.supplierName) || toText(item.title) || "Item";
  const getTitleKey = (item: CrudItem) => {
    if (item?.name !== undefined) return "name";
    if (item?.supplierName !== undefined) return "supplierName";
    if (item?.title !== undefined) return "title";
    return undefined;
  };

  function toggleSummaryColumn(colKey: string) {
    setVisibleSummaryCols(prev => {
      if (prev.includes(colKey)) {
        return prev.length === 1 ? prev : prev.filter(k => k !== colKey);
      }
      return [...prev, colKey];
    });
  }

  function toggleSort(key: string) {
    setSortState(prev => {
      if (!prev || prev.key !== key) return { key, direction: "asc" };
      return { key, direction: prev.direction === "asc" ? "desc" : "asc" };
    });
  }

  // ─────────────────────────────────────────────
  //  Cell rendering
  // ─────────────────────────────────────────────

  function renderCell(item: CrudItem, colKey: string) {
    const val   = item[colKey];
    const field = fields.find(f => f.name === colKey);

    if (field?.type === "checkbox") {
      return val
        ? <span className="badge badge-success">✓ Yes</span>
        : <span className="badge badge-error">✗ No</span>;
    }

    if (Array.isArray(val)) {
      return (
        <div style={{ display: "flex", gap: "0.25rem", flexWrap: "wrap" }}>
          {(val as unknown[]).map((v, i) => (
            <span key={i} className="badge badge-info">{String(v)}</span>
          ))}
        </div>
      );
    }

    if (val === null || val === undefined || val === "") {
      const showNA = ["rpoH", "mtoH", "sla"].includes(colKey);
      return (
        <span style={{ color: "var(--text-secondary)", fontStyle: "italic" }}>
          {showNA ? "n/a" : "-"}
        </span>
      );
    }

    if (field?.type === "url") {
      const urlStr = String(val);
      const href = urlStr.startsWith("http://") || urlStr.startsWith("https://")
        ? urlStr : `https://${urlStr}`;
      return (
        <a href={href} target="_blank" rel="noopener noreferrer"
           style={{ color: "var(--primary-color)", textDecoration: "underline", cursor: "pointer" }}>
          {urlStr}
        </a>
      );
    }

    if (field?.type === "textarea") {
      return <span style={{ whiteSpace: "pre-wrap", wordBreak: "break-word" }}>{String(val)}</span>;
    }

    return <span>{String(val)}</span>;
  }

  // ─────────────────────────────────────────────
  //  Form field rendering
  // ─────────────────────────────────────────────

  function updateField(
    e: ChangeEvent<HTMLInputElement | HTMLTextAreaElement | HTMLSelectElement>,
    field: CrudField
  ) {
    const { name, value } = e.target;

    if (field.type === "checkbox") {
      setForm(p => ({ ...p, [name]: (e.target as HTMLInputElement).checked }));
      setFieldErrors(p => ({ ...p, [name]: "" }));
      return;
    }

    if (field.type === "multiselect") {
      const arr = Array.from((e.target as HTMLSelectElement).options)
        .filter(o => o.selected).map(o => o.value);
      setForm(p => ({ ...p, [name]: arr }));
      setFieldErrors(p => ({ ...p, [name]: "" }));
      return;
    }

    let fieldError = "";
    if (field.numberNotText && value !== "") {
      if (isNaN(Number(value))) fieldError = "This field must be a number";
    }

    setForm(p => ({ ...p, [name]: e.target.type === "number" ? Number(value) : value }));
    setFieldErrors(p => ({ ...p, [name]: fieldError }));
  }

  function renderFormField(f: CrudField) {
    const v       = form[f.name] ?? "";
    const options = resolveMetadataOptions(metadata, f.optionsKey);
    const text    = typeof v === "string" || typeof v === "number" ? v : "";
    const hasErr  = Boolean(fieldErrors[f.name]);
    const errStyle = hasErr ? { borderColor: "var(--error-color, #dc3545)" } : {};

    switch (f.type) {
      case "textarea":
        return <textarea name={f.name} value={text} onChange={e => updateField(e, f)} style={errStyle} />;

      case "url-array": {
        const urls: string[] = Array.isArray(v) ? (v as string[]) : [""];
        return (
          <div>
            {urls.map((url, i) => (
              <div key={i} style={{ display: "flex", gap: 8, marginBottom: 4 }}>
                <input
                  type="url" value={url}
                  onChange={e => {
                    const next = [...urls]; next[i] = e.target.value;
                    setForm(p => ({ ...p, [f.name]: next }));
                  }}
                />
                <button type="button" disabled={urls.length === 1}
                  onClick={() => setForm(p => ({ ...p, [f.name]: urls.filter((_, idx) => idx !== i) }))}>
                  ×
                </button>
              </div>
            ))}
            <button type="button" onClick={() => setForm(p => ({ ...p, [f.name]: [...urls, ""] }))}>
              + Add Link
            </button>
          </div>
        );
      }

      case "checkbox":
        return (
          <input type="checkbox" name={f.name} checked={Boolean(v)}
            onChange={e => updateField(e, f)} />
        );

      case "select":
        return (
          <select name={f.name} value={text} onChange={e => updateField(e, f)}>
            <option value="">-- select --</option>
            {options.map(o => <option key={o}>{o}</option>)}
          </select>
        );

      case "multiselect": {
        const selected = Array.isArray(v) ? v as string[] : [];
        return (
          <div style={{ display: "flex", gap: "0.5rem", flexWrap: "wrap", marginTop: 4 }}>
            {options.map(o => (
              <label key={o} style={{
                display: "flex", alignItems: "center", gap: "0.4rem",
                background: "var(--bg-color)", padding: "0.4rem 0.75rem",
                borderRadius: "1rem", border: "1px solid var(--border-color)",
              }}>
                <input type="checkbox" checked={selected.includes(o)}
                  onChange={e => {
                    const next = e.target.checked
                      ? [...selected, o]
                      : selected.filter(x => x !== o);
                    setForm(p => ({ ...p, [f.name]: next }));
                  }}
                />
                <span>{o}</span>
              </label>
            ))}
          </div>
        );
      }

      default:
        return (
          <input type={f.type ?? "text"} name={f.name} value={text}
            onChange={e => updateField(e, f)} style={errStyle} />
        );
    }
  }

  // ─────────────────────────────────────────────
  //  Render
  // ─────────────────────────────────────────────

  return (
    <div className="page">
      {/* ── Page header ── */}
      <div className="page-header">
        <h2 className="page-title-with-icon">
          <EntityIcon icon={pageIcon} size={24} className="page-title-icon" />
          <span>{title}</span>
        </h2>

        <div className="page-header-actions">
          {headerInfo && (
            <div className="crud-header-info" role="group" aria-label="Additional classification information">
              <button type="button" className="crud-header-info-trigger"
                aria-label="Show additional NIS2 classification information">i</button>
              <div className="crud-header-info-tooltip" role="note">{headerInfo}</div>
            </div>
          )}
          <button className="primary" onClick={startCreate}>+ New</button>
        </div>
      </div>

      {error   && <div className="alert alert-error">{error}</div>}
      {success && <div className="alert alert-success">{success}</div>}

      {/* ── List stack ── */}
      <div className="crud-list-stack">
        {/* Search */}
        <div className="crud-search-row">
          <input
            type="text"
            value={searchTerm}
            onChange={e => setSearchTerm(e.target.value)}
            placeholder={`Search by ${(searchableField?.label ?? "name").toLowerCase()}…`}
            aria-label={`Search by ${searchableField?.label ?? "name"}`}
          />
        </div>

        {/* Column picker — pinned columns show as disabled (always visible) */}
        <div className="crud-column-picker" role="group" aria-label="Choose visible columns in compact view">
          <span className="crud-column-picker-title">Visible columns</span>
          <div className="crud-column-picker-options">
            {allCols.map(col => {
              const checked  = summaryColumns.includes(col);
              // A column is disabled if it's the last user-chosen one OR if it's pinned.
              const disabled = checked && summaryColumns.length === 1;
              return (
                <label key={col} className="crud-column-picker-option"
                       title={undefined}>
                  <input type="checkbox" checked={checked} disabled={disabled}
                    onChange={() => toggleSummaryColumn(col)} />
                  <span>{getLabel(col)}{""}</span>
                </label>
              );
            })}
          </div>
        </div>

        {/* Table */}
        <section className="card crud-table-card">
          {loading ? (
            <p style={{ color: "var(--text-secondary)" }}>Loading…</p>
          ) : (
            <table className="crud-table">
              <thead>
                <tr>
                  {summaryColumns.map(c => (
                    <th key={c} className="crud-sortable-header" onClick={() => toggleSort(c)}
                      aria-sort={sortState?.key === c ? (sortState.direction === "asc" ? "ascending" : "descending") : "none"}>
                      <span>{getLabel(c)}</span>
                      <span className="crud-sort-indicator" aria-hidden="true">
                        {sortState?.key === c ? (sortState.direction === "asc" ? "▲" : "▼") : "↕"}
                      </span>
                    </th>
                  ))}
                  <th />
                </tr>
              </thead>

              <tbody>
                {sortedItems.map(it => {
                  const rowId      = String(it.id ?? "");
                  const open       = isExpanded(rowId);
                  const titleKey   = getTitleKey(it);

                  // "Main information" block: required or explicitly flagged fields (excluding title & description).
                  const mainCols = allCols
                    .filter(col =>
                      (requiredCols.includes(col) || fields.find(f => f.name === col)?.displayInMainSection) &&
                      col !== titleKey && col !== descriptionCol
                    )
                    .sort((a, b) =>
                      fields.findIndex(f => f.name === a) - fields.findIndex(f => f.name === b)
                    );

                  // "Additional details" block: everything else.
                  const extraCols = allCols.filter(col =>
                    !requiredCols.includes(col) &&
                    !fields.find(f => f.name === col)?.displayInMainSection &&
                    col !== descriptionCol && col !== titleKey
                  );

                  return (
                    <Fragment key={it.id}>
                      {/* Collapsed summary row */}
                      <tr
                        data-row-id={rowId}
                        onClick={() => toggleRow(rowId)}
                        onKeyDown={e => { if (e.key === "Enter" || e.key === " ") { e.preventDefault(); toggleRow(rowId); } }}
                        tabIndex={0} aria-expanded={open}
                        className={`crud-summary-row ${open ? "is-open" : ""}`}
                      >
                        {summaryColumns.map(col => (
                          <td key={col} className={col === primarySummaryCol ? "crud-summary-primary" : ""}>
                            {renderCell(it, col)}
                          </td>
                        ))}
                        <td className="crud-row-toggle-cell" aria-hidden="true">
                          {open ? "▲" : "▼"}
                        </td>
                      </tr>

                      {/* Expanded detail panel */}
                      {open && (
                        <tr className="details-row">
                          <td colSpan={summaryColumns.length + 1}>
                            <div className="crud-detail-panel">
                              {/* Header */}
                              <div className="crud-detail-header">
                                <h3>{getTitle(it)}</h3>
                                <div className="crud-detail-actions crud-detail-actions-top">
                                  <button onClick={() => startEdit(it)} className="crud-action-btn crud-action-edit">Edit</button>
                                  <button onClick={() => removeItem(rowId)} className="crud-action-btn crud-action-delete">Delete</button>
                                  {graphSubjectType && (
                                    <button onClick={() => openInGraphExplorer(it)}
                                      className="crud-action-btn crud-action-explore"
                                      title="Explore in Graph Explorer" aria-label="Explore in Graph Explorer">
                                      <EntityIcon icon={ENTITY_ICONS.graphExplorer} size={14} />
                                    </button>
                                  )}
                                </div>
                              </div>

                              {/* Description */}
                              {descriptionCol && Boolean(toText(it[descriptionCol])) && (
                                <div className="crud-detail-description">
                                  <div className="crud-detail-description-label">Description</div>
                                  <p style={{ whiteSpace: "pre-wrap", wordBreak: "break-word" }}>
                                    {toText(it[descriptionCol])}
                                  </p>
                                </div>
                              )}

                              {/* Main information */}
                              {mainCols.length > 0 && (
                                <div className="crud-detail-section">
                                  <div className="crud-detail-section-title">Main information</div>
                                  <div className="crud-detail-grid">
                                    {mainCols.map(col => (
                                      <div key={col} className="crud-detail-item">
                                        <div className="crud-detail-label">{getLabel(col)}</div>
                                        <div className="crud-detail-value">{renderCell(it, col)}</div>
                                      </div>
                                    ))}
                                  </div>
                                </div>
                              )}

                              {/* Additional details */}
                              {extraCols.length > 0 && (
                                <div className="crud-detail-section">
                                  <div className="crud-detail-section-title">Additional details</div>
                                  <div className="crud-detail-grid">
                                    {extraCols.map(col => (
                                      <div key={col} className="crud-detail-item">
                                        <div className="crud-detail-label">{getLabel(col)}</div>
                                        <div className="crud-detail-value">{renderCell(it, col)}</div>
                                      </div>
                                    ))}
                                  </div>
                                </div>
                              )}

                              {/* Custom relationship actions */}
                              {extraActions && (
                                <div className="crud-detail-section">
                                  <div className="crud-detail-section-title">Relationship management</div>
                                  <div className="crud-detail-relations">{extraActions(it)}</div>
                                </div>
                              )}
                            </div>
                          </td>
                        </tr>
                      )}
                    </Fragment>
                  );
                })}

                {sortedItems.length === 0 && (
                  <tr>
                    <td colSpan={summaryColumns.length + 1}>
                      <span style={{ color: "var(--text-secondary)", fontStyle: "italic" }}>No results found.</span>
                    </td>
                  </tr>
                )}
              </tbody>
            </table>
          )}
        </section>
      </div>

      {/* ── Create / edit modal ── */}
      {isModalOpen && (
        <div className="modal-overlay" onClick={closeModal} aria-hidden="true">
          <div className="modal-content" onClick={e => e.stopPropagation()}
            ref={modalRef} role="dialog" aria-modal="true">
            <button className="modal-close" onClick={closeModal}>×</button>
            <h3 style={{ marginBottom: "1rem" }}>{selected ? "Edit item" : "New item"}</h3>

            {error && (
              <div className="alert alert-error" style={{ marginBottom: 12 }}>{error}</div>
            )}

            <form onSubmit={save} className="form-grid">
              {fields.map(f => {
                const isReq = f.requiredInForm ?? f.required;
                return (
                  <div key={f.name} className="form-field">
                    <label>
                      {f.label}
                      {isReq && <span className="required"> *</span>}
                    </label>
                    {renderFormField(f)}
                    {fieldErrors[f.name] && (
                      <div style={{ color: "var(--error-color, #dc3545)", fontSize: "0.875rem", marginTop: "0.25rem" }}>
                        ⚠️ {fieldErrors[f.name]}
                      </div>
                    )}
                  </div>
                );
              })}

              <div className="form-actions">
                <button type="button" onClick={closeModal}>Cancel</button>
                <button type="submit" className="primary" disabled={saving}>
                  {saving ? "Saving…" : selected ? "Update" : "Create"}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}