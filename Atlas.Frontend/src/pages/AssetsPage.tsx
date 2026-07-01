import { useNavigate } from "react-router-dom";
import CrudPage from "../components/CrudPage";

export default function AssetsPage() {
  const navigate = useNavigate();

  return (
    <CrudPage
      title="Assets"
      endpoint="/api/assets"
      headerInfo={(
                    <>
                      <h4>Classification Rules</h4>
                      <p>
                        <strong>Rule 1 - Included in BIA:</strong> If an application is explicitly included in the Business Impact
                        Analysis (BIA), then it must be classified as an Asset.
                      </p>
                      <p>
                        <strong>Rule 2 - Dependency on multiple Services on VM:</strong> If an application depends on multiple distinct
                        Services running on one or more VMs, then the application must be classified as an Asset.
                      </p>
                      <p>
                        <strong>Rule 3 - Single application on VM:</strong> If the application is monolithic, autonomous, and runs as
                        the only workload on a dedicated VM, then it may be classified as a Service.
                      </p>
                      <p>
                        <strong>Rule 4 - Service Composition Chain:</strong> An Asset may be composed of one or more Services
                        (via <code>COMPOSED_BY</code>), which in turn may depend on other Services in a chain
                        (via <code>DEPENDS_ON</code>). Each Service in the chain represents a distinct technical
                        component (e.g., an application instance, a database instance, a database engine). Contracts
                        and Suppliers are associated only with the terminal Service in the chain — the underlying
                        infrastructure component (e.g., the database engine such as SQL Server), not with
                        intermediate or application-level Services. Example:{" "}
                        <em>ERP Asset → COMPOSED_BY → DB Instance (Service) → DEPENDS_ON → SQL Server (Service)</em>;
                        the Contract and Supplier are linked to SQL Server, not to the DB Instance or the ERP Asset.
                      </p>

                      <h4>Precedence Rule</h4>
                      <p>
                        In case of ambiguity or multiple conditions, classification as <strong>Asset</strong> always prevails.
                      </p>
                      <p>
                        Rationale: a "risk-based and conservative" approach; any element that may introduce systemic risk must be
                        governed as an Asset.
                      </p>
                    </>
                  )}
      fields={[
        { name: "name", label: "Name", required: true },
        { name: "type", label: "Type", required: true, type: "select", optionsKey: "assetTypes" },
        { name: "criticality", label: "Criticality", required: true, type: "select", optionsKey: "criticalities" },
        { name: "bia", label: "BIA", type: "checkbox" },
        { name: "rpoH", label: "RPO (in Hours)", type: "text", numberNotText: true },
        { name: "mtoH", label: "MTO (in Hours)", type: "text", numberNotText: true },
        { name: "description", label: "Description", type: "textarea" }
      ]}
      extraActions={(item) => (
        <>
          <button onClick={() => navigate(`/assets/${item.id}/asset-dependencies`)}>Manage the Assets that this Asset depends on</button>
          <button onClick={() => navigate(`/assets/${item.id}/service-dependencies`)}>Manage the Services that compose this Asset</button>
          <button onClick={() => navigate(`/assets/${item.id}/involved-by-processes`)}>Manage the Processes that involve this Asset</button>
          <button onClick={() => navigate(`/assets/${item.id}/used-by-divisions`)}>Manage the Divisions that use this Asset</button>
          <button onClick={() => navigate(`/assets/${item.id}/owned-by-divisions`)}>Manage the Divisions that own this Asset</button>
          <button onClick={() => navigate(`/assets/${item.id}/has-settings`)}>Manage the Settings associated with this Asset</button>
          <button onClick={() => navigate(`/assets/${item.id}/has-login-types`)}>Manage the Login Types associated with this Asset</button>
        </>
      )}
    />
  );
}