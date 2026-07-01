import { useEffect, useMemo } from "react";
import { useMsal } from "@azure/msal-react";
import { InteractionStatus } from "@azure/msal-browser";
import { useLocation, useNavigate } from "react-router-dom";
import { clearReturnTo, readReturnTo } from "../auth/authState";
import { syncActiveAccount } from "../auth/msalInstance";
import { LoginButton } from "../components/LoginButton";

const getAccountLabel = (name?: string, username?: string) => name || username || "Atlas user";

export default function LoginPage() {
  const { accounts, inProgress, instance } = useMsal();
  const navigate = useNavigate();
  const location = useLocation();

  const returnTo = useMemo(() => readReturnTo(), []);
  const activeAccount = accounts[0] ?? instance.getActiveAccount() ?? null;
  const activeAccountId = activeAccount?.homeAccountId ?? "";

  useEffect(() => {
    syncActiveAccount();
  }, [accounts, instance]);

  useEffect(() => {
    if (activeAccountId && inProgress === InteractionStatus.None) {
      const currentPath = `${location.pathname}${location.search}${location.hash}`;
      clearReturnTo();

      if (currentPath !== returnTo) {
        navigate(returnTo, { replace: true });
      }
    }
  }, [activeAccountId, inProgress, location.hash, location.pathname, location.search, navigate, returnTo]);

  return (
    <main className="auth-shell">
      <section className="auth-card">
        <div className="auth-badge">Atlas</div>
        <h1>Sign in to continue</h1>
        <p>
          Access the governance dashboard, graph explorer and entity registries with your Microsoft account.
        </p>

        <div className="auth-points" aria-label="Login benefits">
          <span>Single sign-on</span>
          <span>Protected API access</span>
          <span>Session-aware routing</span>
        </div>

        <div className="auth-actions">
          <LoginButton className="primary auth-login-button" label="Sign in with Microsoft" />
        </div>

        <p className="auth-fineprint">
          {activeAccount
            ? `Signed in as ${getAccountLabel(activeAccount.name, activeAccount.username)}.`
            : "Use your corporate account to authenticate."}
        </p>
      </section>
    </main>
  );
}