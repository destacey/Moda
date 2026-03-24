import { GlobalSearchResultItemDto } from '@/src/services/moda-api'

export function getSearchResultUrl(item: GlobalSearchResultItemDto): string {
  const { entityType, key, auxKey } = item

  switch (entityType) {
    // Work
    case 'WorkItem':
      return `/work/workspaces/${auxKey}/work-items/${key}`
    case 'Workspace':
      return `/work/workspaces/${key}`

    // Organization
    case 'Team':
      return `/organizations/teams/${auxKey}`
    case 'TeamOfTeams':
      return `/organizations/team-of-teams/${auxKey}`
    case 'Employee':
      return `/organizations/employees/${key}`

    // Planning
    case 'Iteration':
      return `/planning/sprints/${key}`
    case 'PlanningInterval':
      return `/planning/planning-intervals/${key}`
    case 'PlanningIntervalIteration':
      return `/planning/planning-intervals/${auxKey}/iterations/${key}`
    case 'PlanningIntervalObjective':
      return `/planning/planning-intervals/${auxKey}/objectives/${key}`
    case 'PiTeam': {
      const [piKey, teamCode] = (auxKey ?? '').split('|')
      return `/planning/planning-intervals/${piKey}/plan-review#${teamCode}`
    }

    // PPM
    case 'Project':
      return `/ppm/projects/${key}`
    case 'Program':
      return `/ppm/programs/${key}`
    case 'ProjectPortfolio':
      return `/ppm/portfolios/${key}`
    case 'StrategicInitiative':
      return `/ppm/strategic-initiatives/${key}`

    default:
      return '/'
  }
}
