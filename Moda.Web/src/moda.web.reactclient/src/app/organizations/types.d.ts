import { TeamNavigationDto } from '@/src/services/moda-api'

export interface TeamListItem {
  id?: string
  key?: number
  name?: string
  code?: string
  type?: string
  isActive?: boolean
  teamOfTeams?: TeamNavigationDto | undefined
}

export interface CreateTeamFormValues {
  type: TeamType
  name: string
  code: string
  description: string
}

export interface EditTeamFormValues {
  id: string
  name: string
  code: string
  description: string
  type?: TeamType
}

export type TeamType = 'Team' | 'Team of Teams'
