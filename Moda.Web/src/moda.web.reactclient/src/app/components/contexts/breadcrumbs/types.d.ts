import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'

export interface BreadcrumbContextType {
  breadcrumbRoute: {
    pathname: string
    route?: ItemType[]
  }
  /**
   * Sets the title displayed for the last item in the page breadcrumb
   * @param title 
   * @returns 
   */
  setBreadcrumbTitle: (title: string) => void
  /**
   * Explicity set the full route for the page breadcrumb
   * @param route
   * @returns
   * @example setBreadcrumbRoute([{ title: 'Org' }, { title: 'Teams', href: '/orgs/teams' }, { title: ${team.name} }])
  */
  setBreadcrumbRoute: (route: ItemType[]) => void
}