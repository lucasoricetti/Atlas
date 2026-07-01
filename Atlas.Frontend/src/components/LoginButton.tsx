import { useMsal } from "@azure/msal-react";
import { loginRequest } from "../auth/msalConfig";

interface Props {
  className?: string;
  label?: string;
}

export const LoginButton = ({ className = "", label = "Login" }: Props) => {
  const { instance } = useMsal();

  const handleLogin = () => {
    void instance.loginRedirect(loginRequest);
  };

  return (
    <button type="button" className={className} onClick={handleLogin}>
      {label}
    </button>
  );
};