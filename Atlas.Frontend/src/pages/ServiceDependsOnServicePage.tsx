import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Services that this Service depends on",
  sourceType: "service",
  relationType: "depends-on-service",
  supportsCritical: true
});


