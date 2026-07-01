import { BarChart3, Compass, Package, Server, Building2, Monitor, Cloud, FileText, Store, Lock, Settings, Workflow, Shield } from "lucide-react";
import type { LucideIcon } from "lucide-react";

export const ENTITY_ICONS: Record<string, LucideIcon> = {
  dashboard: BarChart3,
  graphExplorer: Compass,
  assets: Package,
  services: Server,
  divisions: Building2,
  virtualMachines: Monitor,
  cloudProviders: Cloud,
  contracts: FileText,
  suppliers: Store,
  loginTypes: Lock,
  settings: Settings,
  processes: Workflow,
  acnMacroAreas: Shield
} as const;
