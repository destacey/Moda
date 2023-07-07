import { Typography } from 'antd'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import Link from 'next/link'

const { Text } = Typography

const routes = {
  'organizations': {
    title: 'Organizations'
  },
  'team-of-teams': {
    title: 'Teams',
    route: 'teams'
  },
  'employees': {
    title: 'Employees',
    route: 'employees'
  },
  'teams': {
    title: 'Teams',
    route: 'teams'
  },
  'planning': {
    title: 'Planning',
  },
  'program-increments': {
    title: 'Program Increments',
    route: 'program-increments'
  },
  'risks': {
    title: 'Teams',
  },
  'settings': {
    title: 'Settings',
  },
  'background-jobs': {
    title: 'Background Jobs',
  },
  'roles': {
    title: 'Roles',
    route: 'roles'
  },
  'users': {
    title: 'Users',
    route: 'users'
  },
}

interface BreadcrumbSegmentProps {
  route: ItemType
  paths: string[]
  last?: boolean
}

const BreadcrumbSegment = ({ route, paths, last }: BreadcrumbSegmentProps) => {
  const currentRoute = routes[route.path]
  if (!currentRoute || route.breadcrumbName) {
    return <Text>{route.breadcrumbName}</Text>
  }
  if (last || !currentRoute.route) {
    return <Text>{currentRoute.title}</Text>
  }
  // If currentRoute.route is defined and doesn't match the route.path, then replace the path segment in route.href with the currentRoute.route
  if (currentRoute.route && currentRoute.route !== route.path) {
    const pathSegments = route.href.split('/').filter((item) => item !== '')
    const index = pathSegments.indexOf(route.path)
    pathSegments[index] = currentRoute.route
    return <Link href={'/' + pathSegments.join('/')}>{currentRoute.title}</Link>
  }

  return <Link href={route.href}>{currentRoute.title}</Link>
}

export default BreadcrumbSegment