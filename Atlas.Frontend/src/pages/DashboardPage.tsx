import { useCallback, useEffect, useMemo, useState } from "react";
import { Link } from "react-router-dom";
import type { LucideIcon } from "lucide-react";
import api from "../api/client";
import { ENTITY_ICONS } from "../constants/entityIcons";
import EntityIcon from "../components/EntityIcon";

type DashboardMetric = {
  key: string;
  emoji: LucideIcon;
  label: string;
  endpoint: string;
  href: string;
};

type DashboardSection = {
  id: string;
  title: string;
  description: string;
  metrics: DashboardMetric[];
};

type DashboardCacheState = {
  stats: Record<string, number>;
  lastUpdatedAt: string | null;
};

let dashboardCache: DashboardCacheState | null = null;

const DASHBOARD_SECTIONS: DashboardSection[] = [
  {
    id: "core",
    title: "Core Registries",
    description: "Overview of the main entities managed by the system.",
    metrics: [
      {
        key: "assets",
        emoji: ENTITY_ICONS.assets,
        label: "Assets",
        endpoint: "/api/assets",
        href: "/assets"
      },
      {
        key: "services",
        emoji: ENTITY_ICONS.services,
        label: "Services",
        endpoint: "/api/services",
        href: "/services"
      },
      {
        key: "processes",
        emoji: ENTITY_ICONS.processes,
        label: "Processes",
        endpoint: "/api/processes",
        href: "/processes"
      }
    ]
  },
  {
    id: "organization",
    title: "Organization",
    description: "Organizational structure and macro-area breakdown.",
    metrics: [
      {
        key: "divisions",
        emoji: ENTITY_ICONS.divisions,
        label: "Divisions",
        endpoint: "/api/divisions",
        href: "/divisions"
      },
      {
        key: "acnMacroAreas",
        emoji: ENTITY_ICONS.acnMacroAreas,
        label: "ACN Macro Areas",
        endpoint: "/api/acn-macro-areas",
        href: "/acn-macro-areas"
      }
    ]
  },
  {
    id: "infrastructure",
    title: "Infrastructure",
    description: "Infrastructure components linked to business services.",
    metrics: [
      {
        key: "virtualMachines",
        emoji: ENTITY_ICONS.virtualMachines,
        label: "Virtual Machines",
        endpoint: "/api/virtual-machines",
        href: "/virtual-machines"
      },
      {
        key: "cloudProviders",
        emoji: ENTITY_ICONS.cloudProviders,
        label: "Cloud Providers",
        endpoint: "/api/cloud-providers",
        href: "/cloud-providers"
      }
    ]
  },
  {
    id: "atlas",
    title: "Atlas Control Center",
    description: "Cross-cutting elements for security, contracts and configurations.",
    metrics: [
      {
        key: "contracts",
        emoji: ENTITY_ICONS.contracts,
        label: "Contracts",
        endpoint: "/api/contracts",
        href: "/contracts"
      },
      {
        key: "suppliers",
        emoji: ENTITY_ICONS.suppliers,
        label: "Suppliers",
        endpoint: "/api/suppliers",
        href: "/suppliers"
      },
      {
        key: "loginTypes",
        emoji: ENTITY_ICONS.loginTypes,
        label: "Login Types",
        endpoint: "/api/login-types",
        href: "/login-types"
      },
      {
        key: "settings",
        emoji: ENTITY_ICONS.settings,
        label: "Settings",
        endpoint: "/api/settings",
        href: "/settings"
      }
    ]
  }
];

