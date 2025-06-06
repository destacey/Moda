'use client'

import { PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { useGetStrategicThemesQuery } from '@/src/store/features/strategic-management/strategic-themes-api'
import { Button } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { CreateStrategicThemeForm, StrategicThemesGrid } from './_components'
import { useMessage } from '@/src/components/contexts/messaging'

const StrategicThemesPage: React.FC = () => {
  useDocumentTitle('Strategic Themes')

  const [openCreateStrategicThemeForm, setOpenCreateStrategicThemeForm] =
    useState<boolean>(false)

  const messageApi = useMessage()

  const { hasPermissionClaim } = useAuth()
  const canCreateStrategicTheme = hasPermissionClaim(
    'Permissions.StrategicThemes.Create',
  )
  const showActions = canCreateStrategicTheme

  const {
    data: strategicThemesData,
    isLoading,
    error,
    refetch,
  } = useGetStrategicThemesQuery(undefined)

  useEffect(() => {
    if (error) {
      console.error(error)
      messageApi.error('Failed to load strategic themes.')
    }
  }, [error, messageApi])

  const refresh = useCallback(async () => {
    refetch()
  }, [refetch])

  const actions = useMemo(() => {
    return (
      <>
        {canCreateStrategicTheme && (
          <Button onClick={() => setOpenCreateStrategicThemeForm(true)}>
            Create Strategic Theme
          </Button>
        )}
      </>
    )
  }, [canCreateStrategicTheme])

  return (
    <>
      <PageTitle title="Strategic Themes" actions={showActions && actions} />
      <StrategicThemesGrid
        strategicThemesData={strategicThemesData || []}
        strategicThemesLoading={isLoading}
        refreshStrategicThemes={refresh}
      />
      {openCreateStrategicThemeForm && (
        <CreateStrategicThemeForm
          showForm={openCreateStrategicThemeForm}
          onFormComplete={() => setOpenCreateStrategicThemeForm(false)}
          onFormCancel={() => setOpenCreateStrategicThemeForm(false)}
        />
      )}
    </>
  )
}

const StrategicThemesPageWithAuthorization = authorizePage(
  StrategicThemesPage,
  'Permission',
  'Permissions.StrategicThemes.View',
)

export default StrategicThemesPageWithAuthorization
