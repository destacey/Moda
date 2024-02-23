'use client'

import ModaGrid from '@/src/app/components/common/moda-grid'
import PageTitle from '@/src/app/components/common/page-title'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import { useDocumentTitle } from '../../hooks/use-document-title'
import dayjs from 'dayjs'
import { CreatePlanningIntervalForm } from '../components'
import useAuth from '../../components/contexts/auth'
import { Button } from 'antd'
import { useGetPlanningIntervals } from '@/src/services/queries/planning-queries'
import { authorizePage } from '../../components/hoc'

const PlanningIntervalLinkCellRenderer = ({ value, data }) => {
  return <Link href={`/planning/planning-intervals/${data.key}`}>{value}</Link>
}

const PlanningIntervalListPage = () => {
  useDocumentTitle('Planning Intervals')
  const [openCreatePlanningIntervalForm, setOpenCreatePlanningIntervalForm] =
    useState<boolean>(false)

  const { data, refetch } = useGetPlanningIntervals()

  const { hasClaim } = useAuth()
  const canCreatePlanningInterval = hasClaim(
    'Permission',
    'Permissions.PlanningIntervals.Create',
  )
  const showActions = canCreatePlanningInterval

  // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
  const columnDefs = useMemo(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', cellRenderer: PlanningIntervalLinkCellRenderer },
      {
        field: 'state',
        width: 125,
      },
      {
        field: 'start',
        valueGetter: (params) => dayjs(params.data.start).format('M/D/YYYY'),
      },
      {
        field: 'end',
        valueGetter: (params) => dayjs(params.data.end).format('M/D/YYYY'),
      },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch
  }, [refetch])

  const onCreatePlanningIntervalFormClosed = (wasCreated: boolean) => {
    setOpenCreatePlanningIntervalForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  const actions = () => {
    return (
      <>
        {canCreatePlanningInterval && (
          <Button onClick={() => setOpenCreatePlanningIntervalForm(true)}>
            Create Planning Interval
          </Button>
        )}
      </>
    )
  }

  return (
    <>
      <br />
      <PageTitle
        title="Planning Intervals"
        actions={showActions && actions()}
      />
      <ModaGrid columnDefs={columnDefs} rowData={data} loadData={refresh} />
      {openCreatePlanningIntervalForm && (
        <CreatePlanningIntervalForm
          showForm={openCreatePlanningIntervalForm}
          onFormCreate={() => onCreatePlanningIntervalFormClosed(true)}
          onFormCancel={() => onCreatePlanningIntervalFormClosed(false)}
        />
      )}
    </>
  )
}

const PlanningIntervalListPageWithAuthorization = authorizePage(
  PlanningIntervalListPage,
  'Permission',
  'Permissions.PlanningIntervals.View',
)

export default PlanningIntervalListPageWithAuthorization
