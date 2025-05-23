'use client'

import PageTitle from '@/src/components/common/page-title'
import ModaGrid from '../../../components/common/moda-grid'
import { useCallback, useMemo, useState } from 'react'
import { BackgroundJobDto } from '@/src/services/moda-api'
import { getBackgroundJobsClient } from '@/src/services/clients'
import { authorizePage } from '../../../components/hoc'
import useAuth from '../../../components/contexts/auth'
import { MenuProps } from 'antd'
import Link from 'next/link'
import { ItemType } from 'antd/es/menu/interface'
import { useDocumentTitle } from '../../../hooks'
import { PageActions } from '../../../components/common'
import { useGetJobTypesQuery } from '@/src/store/features/admin/background-jobs-api'
import CreateRecurringJobForm from './create-recurring-job-form'
import { ColDef } from 'ag-grid-community'

const BackgroundJobsListPage = () => {
  useDocumentTitle('Background Jobs')
  const [backgroundJobs, setBackgroundJobs] = useState<BackgroundJobDto[]>([])
  const [openCreateRecurringJobForm, setOpenCreateRecurringJobForm] =
    useState(false)

  const { hasClaim } = useAuth()
  const canViewHangfire = hasClaim('Permission', 'Permissions.Hangfire.View')
  const canRunBackgroundJobs = hasClaim(
    'Permission',
    'Permissions.BackgroundJobs.Create',
  )

  const columnDefs = useMemo<ColDef<BackgroundJobDto>[]>(
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

  const { data: jobTypeData = [] } = useGetJobTypesQuery()

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
    if (canRunBackgroundJobs && jobTypeData.length > 0) {
      if (canViewHangfire) {
        items.push({
          type: 'divider',
        })
      }
      items.push({
        label: 'Create Recurring Job',
        key: 'create-recurring-job',
        onClick: () => setOpenCreateRecurringJobForm(true),
      })
      items.push({
        type: 'divider',
      })

      // Sort jobTypeData by groupName and then by order
      const sortedJobTypeData = [...jobTypeData].sort((a, b) => {
        if (a.groupName === b.groupName) {
          return a.order - b.order
        }
        return a.groupName.localeCompare(b.groupName)
      })

      let currentGroupName = ''
      let currentGroupItems: ItemType[] = []

      sortedJobTypeData.forEach((jobType) => {
        if (jobType.groupName !== currentGroupName) {
          if (currentGroupName !== '') {
            items.push({
              key: currentGroupName,
              type: 'group',
              label: currentGroupName,
              children: currentGroupItems,
            })
            items.push({
              type: 'divider',
            })
          }
          currentGroupName = jobType.groupName
          currentGroupItems = []
        }
        currentGroupItems.push({
          label: jobType.name,
          key: jobType.name,
          onClick: () => runJob(jobType.id),
        })
      })

      // Add the last group
      if (currentGroupName !== '') {
        items.push({
          key: currentGroupName,
          type: 'group',
          label: currentGroupName,
          children: currentGroupItems,
        })
      }
    }
    return items
  }, [canViewHangfire, canRunBackgroundJobs, jobTypeData, runJob])

  const onCreateRecurringJobFormClosed = (wasSaved: boolean) => {
    setOpenCreateRecurringJobForm(false)
    if (wasSaved) {
      getRunningJobs()
    }
  }

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
      {openCreateRecurringJobForm && (
        <CreateRecurringJobForm
          showForm={openCreateRecurringJobForm}
          jobTypes={jobTypeData}
          onFormCreate={() => onCreateRecurringJobFormClosed(true)}
          onFormCancel={() => onCreateRecurringJobFormClosed(false)}
        />
      )}
    </>
  )
}

const PageWithAuthorization = authorizePage(
  BackgroundJobsListPage,
  'Permission',
  'Permissions.BackgroundJobs.View',
)

export default PageWithAuthorization
