'use client'

import PageTitle from '@/src/app/components/common/page-title'
import ModaGrid from '../../components/common/moda-grid'
import { useCallback, useMemo, useState } from 'react'
import { BackgroundJobDto } from '@/src/services/moda-api'
import { getBackgroundJobsClient } from '@/src/services/clients'
import { authorizePage } from '../../components/hoc'

const Page = () => {
  const [backgroundJobs, setBackgroundJobs] = useState<BackgroundJobDto[]>([])

  const columnDefs = useMemo(
    () => [
      { field: 'id' },
      { field: 'action' },
      { field: 'status' },
      { field: 'type' },
      { field: 'namespace' },
      { field: 'startedAt', headerName: 'Start (UTC)' },
    ],
    []
  )

  const getRunningJobs = useCallback(async () => {
    const backgroundJobsClient = await getBackgroundJobsClient()
    const jobDtos = await backgroundJobsClient.getRunningJobs()
    setBackgroundJobs(jobDtos)
  }, [])

  return (
    <>
      <PageTitle title="Background Jobs" />

      <ModaGrid
        columnDefs={columnDefs}
        rowData={backgroundJobs}
        loadData={getRunningJobs}
      />
    </>
  )
}

const PageWithAuthorization = authorizePage(
  Page,
  'Permission',
  'Permissions.BackgroundJobs.View'
)

export default PageWithAuthorization
