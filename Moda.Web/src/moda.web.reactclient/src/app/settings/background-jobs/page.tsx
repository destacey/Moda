'use client'

import PageTitle from '@/src/app/components/common/page-title'
import ModaGrid from '../../components/common/moda-grid'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { BackgroundJobDto, BackgroundJobTypeDto } from '@/src/services/moda-api'
import { getBackgroundJobsClient } from '@/src/services/clients'
import { authorizePage } from '../../components/hoc'
import useAuth from '../../components/contexts/auth'
import { MenuProps } from 'antd'
import Link from 'next/link'
import { ItemType } from 'antd/es/menu/hooks/useItems'
import { useDocumentTitle } from '../../hooks'
import { PageActions } from '../../components/common'

const BackgroundJobsListPage = () => {
  useDocumentTitle('Background Jobs')
  const [jobTypes, setJobTypes] = useState<BackgroundJobTypeDto[]>([])
  const [backgroundJobs, setBackgroundJobs] = useState<BackgroundJobDto[]>([])
  const { hasClaim } = useAuth()
  const canViewHangfire = hasClaim('Permission', 'Permissions.Hangfire.View')
  const canRunBackgroundJobs = hasClaim(
    'Permission',
    'Permissions.BackgroundJobs.Create',
  )

  const columnDefs = useMemo(
    () => [
      { field: 'id' },
      { field: 'action' },
      { field: 'status' },
      { field: 'type' },
      { field: 'namespace' },
      { field: 'startedAt', headerName: 'Start (UTC)' },
    ],
    [],
  )

  const getJobTypes = useCallback(async () => {
    const backgroundJobsClient = await getBackgroundJobsClient()
    const jobTypes = await backgroundJobsClient.getJobTypes()
    setJobTypes(jobTypes)
  }, [])

  const getRunningJobs = useCallback(async () => {
    const backgroundJobsClient = await getBackgroundJobsClient()
    const jobDtos = await backgroundJobsClient.getRunningJobs()
    setBackgroundJobs(jobDtos)
  }, [])

  const runJob = useCallback(
    async (jobTypeId: number) => {
      const backgroundJobsClient = await getBackgroundJobsClient()
      await backgroundJobsClient.run(jobTypeId)
      getRunningJobs()
    },
    [getRunningJobs],
  )
  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const hangfireUrl = process.env.NEXT_PUBLIC_API_BASE_URL + '/jobs'
    const items: ItemType[] = []
    if (canViewHangfire) {
      items.push({
        label: (
          <Link href={hangfireUrl} target="_blank">
            Hangfire Dashboard
          </Link>
        ),
        key: 'view-hangfire',
      })
    }
    if (canRunBackgroundJobs && jobTypes.length > 0) {
      if (canViewHangfire) {
        items.push({
          type: 'divider',
        })
      }
      jobTypes.map((jobType) => {
        items.push({
          label: jobType.name,
          key: jobType.name,
          onClick: () => runJob(jobType.id),
        })
      })
    }
    return items
  }, [canViewHangfire, canRunBackgroundJobs, jobTypes, runJob])

  useEffect(() => {
    getJobTypes()
  }, [getJobTypes])

  return (
    <>
      <PageTitle
        title="Background Jobs"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        rowData={backgroundJobs}
        loadData={getRunningJobs}
      />
    </>
  )
}

const PageWithAuthorization = authorizePage(
  BackgroundJobsListPage,
  'Permission',
  'Permissions.BackgroundJobs.View',
)

export default PageWithAuthorization
