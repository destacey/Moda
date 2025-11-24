'use client'

import { Breadcrumb, Typography } from 'antd'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { useAppSelector } from '../../hooks'
import { selectBreadcrumb, BreadcrumbItem } from '@/src/store/breadcrumbs'
import { usePathname } from 'next/navigation'
import Link from 'next/link'
import { generateRoute } from '@/src/store/breadcrumbs/routes'

const { Text } = Typography

interface BreadcrumbSegmentProps {
  route: BreadcrumbItem
  paths: string[]
  last?: boolean
}

const BreadcrumbSegment = ({ route, last }: BreadcrumbSegmentProps) => {
  return last || !route.href ? (
    <Text>{route.title}</Text>
  ) : (
    <Link href={route.href}>{route.title}</Link>
  )
}

const AppBreadcrumb = () => {
  const pathname = usePathname()
  const breadcrumbRoute = useAppSelector(selectBreadcrumb)

  const pathItems =
    pathname === breadcrumbRoute.forPath
      ? breadcrumbRoute.items
      : generateRoute(pathname)
  const isVisible =
    pathname === breadcrumbRoute.forPath ? breadcrumbRoute.isVisible : true

  const itemRender = (
    route: BreadcrumbItem,
    params: any,
    routes: ItemType[],
    paths: string[],
  ) => {
    const last = routes.indexOf(route) === routes.length - 1
    return <BreadcrumbSegment route={route} paths={paths} last={last} />
  }

  // TODO: how do we make this more configurable without having to use a hook in static scenarios?  Example: We don't want breadcrumbs to show on the Settings page.
  if (
    !isVisible ||
    (pathItems.length >= 1 &&
      (pathItems[0].title === 'Settings' ||
        (pathItems[0].title === 'Planning' &&
          pathItems[1].title === 'Planning Intervals')))
  )
    return null

  return (
    <Breadcrumb
      separator=">"
      style={{ margin: '16px 0px' }}
      itemRender={itemRender}
      items={pathItems}
    />
  )
}

export default AppBreadcrumb
