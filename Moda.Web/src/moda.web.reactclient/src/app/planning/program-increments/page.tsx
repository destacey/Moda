'use client'

import ModaGrid from '@/src/app/components/common/moda-grid'
import PageTitle from '@/src/app/components/common/page-title'
import { ProgramIncrementListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import { useDocumentTitle } from '../../hooks/use-document-title'
import dayjs from 'dayjs'
import { CreateProgramIncrementForm } from '../components'
import useAuth from '../../components/contexts/auth'
import { Button } from 'antd'
import { useGetProgramIncrements } from '@/src/services/queries/planning-queries'

const ProgramIncrementLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/planning/program-increments/${data.key}`}>{value}</Link>
  )
}

const stateOrder = ['Active', 'Future', 'Completed']
const sortProgramIncrements = (data: ProgramIncrementListDto[]) => {
  return data?.sort((a, b) => {
    const aStateIndex = stateOrder.indexOf(a.state)
    const bStateIndex = stateOrder.indexOf(b.state)
    if (aStateIndex !== bStateIndex) {
      return aStateIndex - bStateIndex
    } else {
      return dayjs(b.start).unix() - dayjs(a.start).unix()
    }
  })
}

const ProgramIncrementListPage = () => {
  useDocumentTitle('Program Increments')
  const [openCreateProgramIncrementForm, setOpenCreateProgramIncrementForm] =
    useState<boolean>(false)

  const { data, refetch } = useGetProgramIncrements()

  const { hasClaim } = useAuth()
  const canCreateProgramIncrement = hasClaim(
    'Permission',
    'Permissions.ProgramIncrements.Create',
  )
  const showActions = canCreateProgramIncrement

  // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
  const columnDefs = useMemo(
    () => [
      { field: 'key', width: 90 },
      { field: 'name', cellRenderer: ProgramIncrementLinkCellRenderer },
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

  const onCreateProgramIncrementFormClosed = (wasCreated: boolean) => {
    setOpenCreateProgramIncrementForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  const Actions = () => {
    return (
      <>
        {canCreateProgramIncrement && (
          <Button onClick={() => setOpenCreateProgramIncrementForm(true)}>
            Create Program Increment
          </Button>
        )}
      </>
    )
  }

  return (
    <>
      <PageTitle
        title="Program Increments"
        actions={showActions && <Actions />}
      />
      <ModaGrid
        columnDefs={columnDefs}
        rowData={sortProgramIncrements(data)}
        loadData={refresh}
      />
      {openCreateProgramIncrementForm && (
        <CreateProgramIncrementForm
          showForm={openCreateProgramIncrementForm}
          onFormCreate={() => onCreateProgramIncrementFormClosed(true)}
          onFormCancel={() => onCreateProgramIncrementFormClosed(false)}
        />
      )}
    </>
  )
}

export default ProgramIncrementListPage
