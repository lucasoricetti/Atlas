import api from "./client";

export type GraphQueryRequest = {
  SubjectType: string;
  SubjectId: string;
  Includes: string[];
  DependencyDepth?: number;
};

export type GraphNodeDto = {
  id: string;
  type: string;
  label: string;
};

export type GraphEdgeDto = {
  sourceId: string;
  sourceType: string;
  targetId: string;
  targetType: string;
  relationType: string;
  isCritical?: boolean;
  is_Critical?: boolean;
  critical?: boolean;
  IsCritical?: boolean;
};

export type GraphQueryResponse = {
  subjectType: string;
  subjectId: string;
  requestedIncludes: string[];
  nodes: GraphNodeDto[];
  edges: GraphEdgeDto[];
};

export type GraphCapabilitiesResponse = {
  subjectTypes: string[];
  includes: string[];
  maxDependencyDepth?: number;
};

export type GraphFilterCategoryName =
  | "Dependencies"
  | "Properties"
  | "Organizational"
  | "Infrastructure"
  | "General"
  | string;

export type GraphFilterCapability = {
  id: string;
  label: string;
  description: string;
  usesDependencyDepth: boolean;
  uiOrder?: number;
};

export type GraphFilterCategory = {
  category: GraphFilterCategoryName;
  categoryOrder?: number;
  filters: GraphFilterCapability[];
};

export type GraphCapabilitiesBySubjectTypeResponse = {
  subjectType: string;
  filtersByCategory: GraphFilterCategory[];
};

const LEGACY_INCLUDE_ID_MAP: Record<string, string> = {
  dependson: "outgoing_dependencies",
  dependedby: "incoming_dependencies",
  dependencychain: "dependency_chain",
  connectedservices: "related_services",
  externalsuppliers: "external_suppliers",
  usedbydivision: "division_usage",
  alldirectrelations: "all_direct_relations"
};

type RawCapabilities = {
  subjectTypes?: unknown;
  supportedSubjectTypes?: unknown;
  nodeTypes?: unknown;
  includes?: unknown;
  supportedIncludes?: unknown;
  availableIncludes?: unknown;
  maxDependencyDepth?: unknown;
};

type RawFilterCapability = {
  id?: unknown;
  label?: unknown;
  description?: unknown;
  usesDependencyDepth?: unknown;
  uiOrder?: unknown;
  UiOrder?: unknown;
  UIOrder?: unknown;
};

type RawFilterCategory = {
  category?: unknown;
  categoryOrder?: unknown;
  CategoryOrder?: unknown;
  uiOrder?: unknown;
  UiOrder?: unknown;
  UIOrder?: unknown;
  filters?: unknown;
};

type RawCapabilitiesBySubjectType = {
  subjectType?: unknown;
  filtersByCategory?: unknown;
};

const toStringArray = (value: unknown): string[] => {
  if (!Array.isArray(value)) {
    return [];
  }

  return value
    .filter((item): item is string => typeof item === "string")
    .map(item => item.trim())
    .filter(Boolean);
};

const toNumber = (value: unknown): number | undefined => {
  if (typeof value === "number" && Number.isFinite(value)) {
    return value;
  }

  if (typeof value === "string") {
    const trimmed = value.trim();
    if (!trimmed) {
      return undefined;
    }

    const parsed = Number(trimmed);
    if (Number.isFinite(parsed)) {
      return parsed;
    }
  }

  return undefined;
};

const toBoolean = (value: unknown): boolean => value === true;

const toText = (value: unknown): string => (typeof value === "string" ? value.trim() : "");

export const normalizeGraphIncludeId = (value: string): string => {
  const normalized = value.trim();
  if (!normalized) {
    return normalized;
  }

  const key = normalized.toLowerCase().replace(/[^a-z0-9]+/g, "");
  return LEGACY_INCLUDE_ID_MAP[key] ?? normalized;
};

const uniqueInsensitive = (values: string[]): string[] => {
  const seen = new Set<string>();

  return values.filter(value => {
    const key = value.toLowerCase();
    if (seen.has(key)) {
      return false;
    }

    seen.add(key);
    return true;
  });
};

const normalizeCapabilities = (raw: RawCapabilities): GraphCapabilitiesResponse => {
  const subjectTypes = uniqueInsensitive([
    ...toStringArray(raw.subjectTypes),
    ...toStringArray(raw.supportedSubjectTypes),
    ...toStringArray(raw.nodeTypes)
  ]);

  const includes = uniqueInsensitive([
    ...toStringArray(raw.includes),
    ...toStringArray(raw.supportedIncludes),
    ...toStringArray(raw.availableIncludes)
  ]).map(normalizeGraphIncludeId);

  return {
    subjectTypes,
    includes,
    maxDependencyDepth: toNumber(raw.maxDependencyDepth)
  };
};

const normalizeFilter = (raw: RawFilterCapability): GraphFilterCapability | null => {
  const id = normalizeGraphIncludeId(toText(raw.id));
  if (!id) {
    return null;
  }

  return {
    id,
    label: toText(raw.label) || id,
    description: toText(raw.description),
    usesDependencyDepth: toBoolean(raw.usesDependencyDepth),
    uiOrder: toNumber(raw.uiOrder) ?? toNumber(raw.UiOrder) ?? toNumber(raw.UIOrder)
  };
};

const normalizeFilterCategory = (raw: RawFilterCategory): GraphFilterCategory | null => {
  const category = toText(raw.category) || "General";
  const categoryOrder =
    toNumber(raw.categoryOrder) ??
    toNumber(raw.CategoryOrder) ??
    toNumber(raw.uiOrder) ??
    toNumber(raw.UiOrder) ??
    toNumber(raw.UIOrder);

  const filtersRaw = Array.isArray(raw.filters) ? raw.filters : [];
  const filters = filtersRaw
    .filter((item): item is RawFilterCapability => typeof item === "object" && item !== null)
    .map(normalizeFilter)
    .filter((item): item is GraphFilterCapability => item !== null);

  if (filters.length === 0) {
    return null;
  }

  return {
    category,
    categoryOrder,
    filters
  };
};

const normalizeCapabilitiesBySubjectType = (
  subjectType: string,
  raw: RawCapabilitiesBySubjectType
): GraphCapabilitiesBySubjectTypeResponse => {
  const filtersByCategoryRaw = Array.isArray(raw.filtersByCategory) ? raw.filtersByCategory : [];
  const filtersByCategory = filtersByCategoryRaw
    .filter((item): item is RawFilterCategory => typeof item === "object" && item !== null)
    .map(normalizeFilterCategory)
    .filter((item): item is GraphFilterCategory => item !== null);

  return {
    subjectType: toText(raw.subjectType) || subjectType,
    filtersByCategory
  };
};

export async function getGraphCapabilities() {
  const response = await api.get<RawCapabilities>("/api/graph/capabilities");
  return normalizeCapabilities(response.data);
}

export async function getGraphCapabilitiesBySubjectType(subjectType: string) {
  const response = await api.get<RawCapabilitiesBySubjectType>(`/api/graph/capabilities/${subjectType}`);
  return normalizeCapabilitiesBySubjectType(subjectType, response.data);
}

export async function queryRelationshipGraph(payload: GraphQueryRequest) {
  const response = await api.post<GraphQueryResponse>("/api/graph/query", payload);
  return response.data;
}
