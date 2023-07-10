import { Breadcrumb } from 'antd'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { useEffect, useState } from 'react'
import useBreadcrumbs, { BreadcrumbSegment } from '../contexts/breadcrumbs'
import { usePathname } from 'next/navigation'

export interface AppBreadcrumbProps {
}

const AppBreadcrumb = () => {
  const [pathItems, setPathItems] = useState<ItemType[]>([])
  const [isHome, setIsHome] = useState<boolean>(false)
  const pathname = usePathname()
  const { breadcrumbRoute } = useBreadcrumbs()

  useEffect(() => {
    if (breadcrumbRoute?.route) {
      setIsHome(pathname === '/')
      setPathItems(breadcrumbRoute.route)
    }
  }, [pathname, breadcrumbRoute])

  const itemRender = (
    route: ItemType,
    params: any,
    routes: ItemType[],
    paths: string[]
  ) => {
    const last = routes.indexOf(route) === routes.length - 1
    return <BreadcrumbSegment route={route} paths={paths} last={last} />
  }

  return (
    !isHome && (
      <Breadcrumb
        separator=">"
        style={{ margin: '16px 0' }}
        itemRender={itemRender}
        items={pathItems}
      />
    )
  )
}

export default AppBreadcrumb
