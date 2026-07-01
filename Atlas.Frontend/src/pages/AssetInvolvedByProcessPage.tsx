import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Processes that involve this Asset",
  sourceType: "asset",
  relationType: "involved-by-process"
});