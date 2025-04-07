'use client'

import { PageActions, PageTitle } from '@/src/components/common'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { useGetStrategicThemeQuery } from '@/src/store/features/strategic-management/strategic-themes-api'
import { Descriptions, MenuProps, Space } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import StrategicThemeDetailsLoading from './loading'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { ItemType } from 'antd/es/menu/interface'
import {
  ChangeStrategicThemeStateForm,
  DeleteStrategicThemeForm,
  EditStrategicThemeForm,
} from '../_components'
import { StrategicThemeStateAction } from '../_components/change-strategic-theme-state-form'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item } = Descriptions

enum MenuActions {
  Edit = 'Edit',
  Delete = 'Delete',
  Activate = 'Activate',
  Archive = 'Archive',
}

const StrategicThemeDetailsPage = ({ params }) => {
  useDocumentTitle('Strategic Theme Details')
  const [openEditStrategicThemeForm, setOpenEditStrategicThemeForm] =
    useState<boolean>(false)
  const [openActivateStrategicThemeForm, setOpenActivateStrategicThemeForm] =
    useState<boolean>(false)
  const [openArchiveStrategicThemeForm, setOpenArchiveStrategicThemeForm] =
    useState<boolean>(false)
  const [openDeleteStrategicThemeForm, setOpenDeleteStrategicThemeForm] =
    useState<boolean>(false)

  const messageApi = useMessage()

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canUpdateStrategicTheme = hasPermissionClaim(
    'Permissions.StrategicThemes.Update',
  )
  const canDeleteStrategicTheme = hasPermissionClaim(
    'Permissions.StrategicThemes.Delete',
  )

  const {
    data: strategicThemeData,
    isLoading,
    error,
    refetch: refetchStrategicTheme,
  } = useGetStrategicThemeQuery(params.key)

  useEffect(() => {
    if (!strategicThemeData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'Strategic Management',
      },
      {
        href: `/strategic-management/strategic-themes`,
        title: 'Strategic Themes',
      },
    ]

    breadcrumbRoute.push({
      title: 'Details',
    })

    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname, strategicThemeData])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const currentState = strategicThemeData?.state.name
    const availableActions =
      currentState === 'Proposed'
        ? [MenuActions.Delete, MenuActions.Activate]
        : currentState === 'Active'
          ? [MenuActions.Archive]
          : []

    const items: ItemType[] = []
    if (canUpdateStrategicTheme) {
      items.push({
        key: 'edit',
        label: MenuActions.Edit,
        onClick: () => setOpenEditStrategicThemeForm(true),
      })
    }
    if (
      canDeleteStrategicTheme &&
      availableActions.includes(MenuActions.Delete)
    ) {
      items.push({
        key: 'delete',
        label: MenuActions.Delete,
        onClick: () => setOpenDeleteStrategicThemeForm(true),
      })
    }

    if (
      (canUpdateStrategicTheme &&
        availableActions.includes(MenuActions.Activate)) ||
      availableActions.includes(MenuActions.Archive)
    ) {
      items.push({
        key: 'manage-divider',
        type: 'divider',
      })
    }

    if (
      canUpdateStrategicTheme &&
      availableActions.includes(MenuActions.Activate)
    ) {
      items.push({
        key: 'activate',
        label: MenuActions.Activate,
        onClick: () => setOpenActivateStrategicThemeForm(true),
      })
    }

    if (
      canUpdateStrategicTheme &&
      availableActions.includes(MenuActions.Archive)
    ) {
      items.push({
        key: 'archive',
        label: MenuActions.Archive,
        onClick: () => setOpenArchiveStrategicThemeForm(true),
      })
    }

    return items
  }, [
    strategicThemeData?.state.name,
    canUpdateStrategicTheme,
    canDeleteStrategicTheme,
  ])

  const onEditStrategicThemeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditStrategicThemeForm(false)
      if (wasSaved) {
        refetchStrategicTheme()
      }
    },
    [refetchStrategicTheme],
  )

  const onActivateStrategicThemeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenActivateStrategicThemeForm(false)
      if (wasSaved) {
        refetchStrategicTheme()
      }
    },
    [refetchStrategicTheme],
  )

  const onArchiveStrategicThemeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenArchiveStrategicThemeForm(false)
      if (wasSaved) {
        refetchStrategicTheme()
      }
    },
    [refetchStrategicTheme],
  )

  const onDeleteStrategicThemeFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteStrategicThemeForm(false)
      if (wasDeleted) {
        router.push('/strategic-management/strategic-themes')
      }
    },
    [router],
  )

  if (isLoading) {
    return <StrategicThemeDetailsLoading />
  }

  if (!strategicThemeData) {
    notFound()
  }

  return (
    <>
      <PageTitle
        title={`${strategicThemeData?.key} - ${strategicThemeData?.name}`}
        subtitle="Strategic Theme Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Space direction="vertical" size="small">
        <Descriptions>
          <Item label="State">{strategicThemeData.state.name}</Item>
        </Descriptions>
        <Descriptions layout="vertical" size="small">
          <Item label="Description">
            <MarkdownRenderer markdown={strategicThemeData.description} />
          </Item>
        </Descriptions>
      </Space>

      {openEditStrategicThemeForm && (
        <EditStrategicThemeForm
          strategicThemeKey={strategicThemeData?.key}
          showForm={openEditStrategicThemeForm}
          onFormComplete={() => onEditStrategicThemeFormClosed(true)}
          onFormCancel={() => onEditStrategicThemeFormClosed(false)}
        />
      )}
      {openActivateStrategicThemeForm && (
        <ChangeStrategicThemeStateForm
          strategicTheme={strategicThemeData}
          stateAction={StrategicThemeStateAction.Activate}
          showForm={openActivateStrategicThemeForm}
          onFormComplete={() => onActivateStrategicThemeFormClosed(true)}
          onFormCancel={() => onActivateStrategicThemeFormClosed(false)}
        />
      )}
      {openArchiveStrategicThemeForm && (
        <ChangeStrategicThemeStateForm
          strategicTheme={strategicThemeData}
          stateAction={StrategicThemeStateAction.Archive}
          showForm={openArchiveStrategicThemeForm}
          onFormComplete={() => onArchiveStrategicThemeFormClosed(true)}
          onFormCancel={() => onArchiveStrategicThemeFormClosed(false)}
        />
      )}
      {openDeleteStrategicThemeForm && (
        <DeleteStrategicThemeForm
          strategicTheme={strategicThemeData}
          showForm={openDeleteStrategicThemeForm}
          onFormComplete={() => onDeleteStrategicThemeFormClosed(true)}
          onFormCancel={() => onDeleteStrategicThemeFormClosed(false)}
        />
      )}
    </>
  )
}

const StrategicThemeDetailsPageWithAuthorization = authorizePage(
  StrategicThemeDetailsPage,
  'Permission',
  'Permissions.StrategicThemes.View',
)

export default StrategicThemeDetailsPageWithAuthorization
