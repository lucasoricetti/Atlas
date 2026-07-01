import { useNavigate } from "react-router-dom";
import CrudPage from "../components/CrudPage";

export default function CloudProvidersPage() {
  const navigate = useNavigate();

  return (
    <CrudPage
      title="Cloud Providers"
      endpoint="/api/cloud-providers"
      fields={[
        { name: "name", label: "Name", required: true },
        { name: "type", label: "Type", required: true, type: "select", optionsKey: "cloudProviderTypes" },
        { name: "portalUrl", label: "Portal URL", type: "url" },
        { name: "account", label: "Account" }
      ]}
      extraActions={(item) => (
        <>
          <button onClick={() => navigate(`/cloud-providers/${item.id}/hosts-services`)}>
            Manage the Services hosted by this Cloud Provider
          </button>
        </>
      )}
    />
  );
}