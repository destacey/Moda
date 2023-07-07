import { Breadcrumb } from 'antd'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { useEffect, useState } from 'react'
import useBreadcrumbs, { BreadcrumbSegment } from '../contexts/breadcrumbs'
import { usePathname } from 'next/navigation'

export interface AppBreadcrumbProps {
}

const AppBreadcrumb = () => {
  const [pathItems, setPathItems] = useState<ItemType[]>([])
  const pathname = usePathname()
  const { breadcrumbRoute } = useBreadcrumbs()

  useEffect(() => {
    // Only use the breadcrumbRoute if the pathname matches the current route
    if (pathname === breadcrumbRoute?.pathname && breadcrumbRoute?.fullRoute) {
        setPathItems(breadcrumbRoute.fullRoute)
    }
    else {
      const title = pathname === breadcrumbRoute?.pathname ? breadcrumbRoute?.title : null
      const pathSegments = pathname.split('/').filter((item) => item !== '')
      const pathItems: ItemType[] = pathSegments.map((item, index) => {
        return {
          path: item,
          breadcrumbName: title && index === pathSegments.length - 1 ? title : null,
          href: '/' + pathSegments.slice(0, index + 1).join('/'),
        }
      })
  
      setPathItems(pathItems)
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

  const isHome = pathname === '/'

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
