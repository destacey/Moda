import { TeamNavigationDto } from "@/src/services/moda-api";

export interface TeamListItem {
    id: string,
    localId: number,
    name: string,
    code: string,
    type: string,
    isActive: boolean,
    teamOfTeams?: TeamNavigationDto | undefined
  }