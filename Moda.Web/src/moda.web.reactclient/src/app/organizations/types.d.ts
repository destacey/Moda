import { TeamNavigationDto } from './../../services/moda-api'
import { TeamNavigationDto } from '@/src/services/moda-api'

export interface TeamListItem extends TeamNavigationDto {
  code?: string
  isActive?: boolean
  teamOfTeams?: TeamNavigationDto | undefined
}

export interface CreateTeamFormValues {
  type: TeamTypeName
  name: string
  code: string
  description: string
}

export interface EditTeamFormValues {
  id: string
  name: string
  code: string
  description: string
  type?: TeamTypeName
}

export type TeamTypeName = 'Team' | 'Team of Teams'
