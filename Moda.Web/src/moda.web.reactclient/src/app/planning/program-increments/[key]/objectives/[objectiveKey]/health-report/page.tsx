'use client'

import PageTitle from '@/src/app/components/common/page-title'
import { useEffect } from 'react'
import { useDocumentTitle } from '@/src/app/hooks/use-document-title'
import { authorizePage } from '@/src/app/components/hoc'
import { useGetProgramIncrementObjectiveByKey } from '@/src/services/queries/planning-queries'
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
  } = useGetProgramIncrementObjectiveByKey(params.key, params.objectiveKey)

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  useEffect(() => {
    if (!objectiveData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Planning',
      },
      {
        href: `/planning/program-increments`,
        title: 'Program Increments',
      },
    ]

    breadcrumbRoute.push(
      {
        href: `/planning/program-increments/${objectiveData.programIncrement?.key}`,
        title: objectiveData.programIncrement?.name,
      },
      {
        href: `/planning/program-increments/${objectiveData.programIncrement?.key}/objectives/${objectiveData.key}`,
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
  'Permissions.ProgramIncrements.View',
)

export default ObjectiveHealthReportPageWithAuthorization
