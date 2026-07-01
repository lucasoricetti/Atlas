import { useNavigate } from "react-router-dom";
import CrudPage from "../components/CrudPage";

export default function SuppliersPage() {
  const navigate = useNavigate();

  return (
    <CrudPage
      title="Suppliers"
      endpoint="/api/suppliers"
      fields={[
        { name: "name", label: "Name", required: true }
      ]}
      extraActions={(item) => (
        <>
          <button onClick={() => navigate(`/suppliers/${item.id}/provides-contracts`)}>
            Manage Contracts provided by this Supplier
          </button>
        </>
      )}
    />
  );
}
