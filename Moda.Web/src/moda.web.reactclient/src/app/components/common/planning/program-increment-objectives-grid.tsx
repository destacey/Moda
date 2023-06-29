import { ProgramIncrementObjectiveListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useMemo, useState } from 'react'
import ModaGrid from '../moda-grid'
import { Progress, Space, Switch } from 'antd'
import { ItemType } from 'antd/es/menu/hooks/useItems'

export interface ProgramIncrementObjectivesGridProps {
  objectives: ProgramIncrementObjectiveListDto[]
  hideProgramIncrementColumn?: boolean
  hideTeamColumn?: boolean
}

const ProgramIncrementObjectiveLinkCellRenderer = ({ value, data }) => {
  return (
    <Link
      href={`/planning/program-increments/${data.programIncrement?.localId}/objectives/${data.localId}`}
    >
      {value}
    </Link>
  )
}

const ProgramIncrementLinkCellRenderer = ({ value, data }) => {
  return (
    <Link
      href={`/planning/program-increments/${data.programIncrement?.localId}`}
    >
      {value}
    </Link>
  )
}

const TeamLinkCellRenderer = ({ value, data }) => {
  const teamLink =
    data.team?.type === 'Team'
      ? `/organizations/teams/${data.team?.localId}`
      : `/organizations/team-of-teams/${data.team?.localId}`
  return <Link href={teamLink}>{value}</Link>
}

const ProgressCellRenderer = ({ value }) => {
  // TODO: why is ag-grid showing a period on right side of the column?
  return <Progress percent={value} size="small" />
}

const ProgramIncrementObjectivesGrid = ({
  objectives,
  hideProgramIncrementColumn = false,
  hideTeamColumn = false,
}: ProgramIncrementObjectivesGridProps) => {
  const [hideProgramIncrement, setHideProgramIncrement] = useState<boolean>(
    hideProgramIncrementColumn
  )
  const [hideTeam, setHideTeam] = useState<boolean>(hideTeamColumn)

  const columnDefs = useMemo(
    () => [
      { field: 'localId', headerName: '#', width: 90 },
      {
        field: 'name',
        width: 400,
        cellRenderer: ProgramIncrementObjectiveLinkCellRenderer,
      },
      {
        field: 'programIncrement.name',
        cellRenderer: ProgramIncrementLinkCellRenderer,
        hide: hideProgramIncrement,
      },
      { field: 'status.name' },
      {
        field: 'team.name',
        cellRenderer: TeamLinkCellRenderer,
        hide: hideTeam,
      },
      { field: 'progress', width: 250, cellRenderer: ProgressCellRenderer },
      { field: 'start' },
      { field: 'target' },
      { field: 'isStretch' },
    ],
    [hideProgramIncrement, hideTeam]
  )

  const onHideProgramIncrementChange = (checked: boolean) => {
    setHideProgramIncrement(checked)
  }

  const onHideTeamChange = (checked: boolean) => {
    setHideTeam(checked)
  }

  const controlItems: ItemType[] = [
    {
      label: (
        <>
          <Space direction="vertical" size="small">
            <Space>
              <Switch
                size="small"
                checked={hideProgramIncrement}
                onChange={onHideProgramIncrementChange}
              />
              Hide PI
            </Space>
            <Space>
              <Switch
                size="small"
                checked={hideTeam}
                onChange={onHideTeamChange}
              />
              Hide Team
            </Space>
          </Space>
        </>
      ),
      key: '0',
    },
  ]

  return (
    <>
      <ModaGrid
        columnDefs={columnDefs}
        rowData={objectives}
        gridControlMenuItems={controlItems}
      />
    </>
  )
}

export default ProgramIncrementObjectivesGrid
