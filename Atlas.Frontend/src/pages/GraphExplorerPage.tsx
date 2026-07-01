import { useCallback, useEffect, useMemo, useRef, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import type { CSSProperties, FormEvent } from "react";
import api from "../api/client";
import {
  getGraphCapabilities,
  getGraphCapabilitiesBySubjectType,
  normalizeGraphIncludeId,
  queryRelationshipGraph,
  type GraphCapabilitiesBySubjectTypeResponse,
  type GraphCapabilitiesResponse,
  type GraphFilterCategory,
  type GraphQueryResponse,
} from "../api/graph";
import { ENTITY_ICONS } from "../constants/entityIcons";
import EntityIcon from "../components/EntityIcon";

// ─── Constants ───────────────────────────────────────────────────────────────

const DEFAULT_SUBJECT_TYPES = ["asset", "service", "division"];
const EMPTY_FILTER_CATEGORIES: GraphFilterCategory[] = [];

const ENTITY_TYPE_ORDER: Record<string, number> = {
  asset: 0,
  division: 1,
  logintype: 2,
  setting: 3,
  service: 4,
  contract: 5,
  supplier: 6,
  virtualmachine: 7,
  cloudprovider: 8,
  process: 9,
  acnmacroarea: 10,
};

const CRUD_ROUTE_MAP: Record<string, string> = {
  asset: "/assets",
  service: "/services",
  division: "/divisions",
  contract: "/contracts",
  supplier: "/suppliers",
  setting: "/settings",
  logintype: "/login-types",
  cloudprovider: "/cloud-providers",
  virtualmachine: "/virtual-machines",
  process: "/processes",
  acnmacroarea: "/acn-macro-areas",
};

const ENTITY_TYPE_ALIAS_MAP: Record<string, string> = {
  asset: "ASS",
  service: "SER",
  division: "DIV",
  contract: "CON",
  supplier: "SUP",
  setting: "SET",
  logintype: "LOG",
  cloudprovider: "CP",
  virtualmachine: "VM",
  host: "VM",
  process: "PRO",
  acnmacroarea: "ACN",
};

const ENTITY_DISPLAY_NAME_MAP: Record<string, string> = {
  asset: "Asset",
  service: "Service",
  division: "Division",
  contract: "Contract",
  supplier: "Supplier",
  setting: "Setting",
  logintype: "Login Type",
  cloudprovider: "Cloud Provider",
  virtualmachine: "Virtual Machine",
  process: "Process",
  acnmacroarea: "ACN Macro Area",
};

const SUBJECT_SOURCES: Record<string, SubjectSource> = {
  asset: {
    endpoint: "/api/assets",
    primaryFields: ["name"],
    secondaryFields: ["type", "criticality"],
  },
  service: {
    endpoint: "/api/services",
    primaryFields: ["name"],
    secondaryFields: ["category", "version", "env", "status"],
  },
  division: {
    endpoint: "/api/divisions",
    primaryFields: ["name"],
    secondaryFields: [],
  },
  contract: {
    endpoint: "/api/contracts",
    primaryFields: ["name", "title"],
    secondaryFields: ["sla", "startDate", "endDate", "contactEmail"],
  },
  supplier: {
    endpoint: "/api/suppliers",
    primaryFields: ["name"],
    secondaryFields: [],
  },
  setting: {
    endpoint: "/api/settings",
    primaryFields: ["name", "title"],
    secondaryFields: ["link", "description"],
  },
  logintype: {
    endpoint: "/api/login-types",
    primaryFields: ["name"],
    secondaryFields: ["protocol", "mfa"],
  },
  cloudprovider: {
    endpoint: "/api/cloud-providers",
    primaryFields: ["name"],
    secondaryFields: ["type", "account"],
  },
  virtualmachine: {
    endpoint: "/api/virtual-machines",
    primaryFields: ["name"],
    secondaryFields: ["ip", "cluster", "role"],
  },
  process: {
    endpoint: "/api/processes",
    primaryFields: ["name"],
    secondaryFields: ["description"],
  },
  acnmacroarea: {
    endpoint: "/api/acn-macro-areas",
    primaryFields: ["name"],
    secondaryFields: ["preAssignedAcnCategory", "customAcnCategory"],
  },
};

// ─── Types ────────────────────────────────────────────────────────────────────

type SubjectSource = {
  endpoint: string;
  primaryFields: string[];
  secondaryFields: string[];
};

type SubjectOption = {
  id: string;
  type: string;
  primary: string;
  secondary: string;
  display: string;
  searchable: string;
};

type DisplayNode = {
  id: string;
  type: string;
  label: string;
};

type DisplayEdge = {
  sourceId: string;
  sourceType: string;
  targetId: string;
  targetType: string;
  relationType: string;
  isCritical: boolean;
};

type NextSubjectCandidate = DisplayNode & {
  matchingSubjectType: string | null;
  isSelectable: boolean;
};

type DisplayFilterCategory = {
  key: string;
  category: string;
  categoryOrder?: number;
  filters: GraphFilterCategory["filters"];
  sourceCategories: Array<{
    key: string;
    category: string;
    filters: GraphFilterCategory["filters"];
  }>;
};

type GraphExplorerStoredState = {
  subjectType?: string;
  subjectId?: string;
  includes?: string[];
  dependencyDepth?: number;
  expandedCategoryKeys?: string[];
};

type GraphNodeGroup =
  | "mutualDependencies"
  | "supportingAssets"
  | "dependentAssets"
  | "supportingServices"
  | "dependentServices"
  | "hosts"
  | "loginTypes"
  | "settings"
  | "supplierContracts"
  | "externalSuppliers"
  | "divisions"
  | "processes"
  | "acnMacroAreas"
  | "otherRelated";

type ChainSide = "supporting" | "dependent";

type NodeAssignment = {
  group: GraphNodeGroup;
  chainSide?: ChainSide;
};

type EntityPalette = {
  nodeFill: string;
  nodeStroke: string;
  nodeText: string;
  groupFill: string;
  groupStroke: string;
  chipFill: string;
  chipFillHover: string;
  chipStroke: string;
  chipText: string;
  chipMeta: string;
};

type NextSubjectChipStyle = CSSProperties & {
  "--graph-node-chip-fill": string;
  "--graph-node-chip-fill-hover": string;
  "--graph-node-chip-stroke": string;
  "--graph-node-chip-text": string;
  "--graph-node-chip-meta": string;
};

// ─── Declarative Edge Rules ───────────────────────────────────────────────────

type ChainSideAction = { type: "chainSide"; side: ChainSide };
type TerminalGroupAction = { type: "terminalGroup"; group: GraphNodeGroup };
type EdgeAction = ChainSideAction | TerminalGroupAction;

type SubjectEdgeRule = {
  relations: string[];
  nodeTypes: string[];
  action: EdgeAction;
};

/** Rules when the subject is the SOURCE of an edge */
const SUBJECT_AS_SOURCE_RULES: SubjectEdgeRule[] = [
  {
    relations: ["DEPENDS_ON", "INVOLVES"],
    nodeTypes: ["asset", "service"],
    action: { type: "chainSide", side: "supporting" },
  },
  {
    relations: ["COMPOSED_BY", "HOSTS"],
    nodeTypes: ["service"],
    action: { type: "chainSide", side: "supporting" },
  },
  {
    relations: ["OWNS", "USES"],
    nodeTypes: ["asset"],
    action: { type: "chainSide", side: "supporting" },
  },
  {
    relations: ["OWNS", "USES"],
    nodeTypes: ["process"],
    action: { type: "terminalGroup", group: "processes" },
  },
  {
    relations: ["INVOLVES"],
    nodeTypes: ["process"],
    action: { type: "terminalGroup", group: "processes" },
  },
  {
    relations: ["HAS_CONTRACT"],
    nodeTypes: ["contract"],
    action: { type: "terminalGroup", group: "supplierContracts" },
  },
  {
    relations: ["PROVIDED_BY"],
    nodeTypes: ["supplier"],
    action: { type: "terminalGroup", group: "externalSuppliers" },
  },
  {
    relations: ["HAS_LOGIN_TYPE"],
    nodeTypes: ["logintype"],
    action: { type: "terminalGroup", group: "loginTypes" },
  },
  {
    relations: ["HAS_SETTING"],
    nodeTypes: ["setting"],
    action: { type: "terminalGroup", group: "settings" },
  },
  {
    relations: ["CLASSIFIED_AS"],
    nodeTypes: ["acnmacroarea"],
    action: { type: "terminalGroup", group: "acnMacroAreas" },
  },
];

/** Rules when the subject is the TARGET of an edge */
const SUBJECT_AS_TARGET_RULES: SubjectEdgeRule[] = [
  {
    relations: ["DEPENDS_ON"],
    nodeTypes: ["asset", "service"],
    action: { type: "chainSide", side: "dependent" },
  },
  {
    relations: ["COMPOSED_BY"],
    nodeTypes: ["asset"],
    action: { type: "chainSide", side: "dependent" },
  },
  {
    relations: ["HOSTS"],
    nodeTypes: ["virtualmachine", "cloudprovider", "host"],
    action: { type: "terminalGroup", group: "hosts" },
  },
  {
    relations: ["OWNS", "USES"],
    nodeTypes: ["division"],
    action: { type: "terminalGroup", group: "divisions" },
  },
  {
    relations: ["HAS_CONTRACT"],
    nodeTypes: ["service"],
    action: { type: "chainSide", side: "supporting" },
  },
  {
    relations: ["PROVIDED_BY"],
    nodeTypes: ["contract"],
    action: { type: "terminalGroup", group: "supplierContracts" },
  },
  {
    relations: ["HAS_LOGIN_TYPE"],
    nodeTypes: ["asset"],
    action: { type: "chainSide", side: "supporting" },
  },
  {
    relations: ["HAS_SETTING"],
    nodeTypes: ["asset", "service"],
    action: { type: "chainSide", side: "supporting" },
  },
  {
    relations: ["HAS_SETTING", "INVOLVES", "CLASSIFIED_AS"],
    nodeTypes: ["process"],
    action: { type: "terminalGroup", group: "processes" },
  },
  {
    relations: ["INVOLVES"],
    nodeTypes: ["process"],
    action: { type: "terminalGroup", group: "processes" },
  },
];

/** Rules for BFS propagation from an already-assigned node to its neighbors */
type PropagationRule = {
  fromGroups: GraphNodeGroup[];
  fromChainSides?: ChainSide[];
  relations: string[];
  nodeTypes: string[];
  action:
    | { type: "terminalGroup"; group: GraphNodeGroup }
    | { type: "inheritChain" }
    | { type: "composedByService" }
    | { type: "composedByAsset" };
};

const PROPAGATION_RULES: PropagationRule[] = [
  // supplier chain
  {
    fromGroups: ["supplierContracts"],
    relations: ["PROVIDED_BY"],
    nodeTypes: ["supplier"],
    action: { type: "terminalGroup", group: "externalSuppliers" },
  },
  // process → acn
  {
    fromGroups: ["processes"],
    relations: ["CLASSIFIED_AS"],
    nodeTypes: ["acnmacroarea"],
    action: { type: "terminalGroup", group: "acnMacroAreas" },
  },
  // process → asset/service (inherit chain)
  {
    fromGroups: ["processes"],
    relations: ["INVOLVES"],
    nodeTypes: ["asset", "service"],
    action: { type: "inheritChain" },
  },
  // process → division
  {
    fromGroups: ["processes"],
    relations: ["OWNS", "USES"],
    nodeTypes: ["division"],
    action: { type: "terminalGroup", group: "divisions" },
  },
  // chain propagation: DEPENDS_ON
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["DEPENDS_ON"],
    nodeTypes: ["asset", "service"],
    action: { type: "inheritChain" },
  },
  // COMPOSED_BY → service goes supporting
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["COMPOSED_BY"],
    nodeTypes: ["service"],
    action: { type: "composedByService" },
  },
  // COMPOSED_BY → asset goes dependent
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["COMPOSED_BY"],
    nodeTypes: ["asset"],
    action: { type: "composedByAsset" },
  },
  // HOSTS → service inherits chain
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["HOSTS"],
    nodeTypes: ["service"],
    action: { type: "inheritChain" },
  },
  // HOSTS → vm/cloud → hosts
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["HOSTS"],
    nodeTypes: ["virtualmachine", "cloudprovider", "host"],
    action: { type: "terminalGroup", group: "hosts" },
  },
  // OWNS/USES → asset inherits chain
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["OWNS", "USES"],
    nodeTypes: ["asset"],
    action: { type: "inheritChain" },
  },
  // OWNS/USES → division
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["OWNS", "USES"],
    nodeTypes: ["division"],
    action: { type: "terminalGroup", group: "divisions" },
  },
  // HAS_CONTRACT → contract
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["HAS_CONTRACT"],
    nodeTypes: ["contract"],
    action: { type: "terminalGroup", group: "supplierContracts" },
  },
  // HAS_LOGIN_TYPE → logintype
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["HAS_LOGIN_TYPE"],
    nodeTypes: ["logintype"],
    action: { type: "terminalGroup", group: "loginTypes" },
  },
  // HAS_SETTING → setting
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["HAS_SETTING"],
    nodeTypes: ["setting"],
    action: { type: "terminalGroup", group: "settings" },
  },
  // INVOLVES → asset/service from chain (inherit)
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["INVOLVES"],
    nodeTypes: ["asset", "service"],
    action: { type: "inheritChain" },
  },
  // INVOLVES → process
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["INVOLVES"],
    nodeTypes: ["process"],
    action: { type: "terminalGroup", group: "processes" },
  },
  // OWNS/USES → process
  {
    fromGroups: [
      "supportingAssets",
      "dependentAssets",
      "supportingServices",
      "dependentServices",
      "mutualDependencies",
    ],
    fromChainSides: ["supporting", "dependent"],
    relations: ["OWNS", "USES"],
    nodeTypes: ["process"],
    action: { type: "terminalGroup", group: "processes" },
  },
];

