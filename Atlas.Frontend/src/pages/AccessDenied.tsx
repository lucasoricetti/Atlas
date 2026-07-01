
import { ShieldAlert } from "lucide-react";
import LogoutButton from "../components/LogoutButton";

export default function AccessDenied() {
  return (
    <main className="auth-shell auth-shell-denied">
      <section className="auth-card auth-card-denied">
        <div className="auth-badge auth-badge-danger">Atlas</div>

        <div className="auth-hero-icon auth-hero-icon-danger" aria-hidden="true">
          <ShieldAlert size={34} />
        </div>

        <h1>Access denied</h1>
        <p>
          You have signed in successfully, but this account does not have a
          role allowed to use the Atlas application.
        </p>

        <div className="auth-points auth-points-danger" aria-label="Access status details">
          <span>Signed in</span>
          <span>Role missing</span>
          <span>Admin review needed</span>
        </div>

        <div className="auth-warning-box">
          <strong>What this means</strong>
          <p>
            Your Microsoft Entra ID account is valid, but it has not been
            granted one of the required application roles.
          </p>
        </div>

        <p className="auth-fineprint auth-fineprint-strong">
          If you think this is a mistake, contact your system administrator and
          request access for this application.
        </p>

        <div className="auth-actions auth-actions-stacked">
          <LogoutButton className="primary auth-login-button" label="Sign out" />
        </div>
      </section>
    </main>
  );
}
