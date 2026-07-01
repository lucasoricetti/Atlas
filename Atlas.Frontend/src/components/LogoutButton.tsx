import { useMsal } from "@azure/msal-react";
import { LogOut } from "lucide-react";
import { clearReturnTo } from "../auth/authState";
import { syncActiveAccount } from "../auth/msalInstance";

interface Props {
  className?: string;
  label?: string;
  iconOnly?: boolean;
}

export default function LogoutButton({ className = "", label = "Logout", iconOnly = false }: Props) {
  const { instance } = useMsal();

  const handleLogout = () => {
    clearReturnTo();
    const account = syncActiveAccount();

    void instance.logoutRedirect({
      account: account ?? undefined,
      postLogoutRedirectUri: `${window.location.origin}/login`
    });
  };

  return (
    <button
      type="button"
      className={[className, iconOnly ? "logout-button-icon-only" : ""].filter(Boolean).join(" ")}
      onClick={handleLogout}
      aria-label={iconOnly ? label : undefined}
      title={iconOnly ? label : undefined}
    >
      {iconOnly ? <LogOut size={16} aria-hidden="true" /> : label}
    </button>
  );
}