import { useParams } from "react-router-dom";
import type { ComponentProps, ComponentType } from "react";
import RelationPage from "../components/RelationPage";

type RelationRouteConfig = Omit<ComponentProps<typeof RelationPage>, "sourceId">;

export default function createRelationRoutePage(config: RelationRouteConfig): ComponentType {
  function RelationRoutePage() {
    const { id } = useParams();

    return <RelationPage {...config} sourceId={id ?? ""} />;
  }

  return RelationRoutePage;
}
