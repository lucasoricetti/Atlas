import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Suppliers that provide this Contract",
  sourceType: "contract",
  relationType: "provided-by-supplier"
});
