import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Assets that use this Login Type",
  sourceType: "logintype",
  relationType: "used-by-asset"
});
