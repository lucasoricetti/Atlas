import { useNavigate } from "react-router-dom";
import CrudPage from "../components/CrudPage";

export default function AcnMacroAreasPage() {
  const navigate = useNavigate();

  return (
    <CrudPage
      title="ACN Macro Areas"
      endpoint="/api/acn-macro-areas"
      fields={[
        {
          name: "name",
          label: "Name",
          // required → shown in summary row by default + marked mandatory in form.
          required: true,
        },
        {
          name: "preAssignedAcnCategory",
          label: "Pre-Assigned ACN Category",
          // required → always visible in the collapsed row and in "Main information".
          required: true,
          type: "select",
          optionsKey: "acnCategoryOfRelevances",
        },
        {
          name: "customAcnCategory",
          label: "Custom ACN Category",
          type: "select",
          optionsKey: "acnCategoryOfRelevances",
          // pinned → not required, but we still want it visible in the summary row
          // so users can spot customised categories without expanding the row.
          pinned: true,
          // displayInMainSection → even though it is not required, place it alongside
          // the other important fields in the "Main information" detail block rather
          // than burying it in "Additional details".
          displayInMainSection: true,
        },
      ]}
      extraActions={(item) => (
        <>
          <button onClick={() => navigate(`/acn-macro-areas/${item.id}/classifies`)}>
            Manage the Processes classified by this Macro Area
          </button>
        </>
      )}
    />
  );
}