'use client'

import { useDocumentTitle } from '@/src/hooks'
import {
  useGetPlanningInterval,
  useGetPlanningIntervalRisks,
} from '@/src/services/queries/planning-queries'
import { useCallback, useState } from 'react'
import { PageTitle } from '@/src/components/common'
import { notFound } from 'next/navigation'
import RisksGrid from '@/src/components/common/planning/risks-grid'
import { authorizePage } from '@/src/components/hoc'

const PlanningIntervalRisksPage = ({ params }) => {
  useDocumentTitle('PI Risks')
  const [includeClosedRisks, setIncludeClosedRisks] = useState<boolean>(false)

  const {
    data: planningIntervalData,
    isLoading,
    isFetching,
  } = useGetPlanningInterval(params.key)

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
