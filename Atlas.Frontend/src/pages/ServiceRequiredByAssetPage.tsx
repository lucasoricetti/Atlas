import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Assets that require this Service",
  sourceType: "service",
  relationType: "required-by-asset",
  supportsCritical: true
});
