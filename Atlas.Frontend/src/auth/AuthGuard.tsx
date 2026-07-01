import { useMsal } from "@azure/msal-react";
import { useEffect } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { API_SCOPES } from "./msalConfig";

const REQUIRED_ROLES = ["Editor", "Reader"];

export function AuthGuard() {
  const { instance, accounts } = useMsal();
  const navigate = useNavigate();
  const location = useLocation();

  useEffect(() => {
    const account =
      instance.getActiveAccount() ??
      accounts[0];

    if (!account) return;

    // Evita loop sulla pagina di access denied
    if (location.pathname === "/access-denied") return;

    const checkRoles = async () => {
      try {
        const result = await instance.acquireTokenSilent({
          scopes: API_SCOPES,
          account
        });

        const accessToken = result.accessToken;
        const payload = JSON.parse(atob(accessToken.split(".")[1]));
        const roles: string[] = payload?.roles ?? [];

        const isAuthorized = REQUIRED_ROLES.some(role =>
          roles.includes(role)
        );

        if (!isAuthorized) {
          navigate("/access-denied", { replace: true });
        }
      } catch (error) {
        // Se non riesci nemmeno a ottenere il token → non autorizzato
        navigate("/access-denied", { replace: true });
      }
    };

    void checkRoles();
  }, [instance, accounts, location.pathname, navigate]);

  return null;
}