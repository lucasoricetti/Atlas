import { useNavigate } from "react-router-dom";
import CrudPage from "../components/CrudPage";

export default function DivisionsPage() {
  const navigate = useNavigate();

  return (
    <CrudPage
      title="Divisions"
      endpoint="/api/divisions"
      fields={[
        { name: "name", label: "Name", required: true }
      ]}
      extraActions={(item) => (
        <>
          <button onClick={() => navigate(`/divisions/${item.id}/uses-assets`)}>
            Manage the Assets used by this Division
          </button>
          <button onClick={() => navigate(`/divisions/${item.id}/owns-assets`)}>
            Manage the Assets owned by this Division
          </button>
          <button onClick={() => navigate(`/divisions/${item.id}/uses-processes`)}>
            Manage the Processes used by this Division
          </button>
          <button onClick={() => navigate(`/divisions/${item.id}/owns-processes`)}>
            Manage the Processes owned by this Division
          </button>
        </>
      )}
    />
  );
}