import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Assets that this Asset depends on",
  sourceType: "asset",
  relationType: "depends-on-asset",
  supportsCritical: true
});


