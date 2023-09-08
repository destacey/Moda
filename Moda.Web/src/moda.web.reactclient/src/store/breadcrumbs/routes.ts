import { BreadcrumbItem } from "./types"

const routes = {
  organizations: {
    title: 'Organizations',
    href: null,
  },
  'team-of-teams': {
    title: 'Teams',
    href: '/organizations/teams',
  },
  employees: {
    title: 'Employees',
  },
  teams: {
    title: 'Teams',
  },
  planning: {
    title: 'Planning',
    href: null,
  },
  'program-increments': {
    title: 'Program Increments',
  },
  risks: {
    title: 'Teams',
    href: null,
  },
  settings: {
    title: 'Settings',
    href: null,
  },
  'background-jobs': {
    title: 'Background Jobs',
    href: null,
  },
  connections: {
    title: 'Connections',
  },
  roles: {
    title: 'Roles',
  },
  users: {
    title: 'Users',
  },
  account: {
    title: 'Account',
    href: null,
  },
}

export const generateRoute = 
(pathname: string, lastItemTitleOverride?: string): BreadcrumbItem[] => {
  const pathSegments = pathname.split('/').filter((item) => item !== '')
  return pathSegments.map((item, index) => {
    // Set the title of the last path segment to the title passed in from the page
    let titleOverride = {}
    if (lastItemTitleOverride && index === pathSegments.length - 1) {
      titleOverride = { title: lastItemTitleOverride }
    }

    return {
      path: item,
      href: '/' + pathSegments.slice(0, index + 1).join('/'),
      ...routes[item],
      ...titleOverride,
    }
  })
}

export default routes
