'use client'

import { PageTitle } from '@/src/app/components/common'
import useBreadcrumbs from '@/src/app/components/contexts/breadcrumbs'
import { useDocumentTitle } from '@/src/app/hooks'
import { getProgramIncrementsClient } from '@/src/services/clients'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementTeamResponse,
} from '@/src/services/moda-api'
import { ItemType } from 'antd/es/breadcrumb/Breadcrumb'
import { useCallback, useEffect, useState } from 'react'
import { Card } from 'antd'
import Link from 'next/link'
import TeamPlanReview from './team-plan-review'

const ProgramIncrementPlanReviewPage = ({ params }) => {
  useDocumentTitle('PI Plan Review')
  const [programIncrement, setProgramIncrement] =
    useState<ProgramIncrementDetailsDto>()
  const [teams, setTeams] = useState<ProgramIncrementTeamResponse[]>([])
  const [activeTab, setActiveTab] = useState<string>()
  const { setBreadcrumbRoute } = useBreadcrumbs()

  useEffect(() => {
    const breadcrumbRoute: ItemType[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/program-increments`,
        title: 'Program Increments',
      },
    ]

    const getProgramIncrement = async () => {
      const programIncrementsClient = await getProgramIncrementsClient()
      const programIncrementDto = await programIncrementsClient.getByLocalId(
        params.id
      )
      setProgramIncrement(programIncrementDto)

      breadcrumbRoute.push(
        {
          href: `/planning/program-increments/${programIncrement?.localId}`,
          title: programIncrement?.name,
        },
        {
          title: 'Plan Review',
        }
      )
      // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
      setBreadcrumbRoute(breadcrumbRoute)

      const teamData = await programIncrementsClient.getTeams(
        programIncrementDto.id
      )
      setTeams(
        teamData
          .sort((a, b) => a.code.localeCompare(b.code))
          .filter((t) => t.type === 'Team')
      )
    }

    getProgramIncrement()
  }, [
    params.id,
    programIncrement?.localId,
    programIncrement?.name,
    setBreadcrumbRoute,
  ])

  useEffect(() => {
    if (teams.length > 0) {
      setActiveTab(teams[0].localId.toString())
    }
  }, [teams])

  const tabs = teams.map((team) => {
    return {
      key: team.localId.toString(),
      tab: team.code,
      content: <div>{team.code}</div>,
    }
  })

  const activeTeam = () => {
    return teams.find((t) => t.localId.toString() === activeTab)
  }

  const Actions = () => {
    return (
      <>
        <Link
          href={`/planning/program-increments/${programIncrement?.localId}`}
        >
          PI Details
        </Link>
      </>
    )
  }

  return (
    <>
      <PageTitle
        title={programIncrement?.name}
        subtitle="PI Plan Review"
        actions={<Actions />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        <TeamPlanReview
          programIncrement={programIncrement}
          team={activeTeam()}
        />
      </Card>
    </>
  )
}

export default ProgramIncrementPlanReviewPage
