'use client'

import { useDocumentTitle } from '@/src/app/hooks'
import {
  useGetPlanningIntervalByKey,
  useGetPlanningIntervalRisks,
} from '@/src/services/queries/planning-queries'
import { useCallback, useMemo, useState } from 'react'
import { PageTitle } from '@/src/app/components/common'
import { notFound } from 'next/navigation'
import RisksGrid from '@/src/app/components/common/planning/risks-grid'
import { authorizePage } from '@/src/app/components/hoc'

const PlanningIntervalRisksPage = ({ params }) => {
  useDocumentTitle('PI Risks')
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)

  const {
    data: planningIntervalData,
    isLoading,
    isFetching,
    refetch: refetchPlanningInterval,
  } = useGetPlanningIntervalByKey(params.key)

  const risksQuery = useGetPlanningIntervalRisks(
    planningIntervalData?.id,
    includeClosedRisks,
  )

  if (!isLoading && !isFetching && !planningIntervalData) {
    notFound()
  }

  const onIncludeClosedRisksChanged = useCallback((includeClosed: boolean) => {
    setIncludeClosedRisks(includeClosed)
  }, [])

  return (
    <>
      <PageTitle title="PI Risks" />
      <RisksGrid
        risksQuery={risksQuery}
        updateIncludeClosed={onIncludeClosedRisksChanged}
        newRisksAllowed={true}
        hideTeamColumn={false}
        gridHeight={650}
      />
    </>
  )
}

const PlanningIntervalRisksPageWithAuthorization = authorizePage(
  PlanningIntervalRisksPage,
  'Permission',
  'Permissions.PlanningIntervals.View',
)

export default PlanningIntervalRisksPageWithAuthorization
