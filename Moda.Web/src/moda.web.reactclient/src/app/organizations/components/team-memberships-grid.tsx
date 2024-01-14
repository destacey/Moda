'use client'

import { useCallback, useMemo, useState } from 'react'
import ModaGrid from '../../components/common/moda-grid'
import { TeamMembershipDto } from '@/src/services/moda-api'
import dayjs from 'dayjs'
import { UseQueryResult } from 'react-query'
import {
  RowMenuCellRenderer,
  TeamLinkCellRenderer,
} from '../../components/common/moda-grid-cell-renderers'
import useAuth from '../../components/contexts/auth'
import { MenuProps } from 'antd'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import EditTeamMembershipForm from './edit-team-membership-form'
import { TeamTypeName } from '../types'

export interface TeamMembershipsGridProps {
  teamMembershipsQuery: UseQueryResult<TeamMembershipDto[], unknown>
  teamType: TeamTypeName
}

interface RowMenuProps extends MenuProps {
  membership: TeamMembershipDto
  canManageTeamMemberships: boolean
  onEditTeamMembershipMenuClicked: (membership: TeamMembershipDto) => void
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
  ] as ItemType[]
}

const TeamMembershipsGrid = ({
  teamMembershipsQuery,
  teamType,
}: TeamMembershipsGridProps) => {
  const [openEditTeamMembershipForm, setOpenEditTeamMembershipForm] =
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

  const columnDefs = useMemo(
    () => [
      {
        field: 'actions',
        headerName: '',
        width: 50,
        filter: false,
        sortable: false,
        hide: !showRowActions,
        suppressMenu: true,
        cellRenderer: (params) => {
          const menuItems = getRowMenuItems({
            membership: params.data,
            canManageTeamMemberships,
            onEditTeamMembershipMenuClicked,
          })

          return RowMenuCellRenderer({ menuItems })
        },
      },
      {
        field: 'child',
        valueFormatter: (params) => params.value.name,
        cellRenderer: TeamLinkCellRenderer,
      },
      {
        field: 'parent',
        valueFormatter: (params) => params.value.name,
        cellRenderer: TeamLinkCellRenderer,
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
    [canManageTeamMemberships, onEditTeamMembershipMenuClicked, showRowActions],
  )

  const onEditTeamMembershipFormClosed = (wasCreated: boolean) => {
    setOpenEditTeamMembershipForm(false)
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
    </>
  )
}

export default TeamMembershipsGrid
