'use client'

import { useDocumentTitle } from '@/src/app/hooks'
import { ProgramIncrementTeamResponse } from '@/src/services/moda-api'
import { useEffect, useState } from 'react'
import { Card, Tag } from 'antd'
import Link from 'next/link'
import TeamPlanReview from './team-plan-review'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { notFound, usePathname } from 'next/navigation'
import { ModaEmpty, PageTitle } from '@/src/app/components/common'
import {
  useGetProgramIncrementByKey,
  useGetProgramIncrementTeams,
} from '@/src/services/queries/planning-queries'

const ProgramIncrementPlanReviewPage = ({ params }) => {
  useDocumentTitle('PI Plan Review')
  const [teams, setTeams] = useState<ProgramIncrementTeamResponse[]>([])
  const [activeTab, setActiveTab] = useState<string>()
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const {
    data: programIncrementData,
    isLoading,
    isFetching,
    refetch: refetchProgramIncrement,
  } = useGetProgramIncrementByKey(params.key)

  const { data: teamData } = useGetProgramIncrementTeams(
    programIncrementData?.id,
    true,
  )

  useEffect(() => {
    if (programIncrementData == null) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/program-increments`,
        title: 'Program Increments',
      },
      {
        href: `/planning/program-increments/${programIncrementData?.key}`,
        title: programIncrementData?.name,
      },
      {
        title: 'Plan Review',
      },
    ]
    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))

    setTeams(
      teamData
        ?.sort((a, b) => a.code.localeCompare(b.code))
        .filter((t) => t.type === 'Team'),
    )
  }, [dispatch, params.key, pathname, programIncrementData, teamData])

  useEffect(() => {
    if (teams?.length > 0) {
      setActiveTab(teams[0].key.toString())
    }
  }, [teams])

  const tabs = teams?.map((team) => {
    return {
      key: team.key.toString(),
      tab: team.code,
      content: <div>{team.code}</div>,
    }
  })

  const activeTeam = () => {
    return teams?.find((t) => t.key.toString() === activeTab)
  }

  if (!isLoading && !isFetching && !programIncrementData) {
    notFound()
  }

  const Actions = () => {
    return (
      <>
        <Link
          href={`/planning/program-increments/${programIncrementData?.key}`}
        >
          PI Details
        </Link>
      </>
    )
  }

  const PageContent = () => {
    if (programIncrementData == null) return null
    if (tabs?.length === 0) {
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
          programIncrement={programIncrementData}
          team={activeTeam()}
        />
      </Card>
    )
  }

  return (
    <>
      <PageTitle
        title={programIncrementData?.name}
        subtitle="PI Plan Review"
        tags={
          programIncrementData?.predictability != null && (
            <Tag title="PI Predictability">{`${programIncrementData?.predictability}%`}</Tag>
          )
        }
        actions={<Actions />}
      />
      <PageContent />
    </>
  )
}

export default ProgramIncrementPlanReviewPage
