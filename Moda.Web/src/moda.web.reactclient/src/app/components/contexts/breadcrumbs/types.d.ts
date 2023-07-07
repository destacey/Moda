import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'

export interface BreadcrumbContextType {
  breadcrumbRoute: {
    pathname: string
    title?: string
    fullRoute?: ItemType[]
  }
  setBreadcrumbTitle: (title: string) => void
  setBreadcrumbRoute: (fullRoute: ItemType[]) => void
}