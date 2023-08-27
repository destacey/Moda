import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'

export interface BreadcrumbContextType {
  breadcrumbRoute: {
    pathname: string
    route?: ItemType[]
  }
  isVisible: boolean
  /**
   * Sets the value to make the breadcrumb visible or not
   * @param isVisible
   * @returns
   */
  setBreadcrumbIsVisible: (isVisible: boolean) => void
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
