'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { Card, MenuProps, message } from 'antd'
import { createElement, useCallback, useEffect, useMemo, useState } from 'react'
import TeamOfTeamsDetails from './team-of-teams-details'
import RisksGrid, {
  RisksGridProps,
} from '@/src/app/components/common/planning/risks-grid'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import { EditTeamForm, TeamMembershipsGrid } from '../../components'
import useAuth from '@/src/app/components/contexts/auth'
import {
  useGetTeamOfTeamsMemberships,
  useGetTeamOfTeamsRisks,
} from '@/src/services/queries/organization-queries'
import { authorizePage } from '@/src/app/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import {
  retrieveTeam,
  setEditMode,
  selectTeamContext,
} from '../../../../store/features/organizations/team-slice'
import { useAppDispatch, useAppSelector } from '@/src/app/hooks'
import { setBreadcrumbTitle } from '@/src/store/breadcrumbs'
import { CreateTeamMembershipForm } from '../../components'
import { PageActions } from '@/src/app/components/common'
import { ItemType } from 'antd/es/menu/interface'
import DeactivateTeamOfTeamsForm from '../../components/deactivate-team-of-teams-form'

enum TeamOfTeamsTabs {
  Details = 'details',
  RiskManagement = 'risk-management',
  TeamMemberships = 'team-memberships',
}

const TeamOfTeamsDetailsPage = ({ params }) => {
  useDocumentTitle('Team of Teams Details')
  const { key } = params
  const [activeTab, setActiveTab] = useState(TeamOfTeamsTabs.Details)
  const [openCreateTeamMembershipForm, setOpenCreateTeamMembershipForm] =
    useState<boolean>(false)
  const [openDeactivateTeamForm, setOpenDeactivateTeamForm] =
    useState<boolean>(false)
  const [teamMembershipsQueryEnabled, setTeamMembershipsQueryEnabled] =
    useState<boolean>(false)
  const [risksQueryEnabled, setRisksQueryEnabled] = useState<boolean>(false)
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)

  const [messageApi, contextHolder] = message.useMessage()

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')
  const canManageTeamMemberships = hasClaim(
    'Permission',
    'Permissions.Teams.ManageTeamMemberships',
  )

  const {
    item: team,
    isInEditMode,
    notFound: teamNotFound,
    error,
  } = useAppSelector(selectTeamContext)
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const teamMembershipsQuery = useGetTeamOfTeamsMemberships(
    team?.id,
    teamMembershipsQueryEnabled,
  )

  const risksQuery = useGetTeamOfTeamsRisks(
    team?.id,
    includeClosedRisks,
    risksQueryEnabled,
  )

  const onIncludeClosedRisksChanged = useCallback((includeClosed: boolean) => {
    setIncludeClosedRisks(includeClosed)
  }, [])

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const items: ItemType[] = []

    if (canUpdateTeam) {
      items.push(
        {
          key: 'edit',
          label: 'Edit',
          onClick: () => dispatch(setEditMode(true)),
        },
        {
          key: 'deactivate',
          label: 'Deactivate',
          onClick: () => setOpenDeactivateTeamForm(true),
        },
      )
    }
    if (canManageTeamMemberships) {
      items.push({
        key: 'add-team-membership',
        label: 'Add Team Membership',
        onClick: () => setOpenCreateTeamMembershipForm(true),
      })
    }

    return items
  }, [canManageTeamMemberships, canUpdateTeam, dispatch])

  const tabs = [
    {
      key: TeamOfTeamsTabs.Details,
      tab: 'Details',
      content: <TeamOfTeamsDetails team={team} />,
    },
    {
      key: TeamOfTeamsTabs.RiskManagement,
      tab: 'Risk Management',
      content: createElement(RisksGrid, {
        risksQuery: risksQuery,
        updateIncludeClosed: onIncludeClosedRisksChanged,
        newRisksAllowed: true,
        teamId: team?.id,
        hideTeamColumn: true,
      } as RisksGridProps),
    },
    {
      key: TeamOfTeamsTabs.TeamMemberships,
      tab: 'Team Memberships',
      content: createElement(TeamMembershipsGrid, {
        teamId: team?.id,
        teamMembershipsQuery: teamMembershipsQuery,
        teamType: 'Team of Teams',
      }),
    },
  ]

  useEffect(() => {
    dispatch(retrieveTeam({ key, type: 'Team of Teams' }))
  }, [key, dispatch])

  useEffect(() => {
    team && dispatch(setBreadcrumbTitle({ title: team.name, pathname }))
  }, [team, dispatch, pathname])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  // doesn't trigger on first render
  const onTabChange = useCallback(
    (tabKey) => {
      setActiveTab(tabKey)

      // enables the query for the tab on first render if it hasn't been enabled yet
      if (tabKey == TeamOfTeamsTabs.RiskManagement && !risksQueryEnabled) {
        setRisksQueryEnabled(true)
      } else if (
        tabKey == TeamOfTeamsTabs.TeamMemberships &&
        !teamMembershipsQueryEnabled
      ) {
        setTeamMembershipsQueryEnabled(true)
      }
    },
    [risksQueryEnabled, teamMembershipsQueryEnabled],
  )

  const onCreateTeamMembershipFormClosed = (wasSaved: boolean) => {
    setOpenCreateTeamMembershipForm(false)
    if (wasSaved) {
      dispatch(retrieveTeam({ key, type: 'Team of Teams' }))
    }
  }

  const onDeactivateTeamFormClosed = (wasSaved: boolean) => {
    setOpenDeactivateTeamForm(false)
    if (wasSaved) {
      dispatch(retrieveTeam({ key, type: 'Team of Teams' }))
    }
  }

  if (teamNotFound) {
    return notFound()
  }

  return (
    <>
      {contextHolder}
      <PageTitle
        title={team?.name}
        subtitle="Team of Teams Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>
      {isInEditMode && team && canUpdateTeam && <EditTeamForm team={team} />}
      {openCreateTeamMembershipForm && (
        <CreateTeamMembershipForm
          showForm={openCreateTeamMembershipForm}
          teamId={team?.id}
          teamType={'Team of Teams'}
          onFormCreate={() => onCreateTeamMembershipFormClosed(true)}
          onFormCancel={() => onCreateTeamMembershipFormClosed(false)}
        />
      )}
      {openDeactivateTeamForm && (
        <DeactivateTeamOfTeamsForm
          team={team}
          showForm={openDeactivateTeamForm}
          onFormComplete={() => onDeactivateTeamFormClosed(true)}
          onFormCancel={() => onDeactivateTeamFormClosed(false)}
          messageApi={messageApi}
        />
      )}
    </>
  )
}

const TeamOfTeamsDetailsPageWithAuthorization = authorizePage(
  TeamOfTeamsDetailsPage,
  'Permission',
  'Permissions.Teams.View',
)

export default TeamOfTeamsDetailsPageWithAuthorization
