import { useNavigate } from "react-router-dom";
import CrudPage from "../components/CrudPage";

export default function VirtualMachinesPage() {
  const navigate = useNavigate();

  return (
    <CrudPage
      title="Virtual Machines"
      endpoint="/api/virtual-machines"
      fields={[
        { name: "name", label: "Name", required: true },
        { name: "type", label: "Type", required: true, type: "select", optionsKey: "virtualMachineTypes" },
        { name: "ip", label: "IP Address", pinned: true, displayInMainSection: true },
        { name: "cluster", label: "Cluster" },
        { name: "role", label: "Role" }
      ]}
      extraActions={(item) => (
        <>
          <button onClick={() => navigate(`/virtual-machines/${item.id}/hosts-services`)}>
            Manage the Services hosted by this Virtual Machine
          </button>
        </>
      )}
    />
  );
}