/** Edges that should be rendered as dashed/secondary in the diagram */
const SUPERFLUOUS_EDGE_RULES: Array<{
  sourceType: string;
  relation: string;
  targetType: string;
}> = [
  { sourceType: "service", relation: "HAS_CONTRACT", targetType: "contract" },
  { sourceType: "service", relation: "HAS_SETTING", targetType: "setting" },
  { sourceType: "asset", relation: "HAS_SETTING", targetType: "setting" },
  { sourceType: "division", relation: "OWNS", targetType: "asset" },
  { sourceType: "division", relation: "USES", targetType: "asset" },
  { sourceType: "division", relation: "OWNS", targetType: "process" },
  { sourceType: "division", relation: "USES", targetType: "process" },
  { sourceType: "asset", relation: "HAS_LOGIN_TYPE", targetType: "logintype" },
  { sourceType: "process", relation: "HAS_SETTING", targetType: "setting" },
];

// ─── Entity Palettes ──────────────────────────────────────────────────────────

const ENTITY_TYPE_PALETTES: Record<string, EntityPalette> = {
  asset: {
    nodeFill: "#F3EDE2", nodeStroke: "#64735F", nodeText: "#243128",
    groupFill: "#FAF7F2", groupStroke: "#E3DBCF",
    chipFill: "#F3EDE2", chipFillHover: "#EBE3D6", chipStroke: "#64735F", chipText: "#243128", chipMeta: "#5F6C5B",
  },
  process: {
    nodeFill: "#F4EFE6", nodeStroke: "#6F7D6B", nodeText: "#2B3730",
    groupFill: "#FBF8F4", groupStroke: "#E4DCD1",
    chipFill: "#F4EFE6", chipFillHover: "#ECE5D9", chipStroke: "#6F7D6B", chipText: "#2B3730", chipMeta: "#667361",
  },
  acnmacroarea: {
    nodeFill: "#F2EDE3", nodeStroke: "#72806F", nodeText: "#2D3932",
    groupFill: "#FAF7F3", groupStroke: "#E2DBCF",
    chipFill: "#F2EDE3", chipFillHover: "#EAE2D6", chipStroke: "#72806F", chipText: "#2D3932", chipMeta: "#69766A",
  },
  division: {
    nodeFill: "#F4EFE6", nodeStroke: "#6F7D6B", nodeText: "#2B3730",
    groupFill: "#FBF8F4", groupStroke: "#E4DCD1",
    chipFill: "#F4EFE6", chipFillHover: "#ECE5D9", chipStroke: "#6F7D6B", chipText: "#2B3730", chipMeta: "#667361",
  },
  setting: {
    nodeFill: "#F2EDE3", nodeStroke: "#72806F", nodeText: "#2D3932",
    groupFill: "#FAF7F3", groupStroke: "#E2DBCF",
    chipFill: "#F2EDE3", chipFillHover: "#EAE2D6", chipStroke: "#72806F", chipText: "#2D3932", chipMeta: "#69766A",
  },
  logintype: {
    nodeFill: "#F5F0E8", nodeStroke: "#7D8977", nodeText: "#344038",
    groupFill: "#FCF9F5", groupStroke: "#E7DED4",
    chipFill: "#F5F0E8", chipFillHover: "#EDE6DB", chipStroke: "#7D8977", chipText: "#344038", chipMeta: "#73806E",
  },
  service: {
    nodeFill: "#AEB8C2", nodeStroke: "#586776", nodeText: "#1F2C38",
    groupFill: "#E5EAEE", groupStroke: "#CBD4DB",
    chipFill: "#AEB8C2", chipFillHover: "#A3ADB7", chipStroke: "#586776", chipText: "#1F2C38", chipMeta: "#52616F",
  },
  host: {
    nodeFill: "#A4B0B9", nodeStroke: "#576674", nodeText: "#202B34",
    groupFill: "#E6EBEE", groupStroke: "#CDD5DA",
    chipFill: "#A4B0B9", chipFillHover: "#99A5AE", chipStroke: "#576674", chipText: "#202B34", chipMeta: "#53616D",
  },
  supplier: {
    nodeFill: "#A4B0B9", nodeStroke: "#576674", nodeText: "#202B34",
    groupFill: "#E6EBEE", groupStroke: "#CDD5DA",
    chipFill: "#A4B0B9", chipFillHover: "#99A5AE", chipStroke: "#576674", chipText: "#202B34", chipMeta: "#53616D",
  },
  contract: {
    nodeFill: "#A4B0B9", nodeStroke: "#576674", nodeText: "#202B34",
    groupFill: "#E6EBEE", groupStroke: "#CDD5DA",
    chipFill: "#A4B0B9", chipFillHover: "#99A5AE", chipStroke: "#576674", chipText: "#202B34", chipMeta: "#53616D",
  },
  cloudprovider: {
    nodeFill: "#A4B0B9", nodeStroke: "#576674", nodeText: "#202B34",
    groupFill: "#E6EBEE", groupStroke: "#CDD5DA",
    chipFill: "#A4B0B9", chipFillHover: "#99A5AE", chipStroke: "#576674", chipText: "#202B34", chipMeta: "#53616D",
  },
  virtualmachine: {
    nodeFill: "#A4B0B9", nodeStroke: "#576674", nodeText: "#202B34",
    groupFill: "#E6EBEE", groupStroke: "#CDD5DA",
    chipFill: "#A4B0B9", chipFillHover: "#99A5AE", chipStroke: "#576674", chipText: "#202B34", chipMeta: "#53616D",
  },
  default: {
    nodeFill: "#ECEBE7", nodeStroke: "#7A7770", nodeText: "#34312D",
    groupFill: "#F7F5F1", groupStroke: "#DFD9D1",
    chipFill: "#ECEBE7", chipFillHover: "#E3E0DB", chipStroke: "#7A7770", chipText: "#34312D", chipMeta: "#727069",
  },
};

const MERMAID_SUBJECT_NODE_CLASSDEF =
  "classDef subject-node fill:#FFA6CF,stroke:#D96EAA,stroke-width:3px,color:#0A0A0A,font-weight:700";

const GRAPH_GROUPS: Array<{ id: string; title: string; key: GraphNodeGroup }> = [
  { id: "group_mutual_dependencies", title: "Mutual Dependencies", key: "mutualDependencies" },
  { id: "group_supporting_assets", title: "Supporting Assets", key: "supportingAssets" },
  { id: "group_dependent_assets", title: "Dependent Assets", key: "dependentAssets" },
  { id: "group_supporting_services", title: "Supporting Services", key: "supportingServices" },
  { id: "group_dependent_services", title: "Dependent Services", key: "dependentServices" },
  { id: "group_hosts", title: "Hosts", key: "hosts" },
  { id: "group_login_types", title: "Login Types", key: "loginTypes" },
  { id: "group_settings", title: "Settings", key: "settings" },
  { id: "group_supplier_contracts", title: "Supplier Contracts", key: "supplierContracts" },
  { id: "group_external_suppliers", title: "External Suppliers", key: "externalSuppliers" },
  { id: "group_divisions", title: "Divisions", key: "divisions" },
  { id: "group_processes", title: "Processes", key: "processes" },
  { id: "group_acn_macro_areas", title: "ACN Macro Areas", key: "acnMacroAreas" },
  { id: "group_other_related", title: "Other Related Nodes", key: "otherRelated" },
];

// ─── Module-level caches ──────────────────────────────────────────────────────

