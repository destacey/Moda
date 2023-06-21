'use client'

import PageTitle from "@/src/app/components/common/page-title";
import { useCallback, useMemo, useState } from "react";
import ModaGrid from "../../components/common/moda-grid";
import { getTeamsClient, getTeamsOfTeamsClient } from "@/src/services/clients";
import { TeamNavigationDto } from "@/src/services/moda-api";
import { ItemType } from "antd/es/menu/hooks/useItems";
import { Space, Switch } from "antd";
import Link from "next/link";

interface TeamListViewModel {
  id: string,
  localId: number,
  name: string,
  code: string,
  type: string,
  isActive: boolean,
  teamOfTeams?: TeamNavigationDto | undefined
}

const TeamLinkCellRenderer = ({ value, data }) => {
    const teamRoute = data.type === 'Team' ? 'teams' : 'team-of-teams'
    return (
        <Link href={`/organizations/${teamRoute}/${data.localId}`}>
            {value}
        </Link>
    );
};

const TeamOfTeamsLinkCellRenderer = ({ value, data }) => {
    return (
        <Link href={`/organizations/team-of-teams/${data.teamOfTeams?.localId}`}>
            {value}
        </Link>
    );
};

const Page = () => {
  const [teams, setTeams] = useState<TeamListViewModel[]>([])
  const [includeDisabled, setIncludeDisabled] = useState<boolean>(false)

  const columnDefs = useMemo(() => [
    { field: 'localId', headerName: '#', width: 75 },
    { field: 'name', cellRenderer: TeamLinkCellRenderer },
    { field: 'code', width: 125 },
    { field: 'type' },
    { field: 'teamOfTeams.name', headerName: 'Team of Teams', cellRenderer: TeamOfTeamsLinkCellRenderer },
    { field: 'isActive' } // TODO: convert to yes/no
  ], []);

  const onIncludeDisabledChange = (checked: boolean) => {
    setIncludeDisabled(checked)
  }

  const controlItems: ItemType[] = [
    {
      label: <Space><Switch size="small" 
                            checked={includeDisabled} 
                            onChange={onIncludeDisabledChange} />Include Disabled</Space>,
      key: '0',
    }
  ];

  const getTeams = useCallback(async () => {
    const teamsClient = await getTeamsClient()
    const teamsDtos = await teamsClient.getList(includeDisabled)
    const teamOfTeamsClient = await getTeamsOfTeamsClient()
    const teamOfTeamsDtos = await teamOfTeamsClient.getList(includeDisabled)
    const teamVMs = [...teamsDtos as TeamListViewModel[], ...teamOfTeamsDtos as TeamListViewModel[]]
    setTeams(teamVMs)
  },[includeDisabled])

  return (
    <>
      <PageTitle title="Teams" />
      <ModaGrid columnDefs={columnDefs} gridControlMenuItems={controlItems}
        rowData={teams} loadData={getTeams} />
    </>
  );
}

export default Page;