import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Services that compose this Asset",
  sourceType: "asset",
  relationType: "composed-by-service",
  supportsCritical: true
});


