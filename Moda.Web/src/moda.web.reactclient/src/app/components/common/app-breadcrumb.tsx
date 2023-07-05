import { Breadcrumb } from 'antd'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import Link from 'next/link'
import { Typography } from 'antd'
import useBreadcrumbs from '../contexts/breadcrumbs/use-breadcrumbs'

const { Text } = Typography

const AppBreadcrumb = () => {
  const { breadcrumbs } = useBreadcrumbs()

  const itemRender = (route: ItemType, params: any, routes: ItemType[]) => {
    const last = routes.indexOf(route) === routes.length - 1
    return last ? (
      <Text>{route.title}</Text>
    ) : (
      <Link href={route.path}>{route.title}</Link>
    )
  }

  return (
    <Breadcrumb
      separator=">"
      style={{ margin: '16px 0' }}
      itemRender={itemRender}
      items={breadcrumbs}
    />
  )
}

export default AppBreadcrumb
