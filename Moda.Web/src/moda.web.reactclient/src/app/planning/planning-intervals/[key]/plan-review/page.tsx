'use client'

import { useDocumentTitle } from '@/src/app/hooks'
import { PlanningIntervalTeamResponse } from '@/src/services/moda-api'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { Card, Tag } from 'antd'
import TeamPlanReview from './team-plan-review'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { notFound, usePathname } from 'next/navigation'
import { ModaEmpty, PageTitle } from '@/src/app/components/common'
import {
  useGetPlanningIntervalByKey,
  useGetPlanningIntervalTeams,
} from '@/src/services/queries/planning-queries'
import PlanningIntervalPlanReviewLoading from './loading'
import { authorizePage } from '@/src/app/components/hoc'

const PlanningIntervalPlanReviewPage = ({ params }) => {
  useDocumentTitle('PI Plan Review')
  const [teams, setTeams] = useState<PlanningIntervalTeamResponse[]>([])
  const [activeTab, setActiveTab] = useState<string>()
  const [predictability, setPredictability] = useState<number>()
  const dispatch = useAppDispatch()
  const pathname = usePathname()

  const {
    data: planningIntervalData,
    isLoading,
    isFetching,
    refetch: refetchPlanningInterval,
  } = useGetPlanningIntervalByKey(params.key)

  const { data: teamData } = useGetPlanningIntervalTeams(
    planningIntervalData?.id,
    true,
  )

  useEffect(() => {
    if (planningIntervalData == null) return
    setPredictability(planningIntervalData?.predictability)

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/planning-intervals`,
        title: 'Planning Intervals',
      },
      {
        href: `/planning/planning-intervals/${planningIntervalData?.key}`,
        title: planningIntervalData?.name,
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
  }, [dispatch, params.key, pathname, planningIntervalData, teamData])

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

  const activeTeam = useMemo((): PlanningIntervalTeamResponse => {
    return teams?.find((t) => t.code === activeTab)
  }, [activeTab, teams])

  if (!isLoading && !isFetching && !planningIntervalData) {
    notFound()
  }

  const refreshPlanningInterval = useCallback(() => {
    refetchPlanningInterval()
  }, [refetchPlanningInterval])

  const pageContent = useMemo(() => {
    if (planningIntervalData == null) return null
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
          planningInterval={planningIntervalData}
          team={activeTeam}
          refreshPlanningInterval={refreshPlanningInterval}
        />
      </Card>
    )
  }, [
    activeTab,
    activeTeam,
    planningIntervalData,
    refreshPlanningInterval,
    tabs,
  ])

  if (isLoading) {
    return <PlanningIntervalPlanReviewLoading />
  }

  return (
    <>
      <PageTitle
        title="PI Plan Review"
        tags={
          predictability != null && (
            <Tag title="PI Predictability">{`${predictability}%`}</Tag>
          )
        }
      />
      {pageContent}
    </>
  )
}

const PlanningIntervalPlanReviewPageWithAuthorization = authorizePage(
  PlanningIntervalPlanReviewPage,
  'Permission',
  'Permissions.PlanningIntervals.View',
)

export default PlanningIntervalPlanReviewPageWithAuthorization
