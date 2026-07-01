import createRelationRoutePage from "./createRelationRoutePage";

export default createRelationRoutePage({
  title: "Divisions that use this Asset",
  sourceType: "asset",
  relationType: "used-by-division"
});
