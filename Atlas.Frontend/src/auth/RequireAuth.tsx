import { useEffect } from "react";
import { useMsal } from "@azure/msal-react";
import { InteractionStatus } from "@azure/msal-browser";
import type { ReactNode } from "react";
import { Navigate, Outlet, useLocation } from "react-router-dom";
import { storeReturnTo } from "./authState";
import { syncActiveAccount } from "./msalInstance";

interface Props {
  children?: ReactNode;
}

export default function RequireAuth({ children }: Props) {
  const { accounts, inProgress, instance } = useMsal();
  const location = useLocation();
  const activeAccount = accounts[0] ?? instance.getActiveAccount() ?? null;

  useEffect(() => {
    syncActiveAccount();
  }, [accounts, instance]);

  if (inProgress !== InteractionStatus.None) {
    return <div className="auth-shell auth-shell-loading">Checking session...</div>;
  }

  if (!activeAccount) {
    storeReturnTo(`${location.pathname}${location.search}${location.hash}`);
    return <Navigate to="/login" replace />;
  }

  return children ?? <Outlet />;
}