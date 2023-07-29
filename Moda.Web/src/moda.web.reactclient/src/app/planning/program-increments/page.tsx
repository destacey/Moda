'use client'

import ModaGrid from '@/src/app/components/common/moda-grid'
import PageTitle from '@/src/app/components/common/page-title'
import { getProgramIncrementsClient } from '@/src/services/clients'
import { ProgramIncrementListDto } from '@/src/services/moda-api'
import Link from 'next/link'
import { useCallback, useMemo, useState } from 'react'
import { useDocumentTitle } from '../../hooks/use-document-title'
import dayjs from 'dayjs'
import { CreateProgramIncrementForm } from '../components'
import useAuth from '../../components/contexts/auth'
import { Button } from 'antd'
import { last } from 'lodash'

const ProgramIncrementLinkCellRenderer = ({ value, data }) => {
  return (
    <Link href={`/planning/program-increments/${data.localId}`}>{value}</Link>
  )
}

const ProgramIncrementListPage = () => {
  useDocumentTitle('Program Increments')
  const [programIncrements, setProgramIncrements] = useState<
    ProgramIncrementListDto[]
  >([])
  const [openCreateProgramIncrementForm, setOpenCreateProgramIncrementForm] =
    useState<boolean>(false)
  const [lastRefresh, setLastRefresh] = useState<number>(Date.now())

  const { hasClaim } = useAuth()
  const canCreateProgramIncrement = hasClaim(
    'Permission',
    'Permissions.ProgramIncrements.Create'
  )
  const showActions = canCreateProgramIncrement

  // TODO: dates are formatted correctly and filter, but the filter is string based, not date based
  const columnDefs = useMemo(
    () => [
      { field: 'localId', headerName: '#', width: 90 },
      { field: 'name', cellRenderer: ProgramIncrementLinkCellRenderer },
      { field: 'state', width: 125 },
      {
        field: 'start',
        valueGetter: (params) => dayjs(params.data.start).format('M/D/YYYY'),
      },
      {
        field: 'end',
        valueGetter: (params) => dayjs(params.data.end).format('M/D/YYYY'),
      },
    ],
    []
  )

  const getProgramIncrements = useCallback(async () => {
    const programIncrementClient = await getProgramIncrementsClient()
    const programIncrementDtos = await programIncrementClient.getList()

    setProgramIncrements(
      programIncrementDtos.sort((a, b) => {
        const stateOrder = ['Active', 'Future', 'Completed']
        const aStateIndex = stateOrder.indexOf(a.state)
        const bStateIndex = stateOrder.indexOf(b.state)
        if (aStateIndex !== bStateIndex) {
          return aStateIndex - bStateIndex
        } else {
          return new Date(a.start).getTime() - new Date(b.start).getTime()
        }
      })
    )
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [lastRefresh])

  const onCreateProgramIncrementFormClosed = (wasCreated: boolean) => {
    setOpenCreateProgramIncrementForm(false)
    if (wasCreated) {
      setLastRefresh(Date.now())
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
        rowData={programIncrements}
        loadData={getProgramIncrements}
      />
      {canCreateProgramIncrement && (
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