let graphCapabilitiesCache: GraphCapabilitiesResponse | null = null;
const graphSubjectCapabilitiesCache = new Map<string, GraphCapabilitiesBySubjectTypeResponse>();
const graphSubjectOptionsCache = new Map<string, SubjectOption[]>();
const graphResultCache = new Map<string, GraphQueryResponse | null>();

// ─── Pure utility functions ───────────────────────────────────────────────────

const normalizeTypeKey = (value: string) =>
  value.toLowerCase().replace(/[^a-z0-9]/g, "");

const normalizeEntityType = (value: string): string => {
  const normalized = normalizeTypeKey(value);
  if (normalized === "process" || normalized === "processes") return "process";
  if (normalized.endsWith("s") && normalized.length > 1) return normalized.slice(0, -1);
  return normalized;
};

const normalizeRelationType = (value: string) => value.trim().toUpperCase();

const titleize = (value: string) =>
  value
    .replace(/([a-z])([A-Z])/g, "$1 $2")
    .replace(/[_-]+/g, " ")
    .trim()
    .replace(/^./, (c) => c.toUpperCase());

const formatCategoryLabel = (value: string): string => {
  const trimmed = value.trim();
  const normalized = /^[A-Z0-9_-]+$/.test(trimmed) ? trimmed.toLowerCase() : trimmed;
  const advancedMatch = normalized.match(/^advanced([a-z0-9].*)$/i);
  return advancedMatch ? `Advanced ${titleize(advancedMatch[1])}` : titleize(normalized);
};

const formatEntityTypeName = (entityType: string): string => {
  const normalized = normalizeEntityType(entityType).toLowerCase();
  return ENTITY_DISPLAY_NAME_MAP[normalized] ?? titleize(entityType);
};

const getCrudRoutePath = (entityType: string): string | null =>
  CRUD_ROUTE_MAP[normalizeEntityType(entityType).toLowerCase()] ?? null;

const getEntityTypeAlias = (entityType: string): string => {
  const normalized = normalizeEntityType(entityType).toLowerCase();
  return ENTITY_TYPE_ALIAS_MAP[normalized] ?? normalized.toUpperCase().slice(0, 3);
};

const resolveSubjectSource = (subjectType: string): SubjectSource | null => {
  const key = normalizeTypeKey(subjectType);
  return SUBJECT_SOURCES[key] ?? SUBJECT_SOURCES[key.replace(/s$/, "")] ?? null;
};

const resolveEntityPaletteKey = (value: string): string => {
  const normalized = normalizeEntityType(value);
  return normalized in ENTITY_TYPE_PALETTES ? normalized : "default";
};

const resolveEntityPalette = (value: string): EntityPalette =>
  ENTITY_TYPE_PALETTES[resolveEntityPaletteKey(value)];

const toText = (value: unknown): string => {
  if (value === null || value === undefined) return "";
  if (Array.isArray(value)) return value.map(String).join(", ");
  if (typeof value === "boolean") return value ? "Yes" : "No";
  return String(value).trim();
};

const pickFirstText = (record: Record<string, unknown>, fields: string[]): string => {
  for (const field of fields) {
    const text = toText(record[field]);
    if (text) return text;
  }
  return "";
};

const nodeKey = (type: string, id: string) => `${type.toLowerCase()}::${id}`;

const areSetsEqual = (left: Set<string>, right: Set<string>): boolean => {
  if (left.size !== right.size) return false;
  for (const value of left) {
    if (!right.has(value)) return false;
  }
  return true;
};

const uniqueNormalizedIncludes = (values: string[]): string[] =>
  Array.from(new Set(values.map(normalizeGraphIncludeId)));

const isCriticalEdge = (edge: Record<string, unknown>): boolean =>
  edge.isCritical === true ||
  edge.is_Critical === true ||
  edge.critical === true ||
  edge.IsCritical === true;

const buildNextSubjectChipStyle = (nodeType: string): NextSubjectChipStyle => {
  const palette = resolveEntityPalette(nodeType);
  return {
    "--graph-node-chip-fill": palette.chipFill,
    "--graph-node-chip-fill-hover": palette.chipFillHover,
    "--graph-node-chip-stroke": palette.chipStroke,
    "--graph-node-chip-text": palette.chipText,
    "--graph-node-chip-meta": palette.chipMeta,
  };
};

const buildSubjectOptions = (
  subjectType: string,
  items: unknown[],
  source: SubjectSource
): SubjectOption[] => {
  const options = items
    .filter((item): item is Record<string, unknown> =>
      typeof item === "object" && item !== null
    )
    .flatMap((item) => {
      const id = toText(item.id);
      if (!id) return [];

      const primary = pickFirstText(item, source.primaryFields) || "Senza nome";
      const secondary = source.secondaryFields
        .map((field) => toText(item[field]))
        .filter(Boolean)
        .slice(0, 2)
        .join(" - ");

      return [{ id, type: subjectType, primary, secondary, display: primary, searchable: `${primary} ${secondary}`.toLowerCase() }];
    });

  return options.sort((a, b) => {
    const primarySort = a.primary.localeCompare(b.primary);
    if (primarySort !== 0) return primarySort;
    const secondarySort = a.secondary.localeCompare(b.secondary);
    if (secondarySort !== 0) return secondarySort;
    return a.display.localeCompare(b.display);
  });
};

const sortFiltersByCategory = (filtersByCategory: GraphFilterCategory[]) =>
  [...filtersByCategory]
    .map((category, index) => ({
      ...category,
      _originalIndex: index,
      filters: [...category.filters]
        .map((filter, filterIndex) => ({ ...filter, _originalFilterIndex: filterIndex }))
        .sort((a, b) => {
          const hasA = typeof a.uiOrder === "number" && Number.isFinite(a.uiOrder);
          const hasB = typeof b.uiOrder === "number" && Number.isFinite(b.uiOrder);
          if (hasA && hasB) return a.uiOrder !== b.uiOrder ? (a.uiOrder as number) - (b.uiOrder as number) : a._originalFilterIndex - b._originalFilterIndex;
          if (hasA) return -1;
          if (hasB) return 1;
          return a._originalFilterIndex - b._originalFilterIndex;
        })
        .map(({ _originalFilterIndex, ...filter }) => filter),
    }))
    .sort((a, b) => {
      const orderA = a.categoryOrder ?? Number.MAX_SAFE_INTEGER;
      const orderB = b.categoryOrder ?? Number.MAX_SAFE_INTEGER;
      return orderA !== orderB ? orderA - orderB : a._originalIndex - b._originalIndex;
    })
    .map(({ _originalIndex, ...category }) => category);

const readStoredJson = <T,>(key: string): T | null => {
  const raw = window.sessionStorage.getItem(key);
  if (!raw) return null;
  try {
    return JSON.parse(raw) as T;
  } catch {
    return null;
  }
};

// ─── Edge rule helpers ────────────────────────────────────────────────────────

const matchesEdgeRule = (
  rule: SubjectEdgeRule,
  relationType: string,
  nodeType: string
): boolean =>
  rule.relations.includes(relationType) && rule.nodeTypes.includes(nodeType);

const applyEdgeRules = (
  rules: SubjectEdgeRule[],
  relationType: string,
  nodeType: string,
  nodeKey: string,
  onChainSide: (key: string, side: ChainSide) => void,
  onTerminalGroup: (key: string, group: GraphNodeGroup) => void
): void => {
  for (const rule of rules) {
    if (!matchesEdgeRule(rule, relationType, nodeType)) continue;
    if (rule.action.type === "chainSide") {
      onChainSide(nodeKey, rule.action.side);
    } else {
      onTerminalGroup(nodeKey, rule.action.group);
    }
    return;
  }
};

const isSuperfluousEdge = (edge: DisplayEdge): boolean => {
  const relation = normalizeRelationType(edge.relationType);
  const sourceType = normalizeEntityType(edge.sourceType);
  const targetType = normalizeEntityType(edge.targetType);
  return SUPERFLUOUS_EDGE_RULES.some(
    (rule) =>
      rule.sourceType === sourceType &&
      rule.relation === relation &&
      rule.targetType === targetType
  );
};

const resolveChainGroup = (nodeType: string, chainSide: ChainSide): GraphNodeGroup | null => {
  const type = normalizeEntityType(nodeType);
  if (type === "service") return chainSide === "supporting" ? "supportingServices" : "dependentServices";
  if (type === "asset") return chainSide === "supporting" ? "supportingAssets" : "dependentAssets";
  return null;
};

// ─── Mermaid escape helpers ───────────────────────────────────────────────────

