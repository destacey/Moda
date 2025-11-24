'use client'

import { useDocumentTitle } from '@/src/hooks'
import { PlanningIntervalTeamResponse } from '@/src/services/moda-api'
import { use, useEffect, useMemo, useState } from 'react'
import { Alert, Card, Tag } from 'antd'
import TeamPlanReview from './team-plan-review'
import { notFound, useRouter } from 'next/navigation'
import { ModaEmpty, PageTitle } from '@/src/components/common'
import PlanningIntervalPlanReviewLoading from './loading'
import { authorizePage } from '@/src/components/hoc'
import {
  useGetPlanningIntervalQuery,
  useGetPlanningIntervalTeamsQuery,
} from '@/src/store/features/planning/planning-interval-api'

const PlanningIntervalPlanReviewPage = (props: {
  params: Promise<{ key: string }>
}) => {
  const { key } = use(props.params)
  const piKey = Number(key)

  useDocumentTitle('PI Plan Review')
  const router = useRouter()

  const {
    data: planningIntervalData,
    isLoading,
    error,
    refetch: refetchPlanningInterval,
  } = useGetPlanningIntervalQuery(piKey)

  const { data: teamData, isLoading: teamsIsLoading } =
    useGetPlanningIntervalTeamsQuery(piKey)

  const predictability = planningIntervalData?.predictability

  const teams = useMemo(() => {
    if (!teamData) return []
    return teamData
      .filter((t) => t.type === 'Team')
      .sort((a, b) => a.code.localeCompare(b.code))
  }, [teamData])

  // Track user interactions separately from derived state
  const [userSelectedTab, setUserSelectedTab] = useState<string | null>(null)

  // Derive activeTab: use user selection, or fall back to hash/first team
  const activeTab = useMemo(() => {
    if (userSelectedTab) return userSelectedTab
    if (teams.length === 0) return ''

    const hash = typeof window !== 'undefined' ? window.location.hash.slice(1) : ''
    return hash && hash !== '' ? hash : teams[0]?.code.toLowerCase()
  }, [userSelectedTab, teams])

  // Update URL hash if it doesn't match activeTab - side effect only
  useEffect(() => {
    if (!activeTab || teams.length === 0) return

    const hash = window.location.hash.slice(1)
    if (!hash || hash === '') {
      router.replace(`#${activeTab}`, { scroll: false })
    }
  }, [activeTab, teams, router])

  // Handle hash change events
  useEffect(() => {
    const handleHashChange = () => {
      const hash = window.location.hash.slice(1)
      setUserSelectedTab(hash || null)
    }
    window.addEventListener('hashchange', handleHashChange)

    return () => {
      window.removeEventListener('hashchange', handleHashChange)
    }
  }, [])

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
        }
      }),
    [teams],
  )

  const activeTeam = useMemo((): PlanningIntervalTeamResponse => {
    if (!teams || teams.length === 0 || !activeTab) return null
    return teams?.find((t) => t.code.toLowerCase() === activeTab)
  }, [teams, activeTab])

  if (!isLoading && !planningIntervalData) {
    return notFound()
  }
  if (isLoading || teamsIsLoading) return <PlanningIntervalPlanReviewLoading />
  if (!planningIntervalData) return null
  if (tabs?.length === 0)
    return <ModaEmpty message="No teams found for this PI" />

  const tabExists = tabs?.some((t) => t.key === activeTab)

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
        onTabChange={(key) => setUserSelectedTab(key)}
      >
        {!tabExists ? (
          <Alert message="Please select a valid team." type="error" />
        ) : (
          <TeamPlanReview
            planningInterval={planningIntervalData}
            team={activeTeam}
            refreshPlanningInterval={refetchPlanningInterval}
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
