import { Breadcrumb } from 'antd'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import Link from 'next/link'
import { useEffect, useState } from 'react'
import { Typography } from 'antd'
import { capitalize } from '@/src/utils/strings'

const { Text } = Typography
export interface AppBreadcrumbProps {
  pathname: string
}

const AppBreadcrumb = ({ pathname }: AppBreadcrumbProps) => {
  const [pathItems, setPathItems] = useState<ItemType[]>([])

  useEffect(() => {
    const pathItems = [
      'home',
      ...pathname.split('/').filter((item) => item !== ''),
    ].map((item) => {
      return {
        title: capitalize(item),
        path: item === 'home' ? '/' : '/' + item,
      }
    })

    setPathItems(pathItems)
  }, [pathname])

  const itemRender = (
    route: ItemType,
    params: any,
    routes: ItemType[],
    paths: string[]
  ) => {
    const last = routes.indexOf(route) === routes.length - 1
    return last ? (
      <Text>{route.title}</Text>
    ) : (
      <Link href={paths.join('/') || '/'}>{route.title}</Link>
    )
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