const escapeMermaidText = (value: string) =>
  value
    .replace(/[\r\n]+/g, " ")
    .replace(/(\[|\]|\{|\}|\|)/g, " ")
    .replace(/"/g, "'")
    .replace(/</g, "&lt;")
    .replace(/>/g, "&gt;")
    .trim();

const escapeMermaidEdgeText = (value: string) =>
  escapeMermaidText(value)
    .replace(/-{2,}/g, "-")
    .replace(/\s+/g, " ")
    .trim();

// ─── Mermaid diagram builder ──────────────────────────────────────────────────

const buildMermaidDefinition = (
  graph: GraphQueryResponse,
  displayNodes: DisplayNode[],
  displayEdges: DisplayEdge[]
): string => {
  if (displayNodes.length === 0) return "";

  const lines: string[] = ["flowchart LR"];
  const subjectKey = nodeKey(graph.subjectType, graph.subjectId);
  const nodeIdByKey = new Map<string, string>();
  const nodeByKey = new Map<string, DisplayNode>();

  // Declare all nodes
  displayNodes.forEach((node, index) => {
    const id = `n${index}`;
    const key = nodeKey(node.type, node.id);
    nodeIdByKey.set(key, id);
    nodeByKey.set(key, node);
    lines.push(`  ${id}["${escapeMermaidText(node.label)}"]`);
    lines.push(`  class ${id} type-${resolveEntityPaletteKey(node.type)};`);
    if (key === subjectKey) lines.push(`  class ${id} subject-node;`);
  });

  // Build incident edge index for BFS
  const incidentEdgesByKey = new Map<string, Array<{ edge: DisplayEdge; isSource: boolean }>>();
  for (const edge of displayEdges) {
    const sk = nodeKey(edge.sourceType, edge.sourceId);
    const tk = nodeKey(edge.targetType, edge.targetId);
    incidentEdgesByKey.set(sk, [...(incidentEdgesByKey.get(sk) ?? []), { edge, isSource: true }]);
    incidentEdgesByKey.set(tk, [...(incidentEdgesByKey.get(tk) ?? []), { edge, isSource: false }]);
  }

  // Phase 1: classify direct neighbors of the subject
  const directChainSides = new Map<string, Set<ChainSide>>();
  const directTerminalGroups = new Map<string, GraphNodeGroup>();

  const markChainSide = (key: string, side: ChainSide) => {
    const current = directChainSides.get(key) ?? new Set<ChainSide>();
    current.add(side);
    directChainSides.set(key, current);
  };

  const markTerminalGroup = (key: string, group: GraphNodeGroup) => {
    if (!directTerminalGroups.has(key)) directTerminalGroups.set(key, group);
  };

  for (const edge of displayEdges) {
    const sk = nodeKey(edge.sourceType, edge.sourceId);
    const tk = nodeKey(edge.targetType, edge.targetId);
    const relation = normalizeRelationType(edge.relationType);

    if (sk === subjectKey) {
      applyEdgeRules(
        SUBJECT_AS_SOURCE_RULES, relation, normalizeEntityType(edge.targetType), tk,
        markChainSide, markTerminalGroup
      );
    }
    if (tk === subjectKey) {
      applyEdgeRules(
        SUBJECT_AS_TARGET_RULES, relation, normalizeEntityType(edge.sourceType), sk,
        markChainSide, markTerminalGroup
      );
    }
  }

  // Phase 2: seed node assignments
  const nodeAssignments = new Map<string, NodeAssignment>();

  const enqueueNode = (key: string, assignment: NodeAssignment): boolean => {
    if (key === subjectKey || nodeAssignments.has(key)) return false;
    nodeAssignments.set(key, assignment);
    return true;
  };

  directTerminalGroups.forEach((group, key) => enqueueNode(key, { group }));

  directChainSides.forEach((sides, key) => {
    if (nodeAssignments.has(key)) return;
    if (sides.has("supporting") && sides.has("dependent")) {
      enqueueNode(key, { group: "mutualDependencies" });
      return;
    }
    const chainSide: ChainSide = sides.has("supporting") ? "supporting" : "dependent";
    const node = nodeByKey.get(key);
    if (!node) return;
    const group = resolveChainGroup(node.type, chainSide);
    if (group) enqueueNode(key, { group, chainSide });
  });

  // Phase 3: BFS propagation
  const processQueue = Array.from(nodeAssignments.keys());

  while (processQueue.length > 0) {
    const currentKey = processQueue.shift()!;
    const currentAssignment = nodeAssignments.get(currentKey);
    if (!currentAssignment) continue;

    for (const { edge, isSource } of incidentEdgesByKey.get(currentKey) ?? []) {
      const sk = nodeKey(edge.sourceType, edge.sourceId);
      const tk = nodeKey(edge.targetType, edge.targetId);
      const neighborKey = isSource ? tk : sk;

      if (neighborKey === subjectKey || nodeAssignments.has(neighborKey)) continue;

      const neighborNode = nodeByKey.get(neighborKey);
      if (!neighborNode) continue;

      const relation = normalizeRelationType(edge.relationType);
      const neighborType = normalizeEntityType(neighborNode.type);

      for (const rule of PROPAGATION_RULES) {
        if (!rule.fromGroups.includes(currentAssignment.group)) continue;
        if (rule.fromChainSides && !rule.fromChainSides.includes(currentAssignment.chainSide!)) continue;
        if (!rule.relations.includes(relation)) continue;
        if (!rule.nodeTypes.includes(neighborType)) continue;

        const { action } = rule;

        if (action.type === "terminalGroup") {
          enqueueNode(neighborKey, { group: action.group });
        } else if (action.type === "inheritChain") {
          const chainSide = currentAssignment.chainSide ?? "supporting";
          const group = resolveChainGroup(neighborNode.type, chainSide);
          if (group) enqueueNode(neighborKey, { group, chainSide });
        } else if (action.type === "composedByService") {
          enqueueNode(neighborKey, { group: "supportingServices", chainSide: "supporting" });
        } else if (action.type === "composedByAsset") {
          enqueueNode(neighborKey, { group: "dependentAssets", chainSide: "dependent" });
        }

        processQueue.push(neighborKey);
        break;
      }
    }
  }

  // Phase 4: place remaining unassigned nodes
  const inGroup = new Set<string>();
  const groups = Object.fromEntries(
    GRAPH_GROUPS.map((g) => [g.key, [] as string[]])
  ) as Record<GraphNodeGroup, string[]>;

  const addNodeToGroup = (group: GraphNodeGroup, key: string) => {
    if (inGroup.has(key)) return;
    groups[group].push(key);
    inGroup.add(key);
  };

  for (const node of displayNodes) {
    const key = nodeKey(node.type, node.id);
    if (key === subjectKey) continue;
    addNodeToGroup(nodeAssignments.get(key)?.group ?? "otherRelated", key);
  }

  // Phase 5: render subgraphs
  const sortGroupKeys = (keys: string[]) =>
    [...keys].sort((a, b) => {
      const na = nodeByKey.get(a);
      const nb = nodeByKey.get(b);
      if (!na || !nb) return a.localeCompare(b);
      const typeSort = na.type.localeCompare(nb.type);
      return typeSort !== 0 ? typeSort : na.label.localeCompare(nb.label);
    });

  for (const { id, title, key } of GRAPH_GROUPS) {
    const sorted = sortGroupKeys(groups[key]);
    if (sorted.length === 0) continue;

    const typeCounts = new Map<string, number>();
    sorted.forEach((k) => {
      const t = resolveEntityPaletteKey(nodeByKey.get(k)?.type ?? "");
      typeCounts.set(t, (typeCounts.get(t) ?? 0) + 1);
    });

    let dominantTypeKey = "default";
    let dominantCount = -1;
    typeCounts.forEach((count, typeKey) => {
      if (count > dominantCount) { dominantTypeKey = typeKey; dominantCount = count; }
    });

    const palette = ENTITY_TYPE_PALETTES[dominantTypeKey] ?? ENTITY_TYPE_PALETTES.default;

    lines.push(`  subgraph ${id}["${escapeMermaidText(title)}"]`);
    lines.push("    direction TB");
    sorted.forEach((k) => { const nid = nodeIdByKey.get(k); if (nid) lines.push(`    ${nid}`); });
    lines.push("  end");
    lines.push(`  style ${id} fill:${palette.groupFill},stroke:${palette.groupStroke},stroke-width:1px;`);
  }

  // Phase 6: render edges
  const criticalLinkIndexes: number[] = [];
  const superfluousLinkIndexes: number[] = [];
  const pairedEdgeKeys = new Set<string>();
  let linkIndex = 0;

  const edgePairKey = (sk: string, tk: string, relation: string) => `${sk}__${tk}__${relation}`;

  for (const edge of displayEdges) {
    const sk = nodeKey(edge.sourceType, edge.sourceId);
    const tk = nodeKey(edge.targetType, edge.targetId);
    const source = nodeIdByKey.get(sk);
    const target = nodeIdByKey.get(tk);
    const relation = normalizeRelationType(edge.relationType);

    if (!source || !target) continue;

    const pairKey = edgePairKey(sk, tk, relation);
    if (pairedEdgeKeys.has(pairKey)) continue;

    const reciprocalKey = edgePairKey(tk, sk, relation);
    const reciprocal = displayEdges.find(
      (c) =>
        normalizeRelationType(c.relationType) === relation &&
        c.sourceId === edge.targetId &&
        c.targetId === edge.sourceId &&
        normalizeTypeKey(c.sourceType) === normalizeTypeKey(edge.targetType) &&
        normalizeTypeKey(c.targetType) === normalizeTypeKey(edge.sourceType)
    );

    const relationLabel = escapeMermaidEdgeText(titleize(edge.relationType));
    const isCurrentSuperfluous = isSuperfluousEdge(edge);

    if (reciprocal && !pairedEdgeKeys.has(reciprocalKey)) {
      let leftKey = sk;
      let rightKey = tk;

      if (tk === subjectKey && sk !== subjectKey) {
        leftKey = tk; rightKey = sk;
      } else if (leftKey !== subjectKey && rightKey !== subjectKey) {
        const ln = nodeByKey.get(leftKey);
        const rn = nodeByKey.get(rightKey);
        if (ln && rn && ln.label.localeCompare(rn.label) > 0) {
          leftKey = sk; rightKey = tk;
        }
      }

      const leftId = nodeIdByKey.get(leftKey);
      const rightId = nodeIdByKey.get(rightKey);

      if (leftId && rightId) {
        lines.push(relationLabel ? `  ${leftId} <-->|${relationLabel}| ${rightId}` : `  ${leftId} <--> ${rightId}`);
        if (isCurrentSuperfluous || isSuperfluousEdge(reciprocal)) superfluousLinkIndexes.push(linkIndex);
        if (edge.isCritical || reciprocal.isCritical) criticalLinkIndexes.push(linkIndex);
        linkIndex++;
        pairedEdgeKeys.add(pairKey);
        pairedEdgeKeys.add(reciprocalKey);
        continue;
      }
    }

    lines.push(relationLabel ? `  ${source} -->|${relationLabel}| ${target}` : `  ${source} --> ${target}`);
    if (isCurrentSuperfluous) superfluousLinkIndexes.push(linkIndex);
    if (edge.isCritical) criticalLinkIndexes.push(linkIndex);
    linkIndex++;
    pairedEdgeKeys.add(pairKey);
  }

  // Phase 7: class definitions
  const typeClasses = Array.from(new Set(displayNodes.map((n) => resolveEntityPaletteKey(n.type))));
  typeClasses.forEach((typeClass) => {
    const p = ENTITY_TYPE_PALETTES[typeClass] ?? ENTITY_TYPE_PALETTES.default;
    lines.push(`  classDef type-${typeClass} fill:${p.nodeFill},stroke:${p.nodeStroke},color:${p.nodeText};`);
  });
  lines.push(MERMAID_SUBJECT_NODE_CLASSDEF);

  // Phase 8: link styles
  const criticalSet = new Set(criticalLinkIndexes);
  const superfluousSet = new Set(superfluousLinkIndexes);
  const styledIndexes = Array.from(new Set([...criticalLinkIndexes, ...superfluousLinkIndexes])).sort((a, b) => a - b);

  for (const index of styledIndexes) {
    const styles: string[] = [];
    if (criticalSet.has(index)) styles.push("stroke:#7C3AED", "stroke-width:2.5px");
    if (superfluousSet.has(index)) styles.push("stroke-dasharray:7 5");
    if (styles.length > 0) lines.push(`  linkStyle ${index} ${styles.join(",")};`);
  }

  return lines.join("\n");
};

// ─── Component ────────────────────────────────────────────────────────────────

export default function GraphExplorerPage() {
  const location = useLocation();
  const navigate = useNavigate();

  const graphStateStorageKey = useMemo(
    () => `graph-explorer:state:${location.pathname}`,
    [location.pathname]
  );
  const graphResultStorageKey = useMemo(
    () => `graph-explorer:last-graph:${location.pathname}`,
    [location.pathname]
  );

  const storedState = useMemo(
    () => readStoredJson<GraphExplorerStoredState>(graphStateStorageKey),
    [graphStateStorageKey]
  );
  const storedGraph = useMemo(
    () => readStoredJson<GraphQueryResponse>(graphResultStorageKey),
    [graphResultStorageKey]
  );

  const mermaidContainerRef = useRef<HTMLDivElement | null>(null);
  const isMermaidInitialized = useRef(false);
  const mermaidModuleRef = useRef<typeof import("mermaid") | null>(null);
  const pendingSubjectSelectionRef = useRef<{
    subjectType: string;
    subjectId: string;
    label: string;
  } | null>(null);

  // ── State ──────────────────────────────────────────────────────────────────

  const [capabilities, setCapabilities] = useState<GraphCapabilitiesResponse | null>(
    () => graphCapabilitiesCache
  );
  const [isLoadingCapabilities, setIsLoadingCapabilities] = useState(
    () => graphCapabilitiesCache === null
  );
  const [subjectCapabilities, setSubjectCapabilities] =
    useState<GraphCapabilitiesBySubjectTypeResponse | null>(null);
  const [isLoadingSubjectCapabilities, setIsLoadingSubjectCapabilities] = useState(false);
  const [isLoadingGraph, setIsLoadingGraph] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const [subjectType, setSubjectType] = useState(() => storedState?.subjectType ?? "asset");
  const [subjectId, setSubjectId] = useState(() => storedState?.subjectId ?? "");
  const [subjectSearch, setSubjectSearch] = useState("");
  const [isSubjectDropdownOpen, setIsSubjectDropdownOpen] = useState(false);
  const [subjectOptions, setSubjectOptions] = useState<SubjectOption[]>([]);
  const [isLoadingSubjects, setIsLoadingSubjects] = useState(false);
  const [displayedSubjectCount, setDisplayedSubjectCount] = useState(12);

  const [includes, setIncludes] = useState<string[]>(() =>
    Array.isArray(storedState?.includes) ? storedState.includes : []
  );
  const [expandedCategoryKeys, setExpandedCategoryKeys] = useState<Set<string>>(
    () => new Set(Array.isArray(storedState?.expandedCategoryKeys) ? storedState.expandedCategoryKeys : [])
  );
  const [dependencyDepth, setDependencyDepth] = useState(() =>
    typeof storedState?.dependencyDepth === "number" ? storedState.dependencyDepth : 1
  );

  const [graph, setGraph] = useState<GraphQueryResponse | null>(() => {
    if (graphResultCache.has(graphResultStorageKey)) {
      return graphResultCache.get(graphResultStorageKey) ?? null;
    }
    return storedGraph;
  });
  const [mermaidSvg, setMermaidSvg] = useState("");
  const [mermaidError, setMermaidError] = useState<string | null>(null);
  const [showMermaidSource, setShowMermaidSource] = useState(false);
  const [showJson, setShowJson] = useState(false);

  // ── Derived state ──────────────────────────────────────────────────────────

  const availableSubjectTypes = useMemo(
    () => (capabilities?.subjectTypes.length ? capabilities.subjectTypes : DEFAULT_SUBJECT_TYPES),
    [capabilities]
  );

  const filtersByCategory = useMemo(
    () => subjectCapabilities?.filtersByCategory.length
      ? subjectCapabilities.filtersByCategory
      : EMPTY_FILTER_CATEGORIES,
    [subjectCapabilities]
  );

  const orderedFiltersByCategory = useMemo(
    () => sortFiltersByCategory(filtersByCategory),
    [filtersByCategory]
  );

  const keyedFiltersByCategory = useMemo<DisplayFilterCategory[]>(() => {
    const keyed = orderedFiltersByCategory.map((category, index) => ({
      ...category,
      key: `${normalizeTypeKey(category.category)}-${index}`,
    }));

    const advanced = keyed.filter((c) => c.category.trim().toLowerCase().startsWith("advanced"));
    const regular = keyed.filter((c) => !c.category.trim().toLowerCase().startsWith("advanced"));

    const regularDisplay: DisplayFilterCategory[] = regular.map((c) => ({
      key: c.key,
      category: c.category,
      categoryOrder: c.categoryOrder,
      filters: c.filters,
      sourceCategories: [{ key: c.key, category: c.category, filters: c.filters }],
    }));

    if (advanced.length === 0) return regularDisplay;

    return [
      ...regularDisplay,
      {
        key: "advanced-filters-group",
        category: "Advanced Filters",
        categoryOrder: Math.min(...advanced.map((c) => c.categoryOrder ?? Number.MAX_SAFE_INTEGER)),
        filters: advanced.flatMap((c) => c.filters),
        sourceCategories: advanced.map((c) => ({ key: c.key, category: c.category, filters: c.filters })),
      },
    ];
  }, [orderedFiltersByCategory]);

  const availableFilterIds = useMemo(
    () => keyedFiltersByCategory.flatMap((c) => c.filters.map((f) => f.id)),
    [keyedFiltersByCategory]
  );

  const normalizedIncludes = useMemo(() => uniqueNormalizedIncludes(includes), [includes]);
  const includeLookup = useMemo(() => new Set(normalizedIncludes), [normalizedIncludes]);

  const selectedDepthFilters = useMemo(
    () =>
      keyedFiltersByCategory.flatMap((c) =>
        c.filters.filter(
          (f) => f.usesDependencyDepth && includeLookup.has(normalizeGraphIncludeId(f.id))
        )
      ),
    [includeLookup, keyedFiltersByCategory]
  );

  const hasSelectedDepthFilter = selectedDepthFilters.length > 0;
  const maxDependencyDepth = capabilities?.maxDependencyDepth ?? 5;

  const mapSubjectTypeFromNodeType = useCallback(
    (nodeType: string) => {
      const normalized = normalizeTypeKey(nodeType);
      const singular = normalized.endsWith("s") ? normalized.slice(0, -1) : normalized;
      const plural = singular ? `${singular}s` : normalized;

      return (
        availableSubjectTypes.find((t) => {
          const n = normalizeTypeKey(t);
          return n === normalized || n === singular || n === plural;
        }) ?? null
      );
    },
    [availableSubjectTypes]
  );

  const filteredSubjectOptions = useMemo(() => {
    const term = subjectSearch.trim().toLowerCase();
    return term ? subjectOptions.filter((o) => o.searchable.includes(term)) : subjectOptions;
  }, [subjectOptions, subjectSearch]);

  const selectedSubjectOption = useMemo(
    () => subjectOptions.find((o) => o.id === subjectId) ?? null,
    [subjectId, subjectOptions]
  );

  const nodeLookup = useMemo(() => {
    const byComposite = new Map<string, DisplayNode>();
    const byId = new Map<string, DisplayNode>();

    graph?.nodes.forEach((node) => {
      byComposite.set(nodeKey(node.type, node.id), node);
      if (!byId.has(node.id)) byId.set(node.id, node);
    });

    return { byComposite, byId };
  }, [graph]);

  const resolveNodeLabel = useCallback(
    (type: string, id: string): DisplayNode => {
      return (
        nodeLookup.byComposite.get(nodeKey(type, id)) ??
        nodeLookup.byId.get(id) ??
        { id, type, label: "Item without label" }
      );
    },
    [nodeLookup]
  );

  const graphSubjectLabel = useMemo(
    () => (graph ? resolveNodeLabel(graph.subjectType, graph.subjectId).label : ""),
    [graph, resolveNodeLabel]
  );

  const displayNodes = useMemo<DisplayNode[]>(() => {
    if (!graph) return [];
    return [...graph.nodes].sort((a, b) => {
      const aIsSubject = a.id === graph.subjectId && a.type.toLowerCase() === graph.subjectType.toLowerCase();
      const bIsSubject = b.id === graph.subjectId && b.type.toLowerCase() === graph.subjectType.toLowerCase();
      if (aIsSubject !== bIsSubject) return aIsSubject ? -1 : 1;
      const typeSort = a.type.localeCompare(b.type);
      return typeSort !== 0 ? typeSort : a.label.localeCompare(b.label);
    });
  }, [graph]);

  const displayEdges = useMemo<DisplayEdge[]>(() => {
    if (!graph) return [];
    return [...graph.edges]
      .map((edge) => ({ ...edge, isCritical: isCriticalEdge(edge as Record<string, unknown>) }))
      .sort((a, b) => {
        const relationSort = a.relationType.localeCompare(b.relationType);
        if (relationSort !== 0) return relationSort;
        const sourceSort = resolveNodeLabel(a.sourceType, a.sourceId).label.localeCompare(
          resolveNodeLabel(b.sourceType, b.sourceId).label
        );
        if (sourceSort !== 0) return sourceSort;
        return resolveNodeLabel(a.targetType, a.targetId).label.localeCompare(
          resolveNodeLabel(b.targetType, b.targetId).label
        );
      });
  }, [graph, resolveNodeLabel]);

  const hasGraphData = Boolean(graph && graph.nodes.length > 0);

  const nextSubjectCandidates = useMemo<NextSubjectCandidate[]>(() => {
    if (!graph) return [];
    return displayNodes
      .filter((node) => !(node.id === graph.subjectId && normalizeTypeKey(node.type) === normalizeTypeKey(graph.subjectType)))
      .map((node) => {
        const matchingSubjectType = mapSubjectTypeFromNodeType(node.type);
        return { ...node, matchingSubjectType, isSelectable: Boolean(matchingSubjectType) };
      })
      .sort((a, b) => {
        const orderA = ENTITY_TYPE_ORDER[normalizeEntityType(a.type).toLowerCase()] ?? 999;
        const orderB = ENTITY_TYPE_ORDER[normalizeEntityType(b.type).toLowerCase()] ?? 999;
        return orderA !== orderB ? orderA - orderB : a.label.localeCompare(b.label);
      });
  }, [displayNodes, graph, mapSubjectTypeFromNodeType]);

  const mermaidDefinition = useMemo(() => {
    if (!graph || !hasGraphData) return "";
    return buildMermaidDefinition(graph, displayNodes, displayEdges);
  }, [graph, hasGraphData, displayNodes, displayEdges]);

  // ── Effects ────────────────────────────────────────────────────────────────

  // Reset expanded categories when subject type changes
  useEffect(() => {
    const nextKeys =
      keyedFiltersByCategory.length === 0
        ? new Set<string>()
        : new Set<string>([
            (
              keyedFiltersByCategory.find((c) => c.category.trim().toLowerCase() === "main") ??
              keyedFiltersByCategory[0]
            ).key,
          ]);

    setExpandedCategoryKeys((current) =>
      areSetsEqual(current, nextKeys) ? current : nextKeys
    );
  }, [subjectType, keyedFiltersByCategory]);

  // Reset subject type if not in available list
  useEffect(() => {
    if (isLoadingCapabilities) return;
    const initialType = availableSubjectTypes[0] ?? "asset";
    if (!availableSubjectTypes.some((t) => t.toLowerCase() === subjectType.toLowerCase())) {
      setSubjectType(initialType);
    }
  }, [availableSubjectTypes, isLoadingCapabilities, subjectType]);

  // Load capabilities
  useEffect(() => {
    if (graphCapabilitiesCache) {
      setCapabilities(graphCapabilitiesCache);
      setIsLoadingCapabilities(false);
      return;
    }

    let isActive = true;

    (async () => {
      setIsLoadingCapabilities(true);
      setError(null);
      try {
        const data = await getGraphCapabilities();
        if (!isActive) return;
        graphCapabilitiesCache = data;
        setCapabilities(data);
      } catch (err: unknown) {
        if (!isActive) return;
        setError(
          typeof err === "object" && err !== null && "friendlyMessage" in err
            ? String((err as { friendlyMessage?: string }).friendlyMessage)
            : "Unable to load the graph capabilities."
        );
      } finally {
        if (isActive) setIsLoadingCapabilities(false);
      }
    })();

    return () => { isActive = false; };
  }, []);

  // Persist state to sessionStorage
  useEffect(() => {
    window.sessionStorage.setItem(
      graphStateStorageKey,
      JSON.stringify({
        subjectType,
        subjectId,
        includes,
        dependencyDepth,
        expandedCategoryKeys: Array.from(expandedCategoryKeys),
      })
    );
  }, [graphStateStorageKey, subjectType, subjectId, includes, dependencyDepth, expandedCategoryKeys]);

  // Persist graph to sessionStorage
  useEffect(() => {
    graphResultCache.set(graphResultStorageKey, graph);
    if (!graph) {
      window.sessionStorage.removeItem(graphResultStorageKey);
    } else {
      window.sessionStorage.setItem(graphResultStorageKey, JSON.stringify(graph));
    }
  }, [graphResultStorageKey, graph]);

  // Load subject capabilities
  useEffect(() => {
    let isActive = true;
    const cacheKey = normalizeTypeKey(subjectType);
    const cached = graphSubjectCapabilitiesCache.get(cacheKey);

    const applyCapabilities = (data: GraphCapabilitiesBySubjectTypeResponse) => {
      const filterIds = sortFiltersByCategory(data.filtersByCategory)
        .flatMap((c) => c.filters)
        .map((f) => normalizeGraphIncludeId(f.id));

      setSubjectCapabilities(data);
      setIncludes((current) => {
        const valid = uniqueNormalizedIncludes(current).filter((id) => filterIds.includes(id));
        return valid.length > 0 ? valid : filterIds.length > 0 ? [filterIds[0]] : [];
      });
    };

    if (cached) {
      applyCapabilities(cached);
      setIsLoadingSubjectCapabilities(false);
      return () => { isActive = false; };
    }

    (async () => {
      setIsLoadingSubjectCapabilities(true);
      try {
        const data = await getGraphCapabilitiesBySubjectType(subjectType);
        if (!isActive) return;
        graphSubjectCapabilitiesCache.set(cacheKey, data);
        applyCapabilities(data);
      } catch (err: unknown) {
        if (!isActive) return;
        setError(
          typeof err === "object" && err !== null && "friendlyMessage" in err
            ? String((err as { friendlyMessage?: string }).friendlyMessage)
            : "Unable to load filters for the selected subject."
        );
        setSubjectCapabilities({ subjectType, filtersByCategory: [] });
        setIncludes([]);
      } finally {
        if (isActive) setIsLoadingSubjectCapabilities(false);
      }
    })();

    return () => { isActive = false; };
  }, [subjectType]);

  // Load subject options
  useEffect(() => {
    let isActive = true;
    const cacheKey = normalizeTypeKey(subjectType);

    const syncSelectedSubject = (options: SubjectOption[]) => {
      const pending = pendingSubjectSelectionRef.current;
      const pendingForType =
        pending && normalizeTypeKey(pending.subjectType) === normalizeTypeKey(subjectType)
          ? pending
          : null;

      if (options.length === 0) {
        if (pendingForType) {
          setSubjectId(pendingForType.subjectId);
          setSubjectSearch(pendingForType.label || pendingForType.subjectId);
          pendingSubjectSelectionRef.current = null;
        } else {
          setSubjectId("");
          setSubjectSearch("");
        }
        return;
      }

      setSubjectId((currentId) => {
        const preferredId = pendingForType?.subjectId ?? currentId;
        const match = options.find((o) => o.id === preferredId);

        if (match) {
          setSubjectSearch(match.display);
          pendingSubjectSelectionRef.current = null;
          return match.id;
        }

        if (pendingForType) {
          setSubjectSearch(pendingForType.label || pendingForType.subjectId);
          pendingSubjectSelectionRef.current = null;
          return pendingForType.subjectId;
        }

        const currentMatch = options.find((o) => o.id === currentId);
        if (!currentMatch) return "";
        setSubjectSearch(currentMatch.display);
        return currentMatch.id;
      });
    };

    const pending = pendingSubjectSelectionRef.current;
    const pendingForType =
      pending && normalizeTypeKey(pending.subjectType) === normalizeTypeKey(subjectType)
        ? pending
        : null;

    if (pendingForType) {
      setSubjectId(pendingForType.subjectId);
      setSubjectSearch(pendingForType.label || pendingForType.subjectId);
    }

    const cached = graphSubjectOptionsCache.get(cacheKey);
    if (cached) {
      setSubjectOptions(cached);
      syncSelectedSubject(cached);
    }

    const source = resolveSubjectSource(subjectType);
    if (!source) {
      setSubjectOptions([]);
      if (pendingForType) pendingSubjectSelectionRef.current = null;
      return () => { isActive = false; };
    }

    setIsLoadingSubjects(true);

    (async () => {
      try {
        const response = await api.get<unknown[]>(source.endpoint);
        if (!isActive) return;
        const options = buildSubjectOptions(subjectType, Array.isArray(response.data) ? response.data : [], source);
        graphSubjectOptionsCache.set(cacheKey, options);
        setSubjectOptions(options);
        syncSelectedSubject(options);
      } catch {
        if (!isActive) return;
        setSubjectOptions([]);
        setSubjectId("");
        setSubjectSearch("");
      } finally {
        if (isActive) setIsLoadingSubjects(false);
      }
    })();

    return () => { isActive = false; };
  }, [subjectType]);

  // Reset displayed count when results change
  useEffect(() => { setDisplayedSubjectCount(12); }, [filteredSubjectOptions]);

  // Render Mermaid diagram
  useEffect(() => {
    let isActive = true;

    (async () => {
      if (!mermaidDefinition || !mermaidContainerRef.current) {
        if (mermaidContainerRef.current) mermaidContainerRef.current.innerHTML = "";
        setMermaidSvg("");
        setMermaidError(null);
        return;
      }

      try {
        setMermaidError(null);

        if (!mermaidModuleRef.current) {
          mermaidModuleRef.current = await import("mermaid");
        }

        const mermaid = mermaidModuleRef.current.default;

        if (!isMermaidInitialized.current) {
          mermaid.initialize({
            startOnLoad: false,
            securityLevel: "loose",
            theme: "base",
            themeVariables: {
              background: "#FFFFFF",
              fontFamily: "Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, 'Segoe UI', sans-serif",
              fontSize: "14px",
              primaryColor: "#F3EDE2",
              primaryTextColor: "#243128",
              primaryBorderColor: "#64735F",
              secondaryColor: "#AEB8C2",
              secondaryTextColor: "#1F2C38",
              secondaryBorderColor: "#586776",
              tertiaryColor: "#FAF6F0",
              tertiaryTextColor: "#33404C",
              tertiaryBorderColor: "#DDD3C5",
              lineColor: "#7A8694",
              textColor: "#22303A",
              mainBkg: "#F3EDE2",
              nodeBorder: "#64735F",
              clusterBkg: "#FAF6F0",
              clusterBorder: "#DDD3C5",
              edgeLabelBackground: "#FFFFFF",
              titleColor: "#243128",
            },
            themeCSS: `
              .label, .cluster-label, .edgeLabel, .nodeLabel {
                font-family: Inter, ui-sans-serif, system-ui, -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif !important;
                letter-spacing: -0.01em;
              }
              .nodeLabel { font-weight: 600; }
              .cluster-label text, .cluster-label span, .label text, .label span {
                fill: #243128 !important; color: #243128 !important; font-weight: 600;
              }
              .cluster rect { rx: 8px; ry: 8px; }
              .node rect, .node polygon, .node path { rx: 6px; ry: 6px; }
              .edgeLabel rect, .labelBkg {
                fill: transparent !important; stroke: transparent !important;
                fill-opacity: 0 !important; stroke-opacity: 0 !important;
              }
              .edgeLabel foreignObject { overflow: visible !important; }
              .edgeLabel, .edgeLabel div, .edgeLabel span {
                background: transparent !important; box-shadow: none !important; border: none !important;
              }
              .edgeLabel p {
                margin: 0 auto !important; padding: 0.16rem 0.56rem !important;
                border-radius: 999px !important; border: 1px solid #D9E1E7 !important;
                background: #FFFFFF !important; color: #556470 !important;
                font-weight: 500 !important; font-size: 12px !important;
                line-height: 1 !important; white-space: nowrap !important;
                box-shadow: 0 1px 1px rgba(15, 23, 42, 0.03) !important;
              }
            `,
            flowchart: { useMaxWidth: true, htmlLabels: true, curve: "basis" },
          });
          isMermaidInitialized.current = true;
        }

        const { svg } = await mermaid.render(`graph-mermaid-${Date.now()}`, mermaidDefinition);
        if (!isActive || !mermaidContainerRef.current) return;
        mermaidContainerRef.current.innerHTML = svg;
        setMermaidSvg(svg);
      } catch {
        if (!isActive || !mermaidContainerRef.current) return;
        mermaidContainerRef.current.innerHTML = "";
        setMermaidSvg("");
        setMermaidError("Unable to render the Mermaid diagram for this result.");
      }
    })();

    return () => { isActive = false; };
  }, [mermaidDefinition]);

  // ── Handlers ───────────────────────────────────────────────────────────────

  const pickSubjectOption = (option: SubjectOption) => {
    setSubjectId(option.id);
    setSubjectSearch(option.display);
    setIsSubjectDropdownOpen(false);
  };

  const useNodeAsNextSubject = (node: NextSubjectCandidate) => {
    if (!node.matchingSubjectType) {
      setError(`Il nodo ${node.label} non puo essere usato come subject con le capabilities correnti.`);
      return;
    }
    pendingSubjectSelectionRef.current = {
      subjectType: node.matchingSubjectType,
      subjectId: node.id,
      label: node.label || node.id,
    };
    setSubjectType(node.matchingSubjectType);
    setSubjectId(node.id);
    setSubjectSearch(node.label || node.id);
    setIsSubjectDropdownOpen(false);
    setError(null);
  };

  const toggleInclude = (include: string) => {
    const id = normalizeGraphIncludeId(include);
    setIncludes((current) => {
      const normalized = uniqueNormalizedIncludes(current);
      if (normalized.includes(id)) {
        const next = current.filter((item) => normalizeGraphIncludeId(item) !== id);
        return next.length > 0 ? next : current;
      }
      return [...normalized, id];
    });
  };

  const toggleCategoryExpanded = (categoryKey: string) => {
    setExpandedCategoryKeys((current) => {
      const next = new Set(current);
      next.has(categoryKey) ? next.delete(categoryKey) : next.add(categoryKey);
      return next;
    });
  };

  const selectAllIncludes = () => setIncludes(uniqueNormalizedIncludes(availableFilterIds));
  const deselectAllIncludes = () => setIncludes([]);

  const toggleFilterIds = (filterIds: string[]) => {
    const scoped = filterIds.map(normalizeGraphIncludeId);
    setIncludes((current) => {
      const normalized = uniqueNormalizedIncludes(current);
      const allSelected = scoped.length > 0 && scoped.every((id) => normalized.includes(id));
      return allSelected
        ? normalized.filter((id) => !scoped.includes(id))
        : uniqueNormalizedIncludes([...normalized, ...scoped]);
    });
  };

  const toggleCategoryIncludes = (category: DisplayFilterCategory) =>
    toggleFilterIds(category.filters.map((f) => f.id));

  const clearResults = () => {
    setGraph(null);
    setMermaidSvg("");
    setMermaidError(null);
    setShowMermaidSource(false);
    setShowJson(false);
    setError(null);
  };

  const submitQuery = async (event?: FormEvent<HTMLFormElement>) => {
    event?.preventDefault();

    const trimmedId = subjectId.trim();
    if (!trimmedId) {
      setError("Select a subject from the list before running the query.");
      return;
    }

    const allowedIncludes = new Set(availableFilterIds.map(normalizeGraphIncludeId));
    const selectedIncludes = includes.map(normalizeGraphIncludeId).filter((id) => allowedIncludes.has(id));
    if (selectedIncludes.length === 0) {
      setError("Seleziona almeno una include.");
      return;
    }

    setIsLoadingGraph(true);
    setError(null);

    try {
      const response = await queryRelationshipGraph({
        SubjectType: subjectType,
        SubjectId: trimmedId,
        Includes: selectedIncludes,
        DependencyDepth: dependencyDepth,
      });

      setGraph(response);
      setSubjectType(response.subjectType);
      setSubjectId(response.subjectId);
      setShowMermaidSource(false);
      setShowJson(false);

      const match = subjectOptions.find((o) => o.id === response.subjectId);
      if (match) setSubjectSearch(match.display);
    } catch (err: unknown) {
      setError(
        typeof err === "object" && err !== null && "friendlyMessage" in err
          ? String((err as { friendlyMessage?: string }).friendlyMessage)
          : "Error during the graph query."
      );
    } finally {
      setIsLoadingGraph(false);
    }
  };

  const downloadFile = (filename: string, content: BlobPart, mimeType: string) => {
    const blob = new Blob([content], { type: mimeType });
    const url = URL.createObjectURL(blob);
    const anchor = Object.assign(document.createElement("a"), { href: url, download: filename });
    document.body.appendChild(anchor);
    anchor.click();
    document.body.removeChild(anchor);
    URL.revokeObjectURL(url);
  };

  const downloadMermaidSvg = () => {
    if (mermaidSvg) downloadFile("relationship-graph.svg", mermaidSvg, "image/svg+xml;charset=utf-8");
  };

  const downloadMermaidPng = async () => {
    if (!mermaidSvg) return;

    try {
      const parser = new DOMParser();
      const svgElement = parser.parseFromString(mermaidSvg, "image/svg+xml").documentElement;
      const renderedSvg = mermaidContainerRef.current?.querySelector("svg");
      const renderedRect = renderedSvg?.getBoundingClientRect();

      const renderedWidth = renderedRect?.width && Number.isFinite(renderedRect.width) ? renderedRect.width : 0;
      const renderedHeight = renderedRect?.height && Number.isFinite(renderedRect.height) ? renderedRect.height : 0;

      const parseAttr = (attr: string) => Number.parseFloat((svgElement.getAttribute(attr) ?? "").replace(/[^0-9.]/g, ""));
      let width = parseAttr("width") || 0;
      let height = parseAttr("height") || 0;

      if (!width || !height) {
        const viewBox = (svgElement.getAttribute("viewBox") ?? "")
          .split(/[\s,]+/)
          .map(Number)
          .filter(Number.isFinite);
        if (viewBox.length === 4) { width = width || viewBox[2]; height = height || viewBox[3]; }
      }

      width = width || 1600;
      height = height || 900;
      const exportWidth = renderedWidth || width;
      const exportHeight = renderedHeight || height;

      svgElement.setAttribute("width", `${exportWidth}`);
      svgElement.setAttribute("height", `${exportHeight}`);
      svgElement.setAttribute("xmlns", "http://www.w3.org/2000/svg");
      svgElement.setAttribute("xmlns:xlink", "http://www.w3.org/1999/xlink");

      const encodedSvg = `data:image/svg+xml;charset=utf-8,${encodeURIComponent(new XMLSerializer().serializeToString(svgElement))}`;
      const image = new Image();

      const pngBlob = await new Promise<Blob>((resolve, reject) => {
        image.onload = () => {
          const scale = Math.max(1, Math.min(2, window.devicePixelRatio || 1, 2400 / Math.max(exportWidth, exportHeight)));
          const canvas = document.createElement("canvas");
          canvas.width = Math.max(1, Math.round(exportWidth * scale));
          canvas.height = Math.max(1, Math.round(exportHeight * scale));

          const ctx = canvas.getContext("2d");
          if (!ctx) { reject(new Error("Canvas context not available")); return; }

          ctx.fillStyle = "#ffffff";
          ctx.fillRect(0, 0, canvas.width, canvas.height);
          ctx.imageSmoothingEnabled = true;
          ctx.imageSmoothingQuality = "high";
          ctx.setTransform(scale, 0, 0, scale, 0, 0);
          ctx.drawImage(image, 0, 0, exportWidth, exportHeight);

          canvas.toBlob(
            (blob) => blob ? resolve(blob) : reject(new Error("Unable to generate PNG")),
            "image/png"
          );
        };
        image.onerror = () => reject(new Error("Unable to read SVG"));
        image.src = encodedSvg;
      });

      const url = URL.createObjectURL(pngBlob);
      const anchor = Object.assign(document.createElement("a"), { href: url, download: "relationship-graph.png" });
      document.body.appendChild(anchor);
      anchor.click();
      document.body.removeChild(anchor);
      URL.revokeObjectURL(url);
    } catch {
      setError("Unable to export the graph as PNG.");
    }
  };

  // ── Render ─────────────────────────────────────────────────────────────────

  const sortedSubjectTypes = useMemo(
    () =>
      [...availableSubjectTypes].sort((a, b) => {
        const orderA = ENTITY_TYPE_ORDER[normalizeEntityType(a).toLowerCase()] ?? 999;
        const orderB = ENTITY_TYPE_ORDER[normalizeEntityType(b).toLowerCase()] ?? 999;
        return orderA !== orderB ? orderA - orderB : formatEntityTypeName(a).localeCompare(formatEntityTypeName(b));
      }),
    [availableSubjectTypes]
  );

  return (
    <div className="page graph-explorer-page">
      <div className="page-header">
        <div>
          <h2 className="page-title-with-icon">
            <EntityIcon icon={ENTITY_ICONS.graphExplorer} size={24} className="page-title-icon" />
            <span>Graph Explorer</span>
          </h2>
          <p>Queries the backend in query-driven mode and displays the Mermaid graph directly.</p>
        </div>
      </div>

      {error && <div className="alert alert-error"><span>{error}</span></div>}

      <section className="card graph-query-card">
        <h3>Query</h3>

        {isLoadingCapabilities && (
          <div className="alert badge-info">Loading capabilities...</div>
        )}
        {isLoadingSubjectCapabilities && (
          <div className="alert badge-info">Loading filters for the selected subject type...</div>
        )}

        <form className="graph-query-form" onSubmit={submitQuery}>
          <div className="graph-query-grid">
            {/* Subject Type */}
            <div className="form-field">
              <label htmlFor="subjectType">Subject Type</label>
              <select
                id="subjectType"
                value={subjectType}
                onChange={(e) => {
                  pendingSubjectSelectionRef.current = null;
                  setSubjectType(e.target.value);
                  setSubjectId("");
                  setSubjectSearch("");
                  setIsSubjectDropdownOpen(false);
                }}
                disabled={isLoadingCapabilities || isLoadingGraph}
              >
                {sortedSubjectTypes.map((type) => (
                  <option key={type} value={type}>
                    {formatEntityTypeName(type)}
                  </option>
                ))}
              </select>
            </div>

            {/* Subject search */}
            <div className="form-field graph-subject-field">
              <label htmlFor="subjectSearch">Subject (by name)</label>
              <div className="graph-subject-stack">
                <div className="graph-subject-autocomplete">
                  <input
                    id="subjectSearch"
                    value={subjectSearch}
                    onFocus={() => setIsSubjectDropdownOpen(true)}
                    onBlur={() => setTimeout(() => setIsSubjectDropdownOpen(false), 120)}
                    onChange={(e) => {
                      setSubjectSearch(e.target.value);
                      setSubjectId("");
                      setIsSubjectDropdownOpen(true);
                    }}
                    placeholder="Research and select a subject..."
                    disabled={isLoadingGraph || isLoadingSubjects}
                    autoComplete="off"
                  />

                  {isSubjectDropdownOpen && (
                    <div className="graph-subject-dropdown" role="listbox" aria-label="Suggerimenti soggetto">
                      {filteredSubjectOptions.length > 0 ? (
                        <>
                          {filteredSubjectOptions.slice(0, displayedSubjectCount).map((option) => (
                            <button
                              key={option.id}
                              type="button"
                              className="graph-subject-option"
                              onMouseDown={(e) => e.preventDefault()}
                              onClick={() => pickSubjectOption(option)}
                            >
                              {option.display}
                            </button>
                          ))}
                          {filteredSubjectOptions.length > displayedSubjectCount && (
                            <button
                              type="button"
                              className="graph-subject-load-more"
                              onMouseDown={(e) => e.preventDefault()}
                              onClick={() => setDisplayedSubjectCount((c) => c + 12)}
                            >
                              Load more
                            </button>
                          )}
                        </>
                      ) : (
                        <p className="graph-subject-empty">No results</p>
                      )}
                    </div>
                  )}
                </div>
              </div>
              <div className="graph-subject-meta" aria-live="polite">
                <span>
                  {isLoadingSubjects ? "Loading subjects..." : `${filteredSubjectOptions.length} results`}
                </span>
                <span>
                  {selectedSubjectOption
                    ? `Selected: ${selectedSubjectOption.display}`
                    : "No subject selected"}
                </span>
              </div>
            </div>
          </div>

          {/* Filters */}
          <div className="graph-includes">
            <p>Relationship filters</p>

            <div className="graph-includes-top-row">
              <div className="graph-global-depth-control">
                <label htmlFor="dependencyDepthGlobal">Dependency Depth</label>
                <input
                  id="dependencyDepthGlobal"
                  type="number"
                  min={1}
                  max={maxDependencyDepth}
                  value={dependencyDepth}
                  onChange={(e) =>
                    setDependencyDepth(Math.max(1, Math.min(maxDependencyDepth, Number(e.target.value) || 1)))
                  }
                  disabled={isLoadingGraph}
                />
              </div>

              <div className="graph-filter-toolbar">
                <button
                  type="button"
                  className="graph-filter-select-all"
                  onClick={selectAllIncludes}
                  disabled={isLoadingGraph || availableFilterIds.length === 0}
                >
                  <span
                    className={
                      availableFilterIds.length > 0 && availableFilterIds.every((id) => includeLookup.has(id))
                        ? "graph-action-checkbox is-checked"
                        : "graph-action-checkbox is-empty"
                    }
                    aria-hidden="true"
                  />
                  <span>Check all</span>
                </button>
                <button
                  type="button"
                  className="graph-filter-deselect-all"
                  onClick={deselectAllIncludes}
                  disabled={isLoadingGraph || normalizedIncludes.length === 0}
                >
                  <span className="graph-action-checkbox is-empty" aria-hidden="true" />
                  <span>Deselect all</span>
                </button>
              </div>
            </div>

            <div className="graph-filter-categories">
              {keyedFiltersByCategory.map((category) => {
                const isExpanded = expandedCategoryKeys.has(category.key);
                const categoryFilterIds = category.filters.map((f) => normalizeGraphIncludeId(f.id));
                const areAllCategorySelected =
                  categoryFilterIds.length > 0 && categoryFilterIds.every((id) => includeLookup.has(id));

                return (
                  <section key={category.key} className="graph-filter-category">
                    <div className="graph-filter-category-header">
                      <button
                        type="button"
                        className="graph-filter-category-toggle"
                        onClick={() => toggleCategoryExpanded(category.key)}
                        aria-expanded={isExpanded}
                        aria-controls={`filter-category-panel-${category.key}`}
                      >
                        <span>{formatCategoryLabel(category.category)}</span>
                        <span>{isExpanded ? "-" : "+"}</span>
                      </button>
                      {category.sourceCategories.length === 1 && (
                        <button
                          type="button"
                          className="graph-filter-category-select-all"
                          onClick={() => toggleCategoryIncludes(category)}
                          disabled={isLoadingGraph || category.filters.length === 0}
                        >
                          <span
                            className={areAllCategorySelected ? "graph-action-checkbox is-checked" : "graph-action-checkbox is-empty"}
                            aria-hidden="true"
                          />
                          {areAllCategorySelected ? "Deselect category" : "Check category"}
                        </button>
                      )}
                    </div>

                    {isExpanded && (
                      <div
                        id={`filter-category-panel-${category.key}`}
                        className={
                          category.sourceCategories.length > 1
                            ? "graph-filter-list graph-filter-list-grouped"
                            : "graph-filter-list"
                        }
                      >
                        {category.sourceCategories.map((sourceCategory) => {
                          const sourceCategoryFilterIds = sourceCategory.filters.map((f) =>
                            normalizeGraphIncludeId(f.id)
                          );
                          const areAllSourceSelected =
                            sourceCategoryFilterIds.length > 0 &&
                            sourceCategoryFilterIds.every((id) => includeLookup.has(id));

                          return (
                            <div
                              key={sourceCategory.key}
                              className={
                                category.sourceCategories.length > 1
                                  ? "graph-filter-subcategory"
                                  : "graph-filter-subcategory graph-filter-subcategory-flat"
                              }
                            >
                              {category.sourceCategories.length > 1 && (
                                <div className="graph-filter-subcategory-header">
                                  <h5 className="graph-filter-subcategory-title">
                                    {formatCategoryLabel(sourceCategory.category)}
                                  </h5>
                                  <button
                                    type="button"
                                    className="graph-filter-category-select-all"
                                    onClick={() => toggleFilterIds(sourceCategory.filters.map((f) => f.id))}
                                    disabled={isLoadingGraph || sourceCategory.filters.length === 0}
                                  >
                                    <span
                                      className={areAllSourceSelected ? "graph-action-checkbox is-checked" : "graph-action-checkbox is-empty"}
                                      aria-hidden="true"
                                    />
                                    {areAllSourceSelected ? "Deselect category" : "Check category"}
                                  </button>
                                </div>
                              )}

                              {sourceCategory.filters.map((filter) => {
                                const filterId = normalizeGraphIncludeId(filter.id);
                                const checked = includeLookup.has(filterId);
                                return (
                                  <div key={filterId} className={checked ? "graph-filter-row selected" : "graph-filter-row"}>
                                    <label className="graph-filter-row-main">
                                      <input
                                        type="checkbox"
                                        value={filterId}
                                        checked={checked}
                                        onChange={() => toggleInclude(filterId)}
                                        disabled={isLoadingGraph}
                                      />
                                      <strong className="graph-filter-row-title">
                                        {filter.label || titleize(filterId)}
                                      </strong>
                                    </label>
                                    <button
                                      type="button"
                                      className="graph-filter-info"
                                      aria-label={`Filter info ${filter.label || titleize(filterId)}`}
                                      data-tooltip={filter.description || "No description available."}
                                    >
                                      i
                                    </button>
                                  </div>
                                );
                              })}
                            </div>
                          );
                        })}
                      </div>
                    )}
                  </section>
                );
              })}
            </div>

            {!hasSelectedDepthFilter && (
              <small className="graph-depth-note">
                Dependency Depth is global for the query. It is only used by filters that support depth.
              </small>
            )}
          </div>

          <div className="form-actions graph-query-actions">
            <button
              type="button"
              className="secondary"
              onClick={clearResults}
              disabled={!graph || isLoadingGraph}
            >
              Clear result
            </button>
            <button
              type="submit"
              className="primary"
              disabled={isLoadingGraph || isLoadingCapabilities || isLoadingSubjectCapabilities}
            >
              {isLoadingGraph ? "Executing query..." : "Execute query"}
            </button>
          </div>
        </form>
      </section>

      {error && <div className="alert alert-error"><span>{error}</span></div>}

      {graph && (
        <section className="card graph-results-card">
          <div className="graph-results-header">
            <div>
              <h3>Graph result</h3>
              <p>
                Subject: <strong>{titleize(graph.subjectType)}</strong> /{" "}
                <strong>{graphSubjectLabel}</strong>
              </p>
            </div>
            <div className="graph-badges">
              <span className="badge badge-info">{graph.nodes.length} nodes</span>
              <span className="badge badge-neutral">{graph.edges.length} edges</span>
            </div>
          </div>

          {nextSubjectCandidates.length > 0 && (
            <div className="graph-next-subject-panel">
              <p className="graph-next-subject-title">Use a node as the subject for the next query</p>
              <div className="graph-next-subject-list">
                {nextSubjectCandidates.map((node) => {
                  const crudRoute = getCrudRoutePath(node.type);
                  return (
                    <button
                      key={`${node.type}-${node.id}`}
                      type="button"
                      className="graph-next-subject-button"
                      style={buildNextSubjectChipStyle(node.type)}
                      onClick={() => useNodeAsNextSubject(node)}
                      disabled={!node.isSelectable || isLoadingGraph || isLoadingSubjects || isLoadingSubjectCapabilities}
                      title={
                        node.isSelectable
                          ? `Set ${node.label} as subject`
                          : `Type ${titleize(node.type)} not available as subject`
                      }
                    >
                      <span className="graph-next-subject-content">
                        <span className="graph-next-subject-label">{node.label}</span>
                        <span className="graph-next-subject-meta">{getEntityTypeAlias(node.type)}</span>
                      </span>
                      {crudRoute && (
                        <button
                          type="button"
                          className="graph-next-subject-redirect-btn"
                          onClick={(e) => {
                            e.stopPropagation();
                            e.preventDefault();
                            window.sessionStorage.setItem(`crud:selected-row:${crudRoute}`, node.id);
                            navigate(crudRoute);
                          }}
                          title={`Open in ${titleize(node.type)} list`}
                          aria-label={`Open ${node.label} in CRUD`}
                        >
                          <span className="graph-next-subject-redirect">↗</span>
                        </button>
                      )}
                    </button>
                  );
                })}
              </div>
            </div>
          )}

          <div className="graph-columns">
            <div className="graph-column card graph-visual-column">
              <div className="graph-visual-header">
                <h4>Mermaid Chart</h4>
                <div className="graph-visual-actions">
                  <button type="button" onClick={downloadMermaidSvg} disabled={!mermaidSvg}>Download SVG</button>
                  <button type="button" onClick={downloadMermaidPng} disabled={!mermaidSvg}>Download PNG</button>
                  <button type="button" onClick={() => setShowMermaidSource((v) => !v)} disabled={!mermaidDefinition}>
                    {showMermaidSource ? "Hide Mermaid" : "Show Mermaid"}
                  </button>
                  <button type="button" onClick={() => setShowJson((v) => !v)} disabled={!graph}>
                    {showJson ? "Hide JSON" : "Show JSON"}
                  </button>
                </div>
              </div>
              <p className="graph-column-note">Full view of the query result.</p>
              {mermaidError && <div className="alert alert-error">{mermaidError}</div>}
              {hasGraphData ? (
                <div className="graph-mermaid-canvas" ref={mermaidContainerRef} />
              ) : (
                <div className="alert badge-neutral">No nodes available for the diagram.</div>
              )}
              {showMermaidSource && mermaidDefinition && (
                <div className="graph-mermaid-source"><pre>{mermaidDefinition}</pre></div>
              )}
              {showJson && (
                <div className="graph-json-panel"><pre>{JSON.stringify(graph, null, 2)}</pre></div>
              )}
            </div>
          </div>
        </section>
      )}
    </div>
  );
}