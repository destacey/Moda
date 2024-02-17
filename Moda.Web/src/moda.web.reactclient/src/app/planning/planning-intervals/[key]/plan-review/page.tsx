'use client'

import { useDocumentTitle } from '@/src/app/hooks'
import { PlanningIntervalTeamResponse } from '@/src/services/moda-api'
import { useEffect, useMemo, useState } from 'react'
import { Alert, Card, Tag } from 'antd'
import TeamPlanReview from './team-plan-review'
import { notFound, useRouter } from 'next/navigation'
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
  const [activeTab, setActiveTab] = useState<string>(null)
  const [predictability, setPredictability] = useState<number>()

  const router = useRouter()

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

    const currentTeams = teamData
      ?.sort((a, b) => a.code.localeCompare(b.code))
      .filter((t) => t.type === 'Team')

    setTeams(currentTeams)

    if (currentTeams?.length > 0) {
      const hash = window.location.hash.slice(1)
      const initialTeamCode =
        hash && hash !== '' ? hash : currentTeams[0].code.toLowerCase()

      if (!hash || hash === '') {
        router.replace(`#${initialTeamCode}`, { scroll: false })
      }

      if (currentTeams.some((t) => t.code.toLowerCase() === initialTeamCode)) {
        setActiveTab(initialTeamCode)
      }
    }

    const handleHashChange = () => {
      const hash = window.location.hash.slice(1)
      setActiveTab(hash)
    }
    window.addEventListener('hashchange', handleHashChange)

    return () => {
      window.removeEventListener('hashchange', handleHashChange)
    }
  }, [planningIntervalData, router, teamData])

  useEffect(() => {
    const hash = window.location.hash.slice(1)
    if (!hash || hash === '' || !activeTab || activeTab === hash) return

    router.push(`#${activeTab}`, { scroll: false })
  }, [activeTab, router])

  const tabs = useMemo(
    () =>
      teams?.map((team) => {
        return {
          key: team.code.toLowerCase(),
          tab: team.code,
          content: <div>{team.code}</div>,
        }
      }),
    [teams],
  )

  const activeTeam = useMemo((): PlanningIntervalTeamResponse => {
    if (!teams || teams.length === 0 || !activeTab) return null
    return teams?.find((t) => t.code.toLowerCase() === activeTab)
  }, [teams, activeTab])

  if (!isLoading && !isFetching && !planningIntervalData) {
    notFound()
  }
  if (isLoading) return <PlanningIntervalPlanReviewLoading />
  if (!planningIntervalData) return null
  if (tabs?.length === 0)
    return <ModaEmpty message="No teams found for this PI" />

  const tabExists = tabs.some((t) => t.key === activeTab)

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
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={(key) => setActiveTab(key)}
      >
        {!tabExists ? (
          <Alert message="Please select a valid team." type="error" />
        ) : (
          <TeamPlanReview
            planningInterval={planningIntervalData}
            team={activeTeam}
            refreshPlanningInterval={() => refetchPlanningInterval()}
          />
        )}
      </Card>
    </>
  )
}

const PlanningIntervalPlanReviewPageWithAuthorization = authorizePage(
  PlanningIntervalPlanReviewPage,
  'Permission',
  'Permissions.PlanningIntervals.View',
)

export default PlanningIntervalPlanReviewPageWithAuthorization
