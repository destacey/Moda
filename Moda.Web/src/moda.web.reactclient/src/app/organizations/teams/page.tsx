'use client'

import PageTitle from "@/src/app/components/common/page-title";
import { useEffect, useState } from "react";
import ModaGrid from "../../components/common/moda-grid";
import { getTeamsClient, getTeamsOfTeamsClient } from "@/src/services/clients";
import { TeamNavigationDto } from "@/src/services/moda-api";

interface TeamListViewModel {
  id: string,
  localId: number,
  name: string,
  code: string,
  type: string,
  isActive: boolean,
  teamOfTeams?: TeamNavigationDto | undefined
}

const columnDefs = [
  { field: 'localId', headerName: 'Id' },
  { field: 'name' },
  { field: 'code' },
  { field: 'type' },
  { field: 'teamOfTeams.name', headerName: 'Manager' },
  { field: 'isActive' } // TODO: convert to yes/no
]

const Page = () => {
  const [teams, setTeams] = useState<TeamListViewModel[]>([])

  useEffect(() => {
    const getTeams = async () => {
      const teamsClient = await getTeamsClient()
      const teamsDtos = await teamsClient.getList(false)
      const teamOfTeamsClient = await getTeamsOfTeamsClient()
      const teamOfTeamsDtos = await teamOfTeamsClient.getList(false)
      const teamVMs = [...teamsDtos as TeamListViewModel[], ...teamOfTeamsDtos as TeamListViewModel[]]
      setTeams(teamVMs)
    }

    getTeams()
  }, [])

  return (
    <>
      <PageTitle title="Teams" />

      <ModaGrid columnDefs={columnDefs}
        rowData={teams} />
    </>
  );
}

export default Page;