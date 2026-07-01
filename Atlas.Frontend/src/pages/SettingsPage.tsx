import { useNavigate } from "react-router-dom";
import CrudPage from "../components/CrudPage";

export default function SettingsPage() {
  const navigate = useNavigate();

  return (
    <CrudPage
      title="Settings"
      endpoint="/api/settings"
      fields={[
        { name: "name",        label: "Name",        required: true },
        { name: "links",       label: "Links",       required: true, type: "url-array" },
        { name: "description", label: "Description", type: "textarea" }
      ]}
      extraActions={(item) => (
        <>
          <button onClick={() => navigate(`/settings/${item.id}/used-by-assets`)}>
            Manage the Assets that use this Setting
          </button>
          <button onClick={() => navigate(`/settings/${item.id}/used-by-services`)}>
            Manage the Services that use this Setting
          </button>
          <button onClick={() => navigate(`/settings/${item.id}/used-by-processes`)}>
            Manage the Processes that use this Setting
          </button>
        </>
      )}
    />
  );
}