import { NavLink } from "react-router-dom";
import type { ReactNode } from "react";
import { useEffect, useState } from "react";
import type { LucideIcon } from "lucide-react";
import { Outlet } from "react-router-dom";
import { useMsal } from "@azure/msal-react";
import { ENTITY_ICONS } from "../constants/entityIcons";
import EntityIcon from "./EntityIcon";
import LogoutButton from "./LogoutButton";

type NavItem = {
  to: string;
  label: string;
  icon: LucideIcon;
};

const PRIMARY_NAV_ITEMS: NavItem[] = [
  { to: "/", label: "Dashboard", icon: ENTITY_ICONS.dashboard },
  { to: "/graph-explorer", label: "Graph Explorer", icon: ENTITY_ICONS.graphExplorer },
  { to: "/assets", label: "Assets", icon: ENTITY_ICONS.assets },
  { to: "/services", label: "Services", icon: ENTITY_ICONS.services },
  { to: "/processes", label: "Processes", icon: ENTITY_ICONS.processes },
  { to: "/virtual-machines", label: "Virtual Machines", icon: ENTITY_ICONS.virtualMachines },
  { to: "/contracts", label: "Contracts", icon: ENTITY_ICONS.contracts },
  { to: "/suppliers", label: "Suppliers", icon: ENTITY_ICONS.suppliers }
];

const SECONDARY_NAV_ITEMS: NavItem[] = [
  { to: "/cloud-providers", label: "Cloud Providers", icon: ENTITY_ICONS.cloudProviders },
  { to: "/divisions", label: "Divisions", icon: ENTITY_ICONS.divisions },
  { to: "/acn-macro-areas", label: "ACN Macro Areas", icon: ENTITY_ICONS.acnMacroAreas },
  { to: "/login-types", label: "Login Types", icon: ENTITY_ICONS.loginTypes },
  { to: "/settings", label: "Settings", icon: ENTITY_ICONS.settings }
];

export default function Layout({ children }: { children?: ReactNode }) {
  const { accounts, instance } = useMsal();
  const cls = (p: { isActive: boolean }) =>
    p.isActive ? "sidebar-link active" : "sidebar-link";

  const [isCollapsed, setIsCollapsed] = useState(false);
  const [isMobile, setIsMobile] = useState(false);
  const [isMobileOpen, setIsMobileOpen] = useState(false);
  const [isSecondaryExpanded, setIsSecondaryExpanded] = useState(false);

  useEffect(() => {
    const currentAccount = instance.getActiveAccount();

    if (!currentAccount && accounts[0]) {
      instance.setActiveAccount(accounts[0]);
    }
  }, [accounts, instance]);

  const activeAccount = accounts[0] ?? instance.getActiveAccount();

  useEffect(() => {
    const mediaQuery = window.matchMedia("(max-width: 900px)");

    const handleMediaChange = (event: MediaQueryListEvent | MediaQueryList) => {
      const mobileState = event.matches;
      setIsMobile(mobileState);

      if (mobileState) {
        setIsCollapsed(false);
      } else {
        setIsMobileOpen(false);
      }
    };

    handleMediaChange(mediaQuery);

    const listener = (event: MediaQueryListEvent) => handleMediaChange(event);
    mediaQuery.addEventListener("change", listener);

    return () => mediaQuery.removeEventListener("change", listener);
  }, []);

  const shellClassName = [
    "app-shell",
    isCollapsed ? "sidebar-collapsed" : "",
    isMobileOpen ? "sidebar-open" : ""
  ]
    .filter(Boolean)
    .join(" ");

  const toggleSidebar = () => {
    if (isMobile) {
      setIsMobileOpen(current => !current);
      return;
    }

    setIsCollapsed(current => !current);
  };

  const closeMobileSidebar = () => {
    if (isMobile) {
      setIsMobileOpen(false);
    }
  };

  return (
    <div className={shellClassName}>
      <button
        type="button"
        className="mobile-nav-toggle"
        onClick={toggleSidebar}
        aria-label={isMobileOpen ? "Close navigation menu" : "Open navigation menu"}
      >
        <span className="hamburger" aria-hidden="true">
          <span />
          <span />
          <span />
        </span>
        <span>Menu</span>
      </button>

      <aside className="sidebar">
        <div className="sidebar-top">
          <div className="brand">
            <div className="brand-header">
              <img src="/atlas_logo_extended_blue.png" alt="Atlas logo" className="brand-logo" />
            </div>
          </div>

          <div className="sidebar-top-actions">
            {!isMobile && (
              <button
                type="button"
                className="sidebar-collapse-btn sidebar-collapse-btn-inline"
                onClick={toggleSidebar}
                aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
              >
                <span aria-hidden="true" className={isCollapsed ? "sidebar-expand" : "sidebar-collapse"} />
              </button>
            )}

            {isMobile && (
              <button
                type="button"
                className="sidebar-icon-btn"
                onClick={toggleSidebar}
                aria-label="Close menu"
              >
                <span aria-hidden="true" className="sidebar-close" />
              </button>
            )}
          </div>
        </div>

        <div className="sidebar-user-card">
          <div className="sidebar-user-avatar" aria-hidden="true">
            {(activeAccount?.name ?? activeAccount?.username ?? "A").slice(0, 1).toUpperCase()}
          </div>
          <div className="sidebar-user-meta">
            <strong>{activeAccount?.name ?? "Signed in"}</strong>
            <span>{activeAccount?.username ?? "Microsoft account"}</span>
          </div>
          <LogoutButton
            className="sidebar-logout-button"
            label="Logout"
            iconOnly={isCollapsed && !isMobile}
          />
        </div>

        <nav className="sidebar-nav">
          {PRIMARY_NAV_ITEMS.map(item => (
            <NavLink key={item.to} to={item.to} className={cls} onClick={closeMobileSidebar}>
              <span className="sidebar-link-glyph" aria-hidden="true">
                <EntityIcon icon={item.icon} size={20} />
              </span>
              <span className="sidebar-link-text">{item.label}</span>
            </NavLink>
          ))}

          <div className="sidebar-section">
            <button
              type="button"
              className="sidebar-section-toggle"
              onClick={() => setIsSecondaryExpanded(!isSecondaryExpanded)}
              aria-expanded={isSecondaryExpanded}
            >
              <span className="sidebar-link-glyph" aria-hidden="true">
                <span className={isSecondaryExpanded ? "sidebar-section-collapse" : "sidebar-section-expand"} />
              </span>
              <span className="sidebar-link-text">Others</span>
            </button>

            {isSecondaryExpanded && (
              <div className="sidebar-section-items">
                {SECONDARY_NAV_ITEMS.map(item => (
                  <NavLink key={item.to} to={item.to} className={cls} onClick={closeMobileSidebar}>
                    <span className="sidebar-link-glyph" aria-hidden="true">
                      <EntityIcon icon={item.icon} size={20} />
                    </span>
                    <span className="sidebar-link-text">{item.label}</span>
                  </NavLink>
                ))}
              </div>
            )}
          </div>
        </nav>
      </aside>

      {isMobileOpen && (
        <button
          type="button"
          className="sidebar-backdrop"
          aria-label="Close menu"
          onClick={closeMobileSidebar}
        />
      )}

      <main className="main-content">{children ?? <Outlet />}</main>
    </div>
  );
}