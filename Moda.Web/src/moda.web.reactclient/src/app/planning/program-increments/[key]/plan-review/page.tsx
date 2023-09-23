'use client'

import { useDocumentTitle } from '@/src/app/hooks'
import { getProgramIncrementsClient } from '@/src/services/clients'
import {
  ProgramIncrementDetailsDto,
  ProgramIncrementTeamResponse,
} from '@/src/services/moda-api'
import { useEffect, useState } from 'react'
import { Card, Tag } from 'antd'
import Link from 'next/link'
import TeamPlanReview from './team-plan-review'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { usePathname } from 'next/navigation'
import { ModaEmpty, PageTitle } from '@/src/app/components/common'

const ProgramIncrementPlanReviewPage = ({ params }) => {
  useDocumentTitle('PI Plan Review')
  const [programIncrement, setProgramIncrement] =
    useState<ProgramIncrementDetailsDto>()
  const [teams, setTeams] = useState<ProgramIncrementTeamResponse[]>([])
  const [activeTab, setActiveTab] = useState<string>()
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  useEffect(() => {
    const breadcrumbRoute: BreadcrumbItem[] = [
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
      const programIncrementDto = await programIncrementsClient.getByKey(
        params.key,
      )
      setProgramIncrement(programIncrementDto)

      breadcrumbRoute.push(
        {
          href: `/planning/program-increments/${programIncrement?.key}`,
          title: programIncrement?.name,
        },
        {
          title: 'Plan Review',
        },
      )
      // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
      dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))

      const teamData = await programIncrementsClient.getTeams(
        programIncrementDto.id,
      )
      setTeams(
        teamData
          .sort((a, b) => a.code.localeCompare(b.code))
          .filter((t) => t.type === 'Team'),
      )
    }

    getProgramIncrement()
  }, [
    params.key,
    programIncrement?.key,
    programIncrement?.name,
    dispatch,
    pathname,
  ])

  useEffect(() => {
    if (teams.length > 0) {
      setActiveTab(teams[0].key.toString())
    }
  }, [teams])

  const tabs = teams.map((team) => {
    return {
      key: team.key.toString(),
      tab: team.code,
      content: <div>{team.code}</div>,
    }
  })

  const activeTeam = () => {
    return teams.find((t) => t.key.toString() === activeTab)
  }

  const Actions = () => {
    return (
      <>
        <Link href={`/planning/program-increments/${programIncrement?.key}`}>
          PI Details
        </Link>
      </>
    )
  }

  const PageContent = () => {
    if (programIncrement == null) return null
    if (tabs.length === 0) {
      return <ModaEmpty message="No teams found for this PI" />
    }

    return (
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
    )
  }

  return (
    <>
      <PageTitle
        title={programIncrement?.name}
        subtitle="PI Plan Review"
        tags={
          programIncrement?.predictability != null && (
            <Tag title="PI Predictability">{`${programIncrement?.predictability}%`}</Tag>
          )
        }
        actions={<Actions />}
      />
      <PageContent />
    </>
  )
}

export default ProgramIncrementPlanReviewPage
