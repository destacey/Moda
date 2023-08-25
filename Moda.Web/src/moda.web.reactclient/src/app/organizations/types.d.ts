import { TeamNavigationDto } from '@/src/services/moda-api'

export interface TeamListItem {
  id: string
  key: number
  name: string
  code: string
  type: string
  isActive: boolean
  teamOfTeams?: TeamNavigationDto | undefined
}