export default function DashboardPage() {
  const [stats, setStats] = useState<Record<string, number>>(() => dashboardCache?.stats ?? {});
  const [expandedSections, setExpandedSections] = useState<string[]>(() =>
    DASHBOARD_SECTIONS.map(section => section.id)
  );
  const [isLoading, setIsLoading] = useState(() => dashboardCache === null);
  const [error, setError] = useState<string | null>(null);
  const [lastUpdatedAt, setLastUpdatedAt] = useState<string | null>(() => dashboardCache?.lastUpdatedAt ?? null);

  const allMetrics = useMemo(
    () => DASHBOARD_SECTIONS.flatMap(section => section.metrics),
    []
  );

  const toggleSection = (sectionId: string) => {
    setExpandedSections(current =>
      current.includes(sectionId)
        ? current.filter(id => id !== sectionId)
        : [...current, sectionId]
    );
  };

  const refreshData = useCallback(async () => {
    setIsLoading(true);
    setError(null);

    try {
      const responses = await Promise.allSettled(
        allMetrics.map(metric => api.get(metric.endpoint))
      );

      const nextStats = Object.fromEntries(
        allMetrics.map((metric, index) => {
          const response = responses[index];
          const data = response.status === "fulfilled" ? response.value.data : [];
          const count = Array.isArray(data)
            ? data.length
            : typeof data?.count === "number"
              ? data.count
              : 0;

          return [metric.key, count];
        })
      );

      setStats(nextStats);
      const nextUpdatedAt = new Date().toLocaleTimeString("en-US");
      setLastUpdatedAt(nextUpdatedAt);
      dashboardCache = {
        stats: nextStats,
        lastUpdatedAt: nextUpdatedAt
      };

      const failedRequests = responses.filter(
        response => response.status === "rejected"
      );

      if (failedRequests.length > 0) {
        setError(
          `Some metrics are not available (${failedRequests.length}/${responses.length}).`
        );
      }
    } catch (err: unknown) {
      const message =
        typeof err === "object" && err !== null && "friendlyMessage" in err
          ? String((err as { friendlyMessage?: string }).friendlyMessage)
          : "Unable to load the dashboard. Try again in a moment.";

      setError(message);
    } finally {
      setIsLoading(false);
    }
  }, [allMetrics]);

  const totalObjects = useMemo(
    () => Object.values(stats).reduce((sum, value) => sum + value, 0),
    [stats]
  );

  useEffect(() => {
    refreshData();
  }, [refreshData]);

  return (
    <div className="page dashboard-page">
      <div className="page-header">
        <div>
          <h2 className="page-title-with-icon">
            <EntityIcon icon={ENTITY_ICONS.dashboard} size={24} className="page-title-icon" />
            <span>Dashboard</span>
          </h2>
          <p>Quick and navigable view of the Atlas platform.</p>
        </div>
      </div>

      <section className="dashboard-hero card" aria-live="polite">
        <div>
          <p className="dashboard-kicker">Real-time snapshot</p>
          <h3>{totalObjects} registered elements</h3>
          <p className="dashboard-hero-text">
            Monitor registries, infrastructure and compliance from a single point
            and open only the section you need.
          </p>
        </div>
        <div className="dashboard-meta">
          <span>{lastUpdatedAt ? `Updated at ${lastUpdatedAt}` : "Loading"}</span>
          <div className="dashboard-quick-links">
            <Link to="/graph-explorer">Open Graph Explorer</Link>
          </div>
        </div>
      </section>

      {error && (
        <div className="alert alert-error">
          <span>{error}</span>
          <button type="button" onClick={refreshData}>Retry</button>
        </div>
      )}

      {isLoading && Object.keys(stats).length === 0 ? (
        <div className="stats-grid" aria-hidden="true">
          {Array.from({ length: 6 }).map((_, index) => (
            <div key={index} className="stat-card stat-card-skeleton" />
          ))}
        </div>
      ) : (
        <div className="dashboard-sections">
          {DASHBOARD_SECTIONS.map(section => {
            const isExpanded = expandedSections.includes(section.id);

            return (
              <section key={section.id} className="dashboard-section card">
                <button
                  type="button"
                  className="dashboard-section-toggle"
                  onClick={() => toggleSection(section.id)}
                  aria-expanded={isExpanded}
                >
                  <div>
                    <h3>{section.title}</h3>
                    <p>{section.description}</p>
                  </div>
                  <span className="dashboard-toggle-indicator">
                    {isExpanded ? "Hide" : "Expand"}
                  </span>
                </button>

                {isExpanded && (
                  <div className="stats-grid">
                    {section.metrics.map(metric => (
                      <Link key={metric.key} to={metric.href} className="dashboard-stat-link">
                        <article className="stat-card">
                          <h3 className="dashboard-metric-title">
                            <EntityIcon icon={metric.emoji} size={20} className="dashboard-metric-emoji" />
                            <span>{metric.label}</span>
                          </h3>
                          <strong>{stats[metric.key] ?? 0}</strong>
                        </article>
                      </Link>
                    ))}
                  </div>
                )}
              </section>
            );
          })}
        </div>
      )}
      </div>
  );
}