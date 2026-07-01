import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Processes that use this Setting",
  sourceType: "setting",
  relationType: "used-by-process"
});