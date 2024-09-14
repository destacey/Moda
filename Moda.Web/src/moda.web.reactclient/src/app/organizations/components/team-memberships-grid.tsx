'use client'

import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { TeamMembershipDto } from '@/src/services/moda-api'
import dayjs from 'dayjs'
import { UseQueryResult } from 'react-query'
import {
  RowMenuCellRenderer,
  TeamNameLinkCellRenderer,
} from '../../components/common/moda-grid-cell-renderers'
import useAuth from '../../components/contexts/auth'
import { MenuProps } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import EditTeamMembershipForm from './edit-team-membership-form'
import { TeamTypeName } from '../types'
import DeleteTeamMembershipForm from './delete-team-membership-form'
import { ColDef } from 'ag-grid-community'

export interface TeamMembershipsGridProps {
  teamId: string
  teamMembershipsQuery: UseQueryResult<TeamMembershipDto[], unknown>
  teamType: TeamTypeName
}

const LocalChildTeamNameLinkCellRenderer = ({ data }) => {
  return TeamNameLinkCellRenderer({ data: data.child })
}

const LocalParentTeamNameLinkCellRenderer = ({ data }) => {
  return TeamNameLinkCellRenderer({ data: data.parent })
}

interface RowMenuProps extends MenuProps {
  membership: TeamMembershipDto
  canManageTeamMemberships: boolean
  onEditTeamMembershipMenuClicked: (membership: TeamMembershipDto) => void
  onDeleteTeamMembershipMenuClicked: (membership: TeamMembershipDto) => void
}

const getRowMenuItems = (props: RowMenuProps) => {
  if (!props.membership) return null

  return [
    {
      key: 'edit-team-membership',
      label: 'Edit Team Membership',
      disabled: !props.canManageTeamMemberships,
      onClick: () => props.onEditTeamMembershipMenuClicked(props.membership),
    },
    {
      key: 'delete-team-membership',
      label: 'Delete Team Membership',
      disabled: !props.canManageTeamMemberships,
      onClick: () => props.onDeleteTeamMembershipMenuClicked(props.membership),
    },
  ] as ItemType[]
}

const TeamMembershipsGrid = ({
  teamId,
  teamMembershipsQuery,
  teamType,
}: TeamMembershipsGridProps) => {
  const [openEditTeamMembershipForm, setOpenEditTeamMembershipForm] =
    useState<boolean>(false)
  const [openDeleteTeamMembershipForm, setOpenDeleteTeamMembershipForm] =
    useState<boolean>(false)
  const [selectedTeamMembership, setSelectedTeamMembership] =
    useState<TeamMembershipDto | null>(null)

  const { hasClaim } = useAuth()
  const canManageTeamMemberships = hasClaim(
    'Permission',
    'Permissions.Teams.ManageTeamMemberships',
  )
  const showRowActions = canManageTeamMemberships

  const refresh = useCallback(async () => {
    teamMembershipsQuery.refetch()
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [])

  const onEditTeamMembershipMenuClicked = useCallback(
    (membership: TeamMembershipDto) => {
      setSelectedTeamMembership(membership)
      setOpenEditTeamMembershipForm(true)
    },
    [],
  )

  const onDeleteTeamMembershipMenuClicked = useCallback(
    (membership: TeamMembershipDto) => {
      setSelectedTeamMembership(membership)
      setOpenDeleteTeamMembershipForm(true)
    },
    [],
  )

  const columnDefs = useMemo<ColDef<TeamMembershipDto>[]>(
    () => [
      {
        width: 50,
        filter: false,
        sortable: false,
        hide: !showRowActions,
        suppressHeaderMenuButton: true,
        cellRenderer: (params) => {
          // only allow editing memberships for current team
          if (teamId != params.data.child.id) return null

          const menuItems = getRowMenuItems({
            id: params.data.id,
            membership: params.data,
            canManageTeamMemberships,
            onEditTeamMembershipMenuClicked,
            onDeleteTeamMembershipMenuClicked,
          })

          return RowMenuCellRenderer({ menuItems })
        },
      },
      {
        field: 'child.name',
        headerName: 'Child Team',
        cellRenderer: LocalChildTeamNameLinkCellRenderer,
      },
      {
        field: 'parent.name',
        headerName: 'Parent Team',
        cellRenderer: LocalParentTeamNameLinkCellRenderer,
      },
      { field: 'state' },
      {
        field: 'start',
        valueGetter: (params) => dayjs(params.data.start).format('M/D/YYYY'),
      },
      {
        field: 'end',
        valueGetter: (params) =>
          params.data.end ? dayjs(params.data.end).format('M/D/YYYY') : null,
      },
    ],
    [
      canManageTeamMemberships,
      onDeleteTeamMembershipMenuClicked,
      onEditTeamMembershipMenuClicked,
      showRowActions,
      teamId,
    ],
  )

  const onEditTeamMembershipFormClosed = (wasCreated: boolean) => {
    setOpenEditTeamMembershipForm(false)
    setSelectedTeamMembership(null)
    if (wasCreated) {
      refresh()
    }
  }

  const onDeleteTeamMembershipFormClosed = (wasCreated: boolean) => {
    setOpenDeleteTeamMembershipForm(false)
    setSelectedTeamMembership(null)
    if (wasCreated) {
      refresh()
    }
  }

  return (
    <>
      <ModaGrid
        height={550}
        columnDefs={columnDefs}
        rowData={teamMembershipsQuery.data}
        loading={teamMembershipsQuery.isLoading}
        loadData={refresh}
      />
      {openEditTeamMembershipForm && (
        <EditTeamMembershipForm
          showForm={openEditTeamMembershipForm}
          membership={selectedTeamMembership}
          teamType={teamType}
          onFormSave={() => onEditTeamMembershipFormClosed(true)}
          onFormCancel={() => onEditTeamMembershipFormClosed(false)}
        />
      )}
      {openDeleteTeamMembershipForm && (
        <DeleteTeamMembershipForm
          showForm={openDeleteTeamMembershipForm}
          membership={selectedTeamMembership}
          teamType={teamType}
          onFormSave={() => onDeleteTeamMembershipFormClosed(true)}
          onFormCancel={() => onDeleteTeamMembershipFormClosed(false)}
        />
      )}
    </>
  )
}

export default TeamMembershipsGrid
