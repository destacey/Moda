'use client'

import { ModaGrid, PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { ExpenditureCategoryListDto } from '@/src/services/moda-api'
import { useGetExpenditureCategoriesQuery } from '@/src/store/features/ppm/expenditure-categories-api'
import { ColDef } from 'ag-grid-community'
import { Button } from 'antd'
import Link from 'next/link'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { CreateExpenditureCategoryForm } from './_components'
import { useMessage } from '@/src/components/contexts/messaging'

const ExpenditureCategoryCellRenderer = ({ value, data }) => {
  return <Link href={`./expenditure-categories/${data.id}`}>{value}</Link>
}

const ExpenditureCategoriesPage = () => {
  useDocumentTitle('PPM - Expenditure Categories')
  const [
    openCreateExpenditureCategoryForm,
    setOpenCreateExpenditureCategoryForm,
  ] = useState<boolean>(false)

  const messageApi = useMessage()

  const {
    data: categoryData,
    isLoading,
    error,
    refetch,
  } = useGetExpenditureCategoriesQuery()

  const { hasPermissionClaim } = useAuth()
  const canCreateExpenditureCategory = hasPermissionClaim(
    'Permissions.ExpenditureCategories.Create',
  )
  const showActions = canCreateExpenditureCategory

  useEffect(() => {
    if (error) {
      messageApi.error(
        error.detail ??
          'An error occurred while loading expenditure categories',
      )
      console.error(error)
    }
  }, [error, messageApi])

  const columnDefs = useMemo<ColDef<ExpenditureCategoryListDto>[]>(
    () => [
      { field: 'id', hide: true },
      { field: 'name', cellRenderer: ExpenditureCategoryCellRenderer },
      { field: 'state.name', headerName: 'State', width: 100 },
      {
        field: 'isCapitalizable',
        headerName: 'Capitalizable',
        width: 100,
      },
      {
        field: 'requiresDepreciation',
        headerName: 'Requires Depreciation',
        width: 150,
      },
      { field: 'accountingCode', headerName: 'Accounting Code', width: 150 },
    ],
    [],
  )

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const actions = useMemo(() => {
    if (!showActions) return null
    return (
      <>
        {canCreateExpenditureCategory && (
          <Button onClick={() => setOpenCreateExpenditureCategoryForm(true)}>
            Create Expenditure Category
          </Button>
        )}
      </>
    )
  }, [canCreateExpenditureCategory, showActions])

  const onCreateExpenditureCategoryFormClosed = (wasCreated: boolean) => {
    setOpenCreateExpenditureCategoryForm(false)
    if (wasCreated) {
      refetch()
    }
  }

  return (
    <>
      <PageTitle title="Expenditure Categories" actions={actions} />

      <ModaGrid
        height={600}
        columnDefs={columnDefs}
        rowData={categoryData}
        loadData={refresh}
        loading={isLoading}
      />
      {openCreateExpenditureCategoryForm && (
        <CreateExpenditureCategoryForm
          showForm={openCreateExpenditureCategoryForm}
          onFormComplete={() => onCreateExpenditureCategoryFormClosed(true)}
          onFormCancel={() => onCreateExpenditureCategoryFormClosed(false)}
        />
      )}
    </>
  )
}

const ExpenditureCategoriesPageWithAuthorization = authorizePage(
  ExpenditureCategoriesPage,
  'Permission',
  'Permissions.ExpenditureCategories.View',
)

export default ExpenditureCategoriesPageWithAuthorization
