import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Assets that use this Setting",
  sourceType: "setting",
  relationType: "used-by-asset"
});
