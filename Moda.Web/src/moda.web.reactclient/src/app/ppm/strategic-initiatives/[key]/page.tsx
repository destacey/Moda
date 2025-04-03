'use client'

import { PageActions, PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { Card, MenuProps, message } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import { useCallback, useEffect, useMemo, useState } from 'react'
import StrategicInitiativeDetailsLoading from './loading'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { ItemType } from 'antd/es/menu/interface'
import { useGetStrategicInitiativeQuery } from '@/src/store/features/ppm/strategic-initiatives-api'
import {
  ChangeStrategicInitiativeStatusForm,
  DeleteStrategicInitiativeForm,
  StrategicInitiativeDetails,
} from '../_components'
import EditStrategicInitiativeForm from '../_components/edit-strategic-initiative-form'
import { StrategicInitiativeStatusAction } from '../_components/change-strategic-initiative-status-form'

enum StrategicInitiativeTabs {
  Details = 'details',
}

enum StrategicInitiativeAction {
  Edit = 'Edit',
  Delete = 'Delete',
  Approve = 'Approve',
  Activate = 'Activate',
  Complete = 'Complete',
  Cancel = 'Cancel',
}

const StrategicInitiativeDetailsPage = ({ params }) => {
  useDocumentTitle('Strategic Initiative Details')
  const [activeTab, setActiveTab] = useState(StrategicInitiativeTabs.Details)
  const [openEditStrategicInitiativeForm, setOpenEditStrategicInitiativeForm] =
    useState<boolean>(false)
  const [
    openApproveStrategicInitiativeForm,
    setOpenApproveStrategicInitiativeForm,
  ] = useState<boolean>(false)
  const [
    openActivateStrategicInitiativeForm,
    setOpenActivateStrategicInitiativeForm,
  ] = useState<boolean>(false)
  const [
    openCompleteStrategicInitiativeForm,
    setOpenCompleteStrategicInitiativeForm,
  ] = useState<boolean>(false)
  const [
    openCancelStrategicInitiativeForm,
    setOpenCancelStrategicInitiativeForm,
  ] = useState<boolean>(false)
  const [
    openDeleteStrategicInitiativeForm,
    setOpenDeleteStrategicInitiativeForm,
  ] = useState<boolean>(false)

  const [messageApi, contextHolder] = message.useMessage()

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canUpdateStrategicInitiative = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Update',
  )
  const canDeleteStrategicInitiative = hasPermissionClaim(
    'Permissions.StrategicInitiatives.Delete',
  )

  const {
    data: strategicInitiativeData,
    isLoading,
    error,
    refetch: refetchStrategicInitiative,
  } = useGetStrategicInitiativeQuery(params.key)

  useEffect(() => {
    if (!strategicInitiativeData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'PPM',
      },
      {
        href: `/ppm/strategic-initiatives`,
        title: 'Strategic Initiatives',
      },
    ]

    breadcrumbRoute.push({
      title: 'Details',
    })

    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname, strategicInitiativeData])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const tabs = useMemo(() => {
    const pageTabs = [
      {
        key: StrategicInitiativeTabs.Details,
        label: 'Details',
        content: (
          <StrategicInitiativeDetails
            strategicInitiative={strategicInitiativeData}
          />
        ),
      },
    ]
    return pageTabs
  }, [strategicInitiativeData])

  // doesn't trigger on first render
  const onTabChange = useCallback((tabKey) => {
    setActiveTab(tabKey)
  }, [])

  const missingDates =
    strategicInitiativeData?.start === null ||
    strategicInitiativeData?.end === null

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const currentStatus = strategicInitiativeData?.status.name
    const availableActions =
      currentStatus === 'Proposed'
        ? [
            StrategicInitiativeAction.Edit,
            StrategicInitiativeAction.Delete,
            StrategicInitiativeAction.Approve,
            StrategicInitiativeAction.Cancel,
          ]
        : currentStatus === 'Approved'
          ? [
              StrategicInitiativeAction.Edit,
              StrategicInitiativeAction.Delete,
              StrategicInitiativeAction.Activate,
              StrategicInitiativeAction.Cancel,
            ]
          : currentStatus === 'Active'
            ? [
                StrategicInitiativeAction.Edit,
                StrategicInitiativeAction.Complete,
                StrategicInitiativeAction.Cancel,
              ]
            : []

    // TODO: Implement On Hold status

    const items: ItemType[] = []
    if (
      canUpdateStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Edit)
    ) {
      items.push({
        key: 'edit',
        label: StrategicInitiativeAction.Edit,
        onClick: () => setOpenEditStrategicInitiativeForm(true),
      })
    }
    if (
      canDeleteStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Delete)
    ) {
      items.push({
        key: 'delete',
        label: StrategicInitiativeAction.Delete,
        onClick: () => setOpenDeleteStrategicInitiativeForm(true),
      })
    }

    if (
      canUpdateStrategicInitiative &&
      (availableActions.includes(StrategicInitiativeAction.Approve) ||
        availableActions.includes(StrategicInitiativeAction.Activate) ||
        availableActions.includes(StrategicInitiativeAction.Complete) ||
        availableActions.includes(StrategicInitiativeAction.Cancel))
    ) {
      items.push({
        key: 'manage-divider',
        type: 'divider',
      })
    }

    if (
      canUpdateStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Approve)
    ) {
      items.push({
        key: 'approve',
        label: StrategicInitiativeAction.Approve,
        onClick: () => setOpenApproveStrategicInitiativeForm(true),
      })
    }

    if (
      canUpdateStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Activate)
    ) {
      items.push({
        key: 'activate',
        label: StrategicInitiativeAction.Activate,
        onClick: () => setOpenActivateStrategicInitiativeForm(true),
      })
    }

    if (
      canUpdateStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Complete)
    ) {
      items.push({
        key: 'complete',
        label: StrategicInitiativeAction.Complete,
        onClick: () => setOpenCompleteStrategicInitiativeForm(true),
      })
    }

    if (
      canUpdateStrategicInitiative &&
      availableActions.includes(StrategicInitiativeAction.Cancel)
    ) {
      items.push({
        key: 'cancel',
        label: StrategicInitiativeAction.Cancel,
        onClick: () => setOpenCancelStrategicInitiativeForm(true),
      })
    }

    return items
  }, [
    canDeleteStrategicInitiative,
    canUpdateStrategicInitiative,
    strategicInitiativeData?.status.name,
  ])

  const onEditStrategicInitiativeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditStrategicInitiativeForm(false)
      if (wasSaved) {
        refetchStrategicInitiative()
      }
    },
    [refetchStrategicInitiative],
  )

  const onApproveStrategicInitiativeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenApproveStrategicInitiativeForm(false)
      if (wasSaved) {
        refetchStrategicInitiative()
      }
    },
    [refetchStrategicInitiative],
  )

  const onActivateStrategicInitiativeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenActivateStrategicInitiativeForm(false)
      if (wasSaved) {
        refetchStrategicInitiative()
      }
    },
    [refetchStrategicInitiative],
  )

  const onCompleteStrategicInitiativeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCompleteStrategicInitiativeForm(false)
      if (wasSaved) {
        refetchStrategicInitiative()
      }
    },
    [refetchStrategicInitiative],
  )

  const onCancelStrategicInitiativeFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCancelStrategicInitiativeForm(false)
      if (wasSaved) {
        refetchStrategicInitiative()
      }
    },
    [refetchStrategicInitiative],
  )

  const onDeleteStrategicInitiativeFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeleteStrategicInitiativeForm(false)
      if (wasDeleted) {
        router.push('/ppm/strategic-initiatives')
      }
    },
    [router],
  )

  if (isLoading) {
    return <StrategicInitiativeDetailsLoading />
  }

  if (!strategicInitiativeData) {
    notFound()
  }

  return (
    <>
      {contextHolder}
      <PageTitle
        title={`${strategicInitiativeData?.key} - ${strategicInitiativeData?.name}`}
        subtitle="StrategicInitiative Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Card
        style={{ width: '100%' }}
        tabList={tabs}
        activeTabKey={activeTab}
        onTabChange={onTabChange}
      >
        {tabs.find((t) => t.key === activeTab)?.content}
      </Card>

      {openEditStrategicInitiativeForm && (
        <EditStrategicInitiativeForm
          strategicInitiativeKey={strategicInitiativeData?.key}
          showForm={openEditStrategicInitiativeForm}
          onFormComplete={() => onEditStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onEditStrategicInitiativeFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openApproveStrategicInitiativeForm && (
        <ChangeStrategicInitiativeStatusForm
          strategicInitiative={strategicInitiativeData}
          statusAction={StrategicInitiativeStatusAction.Approve}
          showForm={openApproveStrategicInitiativeForm}
          onFormComplete={() => onApproveStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onApproveStrategicInitiativeFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openActivateStrategicInitiativeForm && (
        <ChangeStrategicInitiativeStatusForm
          strategicInitiative={strategicInitiativeData}
          statusAction={StrategicInitiativeStatusAction.Activate}
          showForm={openActivateStrategicInitiativeForm}
          onFormComplete={() => onActivateStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onActivateStrategicInitiativeFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openCompleteStrategicInitiativeForm && (
        <ChangeStrategicInitiativeStatusForm
          strategicInitiative={strategicInitiativeData}
          statusAction={StrategicInitiativeStatusAction.Complete}
          showForm={openCompleteStrategicInitiativeForm}
          onFormComplete={() => onCompleteStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onCompleteStrategicInitiativeFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openCancelStrategicInitiativeForm && (
        <ChangeStrategicInitiativeStatusForm
          strategicInitiative={strategicInitiativeData}
          statusAction={StrategicInitiativeStatusAction.Cancel}
          showForm={openCancelStrategicInitiativeForm}
          onFormComplete={() => onCancelStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onCancelStrategicInitiativeFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openDeleteStrategicInitiativeForm && (
        <DeleteStrategicInitiativeForm
          strategicInitiative={strategicInitiativeData}
          showForm={openDeleteStrategicInitiativeForm}
          onFormComplete={() => onDeleteStrategicInitiativeFormClosed(true)}
          onFormCancel={() => onDeleteStrategicInitiativeFormClosed(false)}
          messageApi={messageApi}
        />
      )}
    </>
  )
}

const StrategicInitiativeDetailsPageWithAuthorization = authorizePage(
  StrategicInitiativeDetailsPage,
  'Permission',
  'Permissions.StrategicInitiatives.View',
)

export default StrategicInitiativeDetailsPageWithAuthorization
