import { useNavigate } from "react-router-dom";
import CrudPage from "../components/CrudPage";

export default function LoginTypesPage() {
  const navigate = useNavigate();

  return (
    <CrudPage
      title="Login Types"
      endpoint="/api/login-types"
      fields={[
        { name: "name", label: "Name", required: true },
        { name: "protocol", label: "Protocol", required: true },
        { name: "mfa", label: "MFA Enabled", type: "checkbox", pinned: true },
      ]}
      extraActions={(item) => (
        <>
          <button onClick={() => navigate(`/login-types/${item.id}/used-by-assets`)}>Manage the Assets that use this Login Type</button>
        </>
      )}
    />
  );
}
