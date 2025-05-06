'use client'

import ModaGrid from '@/src/components/common/moda-grid'
import PageTitle from '@/src/components/common/page-title'
import Link from 'next/link'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { useDocumentTitle } from '../../../hooks/use-document-title'
import dayjs from 'dayjs'
import { CreatePlanningIntervalForm } from './_components'
import useAuth from '../../../components/contexts/auth'
import { Button } from 'antd'
import { authorizePage } from '../../../components/hoc'
import { useGetPlanningIntervalsQuery } from '@/src/store/features/planning/planning-interval-api'
import { PlanningIntervalListDto } from '@/src/services/moda-api'

const PlanningIntervalLinkCellRenderer = ({ value, data }) => {
  return <Link href={`/planning/planning-intervals/${data.key}`}>{value}</Link>
}

const stateOrder = ['Active', 'Future', 'Completed']

const PlanningIntervalListPage = () => {
  useDocumentTitle('Planning Intervals')

  const [data, setData] = useState<PlanningIntervalListDto[]>([])
  const [openCreatePlanningIntervalForm, setOpenCreatePlanningIntervalForm] =
    useState<boolean>(false)

  const {
    data: piData,
    isLoading,
    error,
    refetch,
  } = useGetPlanningIntervalsQuery()

  const { hasPermissionClaim } = useAuth()
  const canCreatePlanningInterval = hasPermissionClaim(
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
    refetch()
  }, [refetch])

  useEffect(() => {
    if (!piData) return

    const sortedData = piData.slice().sort((a, b) => {
      const aStateIndex = stateOrder.indexOf(a.state)
      const bStateIndex = stateOrder.indexOf(b.state)
      if (aStateIndex !== bStateIndex) {
        return aStateIndex - bStateIndex
      } else {
        return dayjs(b.start).unix() - dayjs(a.start).unix()
      }
    })

    setData(sortedData)
  }, [piData])

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
      <ModaGrid
        columnDefs={columnDefs}
        rowData={data}
        loading={isLoading}
        loadData={refresh}
      />
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
