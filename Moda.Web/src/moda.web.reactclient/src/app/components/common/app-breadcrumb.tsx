'use client'

import { Breadcrumb } from 'antd'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { useEffect, useState } from 'react'
import useBreadcrumbs, { BreadcrumbSegment } from '../contexts/breadcrumbs'

export interface AppBreadcrumbProps {}

const AppBreadcrumb = () => {
  const [pathItems, setPathItems] = useState<ItemType[]>([])
  const { breadcrumbRoute, isVisible } = useBreadcrumbs()

  useEffect(() => {
    if (breadcrumbRoute?.route !== undefined) {
      setPathItems(breadcrumbRoute.route)
    }
  }, [breadcrumbRoute])

  const itemRender = (
    route: ItemType,
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
