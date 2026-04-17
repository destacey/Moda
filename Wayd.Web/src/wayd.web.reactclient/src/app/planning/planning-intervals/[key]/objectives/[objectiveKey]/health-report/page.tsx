'use client'

import PageTitle from '@/src/components/common/page-title'
import { use, useEffect } from 'react'
import { useDocumentTitle } from '@/src/hooks/use-document-title'
import { authorizePage } from '@/src/components/hoc'
import { notFound, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import HealthCheckTag from '@/src/components/common/health-check/health-check-tag'
import HealthReportGrid from '@/src/components/common/health-check/health-report-grid'
import { useGetPlanningIntervalObjectiveQuery } from '@/src/store/features/planning/planning-interval-api'

const ObjectiveHealthReportPage = (props: {
  params: Promise<{ key: number; objectiveKey: number }>
}) => {
  const { key: planningIntervalKey, objectiveKey } = use(props.params)

  useDocumentTitle('PI Objective Health Report')

  const { data: objectiveData, isLoading } =
    useGetPlanningIntervalObjectiveQuery({
      planningIntervalKey: planningIntervalKey.toString(),
      objectiveKey: objectiveKey.toString(),
    })

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  useEffect(() => {
    if (!objectiveData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/planning-intervals`,
        title: 'Planning Intervals',
      },
    ]

    breadcrumbRoute.push(
      {
        href: `/planning/planning-intervals/${objectiveData.planningInterval?.key}`,
        title: objectiveData.planningInterval?.name,
      },
      {
        href: `/planning/planning-intervals/${objectiveData.planningInterval?.key}/objectives/${objectiveData.key}`,
        title: 'PI Objective Details',
      },
      {
        title: 'Health Report',
      },
    )
    // TODO: for a split second, the breadcrumb shows the default path route, then the new one.
    dispatch(
      setBreadcrumbRoute({
        pathname,
        route: breadcrumbRoute,
      }),
    )
  }, [dispatch, objectiveData, pathname])

  if (!isLoading && !objectiveData) {
    return notFound()
  }

  return (
    <>
      <PageTitle
        title={`${objectiveData?.key} - ${objectiveData?.name}`}
        subtitle="PI Objective Health Report"
        tags={<HealthCheckTag healthCheck={objectiveData?.healthCheck} />}
      />
      {objectiveData?.id && <HealthReportGrid objectId={objectiveData.id} />}
    </>
  )
}

const ObjectiveHealthReportPageWithAuthorization = authorizePage(
  ObjectiveHealthReportPage,
  'Permission',
  'Permissions.PlanningIntervals.View',
)

export default ObjectiveHealthReportPageWithAuthorization
