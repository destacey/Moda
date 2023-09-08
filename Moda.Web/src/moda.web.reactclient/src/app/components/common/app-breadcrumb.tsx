'use client'

import { Breadcrumb, Typography } from 'antd'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { useEffect, useState } from 'react'
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
  return ( last || !route.href ) 
  ? <Text>{route.title}</Text> 
  : <Link href={route.href}>{route.title}</Link>
}

const AppBreadcrumb = () => {
  const [pathItems, setPathItems] = useState<ItemType[]>([])
  const [isVisible, setIsVisible] = useState<boolean>(true)
  const pathname = usePathname()
  const breadcrumbRoute = useAppSelector(selectBreadcrumb)


  useEffect(() => {
    if(pathname === breadcrumbRoute.forPath) {
      setPathItems(breadcrumbRoute.items)
      setIsVisible(breadcrumbRoute.isVisible)
    }
    else {
      setPathItems(generateRoute(pathname))
    }
  }, [pathname, breadcrumbRoute])

  const itemRender = (
    route: BreadcrumbItem,
    params: any,
    routes: ItemType[],
    paths: string[],
  ) => {
    const last = routes.indexOf(route) === routes.length - 1
    return <BreadcrumbSegment route={route} paths={paths} last={last} />
  }

  if (!isVisible) return null

  return (
    <Breadcrumb
      separator=">"
      style={{ margin: '16px 0' }}
      itemRender={itemRender}
      items={pathItems}
    />
  )
}

export default AppBreadcrumb
