'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useEffect } from 'react'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import { authorizePage } from '@/src/app/components/hoc'
import { useGetPlanningIntervalObjectiveByKey } from '@/src/services/queries/planning-queries'
import { notFound, usePathname } from 'next/navigation'
import { useAppDispatch } from '@/src/app/hooks'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import HealthCheckTag from '@/src/app/components/common/health-check/health-check-tag'
import HealthReportGrid from '@/src/app/components/common/health-check/health-report-grid'

const ObjectiveHealthReportPage = ({ params }) => {
  useDocumentTitle('PI Objective Health Report')

  const {
    data: objectiveData,
    isLoading,
    isFetching,
  } = useGetPlanningIntervalObjectiveByKey(params.key, params.objectiveKey)

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

  if (!isLoading && !isFetching && !objectiveData) {
    notFound()
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
