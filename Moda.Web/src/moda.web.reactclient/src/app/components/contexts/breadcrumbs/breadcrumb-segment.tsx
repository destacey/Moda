import { Typography } from 'antd'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import Link from 'next/link'

const { Text } = Typography

interface BreadcrumbSegmentProps {
  route: ItemType
  paths: string[]
  last?: boolean
}

const BreadcrumbSegment = ({ route, last }: BreadcrumbSegmentProps) => {
  return ( last || !route.href ) 
  ? <Text>{route.title}</Text> 
  : <Link href={route.href}>{route.title}</Link>
}

export default BreadcrumbSegment