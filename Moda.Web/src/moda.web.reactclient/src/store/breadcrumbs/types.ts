
export interface BreadcrumbState {
  items: BreadcrumbItem[];
  isVisible: boolean;
  forPath: string;
}

export interface BreadcrumbItem {
  title: string;
  href?: string;
  path?: string;
}