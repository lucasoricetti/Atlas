import { useNavigate } from "react-router-dom";
import CrudPage from "../components/CrudPage";

export default function ProcessesPage() {
  const navigate = useNavigate();

  return (
    <CrudPage
      title="Processes"
      endpoint="/api/processes"
      fields={[
        { name: "name", label: "Name", required: true },
        { name: "description", label: "Description", type: "textarea", displayInMainSection: true },
      ]}
      extraActions={(item) => (
        <>
          <button onClick={() => navigate(`/processes/${item.id}/involves-assets`)}>
            Manage the Assets involved in this Process
          </button>
          <button onClick={() => navigate(`/processes/${item.id}/involves-services`)}>
            Manage the Services involved in this Process
          </button>
          <button onClick={() => navigate(`/processes/${item.id}/has-settings`)}>
            Manage the Settings associated with this Process
          </button>
          <button onClick={() => navigate(`/processes/${item.id}/owned-by-divisions`)}>
            Manage the Divisions that own this Process
          </button>
          <button onClick={() => navigate(`/processes/${item.id}/used-by-divisions`)}>
            Manage the Divisions that use this Process
          </button>
          <button onClick={() => navigate(`/processes/${item.id}/classified-as`)}>
            Manage the ACN Macro Area that classify this Process
          </button>
        </>
      )}
    />
  );
}