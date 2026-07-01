import { useNavigate } from "react-router-dom";
import CrudPage from "../components/CrudPage";

export default function ContractsPage() {
  const navigate = useNavigate();

  return (
    <CrudPage
      title="Contracts"
      endpoint="/api/contracts"
      fields={[
        { name: "name", label: "Name", required: true },
        { name: "contractTypes", label: "Contract Types", required: true, type: "multiselect", optionsKey: "contractTypes" },
        { name: "sla", label: "SLA (in Hours)", type: "text", numberNotText: true },
        { name: "contactEmail", label: "Email" },
        { name: "contactPhone", label: "Phone" },
        { name: "startDate", label: "Start Date", type: "date" },
        { name: "endDate", label: "End Date", type: "date" },
        { name: "notes", label: "Notes", type: "textarea" }
      ]}
      extraActions={(item) => (
        <>
          <button onClick={() => navigate(`/contracts/${item.id}/provided-by-supplier`)}>
            Manage Suppliers that provide this Contract
          </button>
          <button onClick={() => navigate(`/contracts/${item.id}/used-by-services`)}>
            Manage Services that use this Contract
          </button>
        </>
      )}
    />
  );
}
