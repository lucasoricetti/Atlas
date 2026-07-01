import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Processes owned by this Division",
  sourceType: "division",
  relationType: "owns-process"
});