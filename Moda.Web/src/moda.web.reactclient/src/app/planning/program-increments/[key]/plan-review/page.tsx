'use client'

import { useDocumentTitle } from '@/src/app/hooks'
import { ProgramIncrementTeamResponse } from '@/src/services/moda-api'
import { useEffect, useMemo, useState } from 'react'
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
  const [predictability, setPredictability] = useState<number>()
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
    setPredictability(programIncrementData?.predictability)

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
    if (!activeTab && teams?.length > 0) {
      const active = teams[0].code
      setActiveTab(active)
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [teams])

  const tabs = useMemo(
    () =>
      teams?.map((team) => {
        return {
          key: team.code,
          tab: team.code,
          content: <div>{team.code}</div>,
        }
      }),
    [teams],
  )

  const activeTeam = (): ProgramIncrementTeamResponse => {
    return teams?.find((t) => t.code === activeTab)
  }

  if (!isLoading && !isFetching && !programIncrementData) {
    notFound()
  }

  const refreshProgramIncrement = () => {
    refetchProgramIncrement()
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
          refreshProgramIncrement={refreshProgramIncrement}
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
          predictability != null && (
            <Tag title="PI Predictability">{`${predictability}%`}</Tag>
          )
        }
        actions={<Actions />}
      />
      <PageContent />
    </>
  )
}

export default ProgramIncrementPlanReviewPage
