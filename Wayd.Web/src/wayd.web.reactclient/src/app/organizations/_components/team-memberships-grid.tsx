'use client'

import { useState, useMemo } from 'react'
import WaydGrid from '../../../components/common/wayd-grid'
import { TeamMembershipDto } from '@/src/services/wayd-api'
import dayjs from 'dayjs'
import {
  RowMenuCellRenderer,
  renderTeamLinkHelper,
} from '../../../components/common/wayd-grid-cell-renderers'
import useAuth from '../../../components/contexts/auth'
import { MenuProps } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import EditTeamMembershipForm from './edit-team-membership-form'
import { TeamTypeName } from '../types'
import DeleteTeamMembershipForm from './delete-team-membership-form'
import { ColDef, ICellRendererParams } from 'ag-grid-community'

export interface TeamMembershipsGridProps {
  teamId: string
  teamMemberships: TeamMembershipDto[] | undefined
  isLoading: boolean
  refetch: () => void
  teamType: TeamTypeName
}

const LocalChildTeamNameLinkCellRenderer = ({ data }: ICellRendererParams<TeamMembershipDto>) => {
  return renderTeamLinkHelper(data?.child)
}

const LocalParentTeamNameLinkCellRenderer = ({ data }: ICellRendererParams<TeamMembershipDto>) => {
  return renderTeamLinkHelper(data?.parent)
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
  teamMemberships,
  isLoading,
  refetch,
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

  const refresh = async () => {
    refetch()
  }

  const columnDefs = useMemo<ColDef<TeamMembershipDto>[]>(
    () => {
      const onEditTeamMembershipMenuClicked = (membership: TeamMembershipDto) => {
        setSelectedTeamMembership(membership)
        setOpenEditTeamMembershipForm(true)
      }

      const onDeleteTeamMembershipMenuClicked = (membership: TeamMembershipDto) => {
        setSelectedTeamMembership(membership)
        setOpenDeleteTeamMembershipForm(true)
      }

      return [
      {
        width: 50,
        filter: false,
        sortable: false,
        hide: !showRowActions,
        suppressHeaderMenuButton: true,
        cellRenderer: (params: ICellRendererParams<TeamMembershipDto>) => {
          // only allow editing memberships for current team
          if (teamId != params.data!.child.id) return null

          const menuItems = getRowMenuItems({
            id: params.data!.id,
            membership: params.data!,
            canManageTeamMemberships,
            onEditTeamMembershipMenuClicked,
            onDeleteTeamMembershipMenuClicked,
          })

          return RowMenuCellRenderer({ ...params, menuItems: menuItems ?? [] })
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
        valueGetter: (params) => params.data?.start ? dayjs(params.data.start).format('M/D/YYYY') : null,
      },
      {
        field: 'end',
        valueGetter: (params) =>
          params.data?.end ? dayjs(params.data.end).format('M/D/YYYY') : null,
      },
    ]},
    [
      showRowActions,
      teamId,
      canManageTeamMemberships,
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
      <WaydGrid
        height={550}
        columnDefs={columnDefs}
        rowData={teamMemberships}
        loading={isLoading}
        loadData={refresh}
      />
      {openEditTeamMembershipForm && selectedTeamMembership && (
        <EditTeamMembershipForm
          membership={selectedTeamMembership}
          teamType={teamType}
          onFormSave={() => onEditTeamMembershipFormClosed(true)}
          onFormCancel={() => onEditTeamMembershipFormClosed(false)}
        />
      )}
      {openDeleteTeamMembershipForm && selectedTeamMembership && (
        <DeleteTeamMembershipForm
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
