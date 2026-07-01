import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Divisions that own this Process",
  sourceType: "process",
  relationType: "owned-by-division"
});