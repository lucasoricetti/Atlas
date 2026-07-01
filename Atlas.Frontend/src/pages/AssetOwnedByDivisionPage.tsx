import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Divisions that own this Asset",
  sourceType: "asset",
  relationType: "owned-by-division"
});
