import type { LucideIcon } from "lucide-react";

interface Props {
  icon: LucideIcon;
  size?: number;
  className?: string;
}

export default function EntityIcon({ icon: Icon, size = 20, className = "" }: Props) {
  return <Icon size={size} className={className} strokeWidth={1.5} />;
}
