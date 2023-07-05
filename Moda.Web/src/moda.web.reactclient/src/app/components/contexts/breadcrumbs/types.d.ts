import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'

export interface BreadcrumbsContextType {
  breadcrumbs: BreadcrumbModel[] | null
  setBreadcrumbs: (breadcrumbs: BreadcrumbModel[]) => void
}

export interface BreadcrumbModel extends ItemType {}
