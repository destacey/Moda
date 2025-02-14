'use client'

import { PageActions, PageTitle } from '@/src/components/common'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import { authorizePage } from '@/src/components/hoc'
import { useAppDispatch, useDocumentTitle } from '@/src/hooks'
import { Descriptions, MenuProps, message, Space } from 'antd'
import { notFound, usePathname, useRouter } from 'next/navigation'
import PortfolioDetailsLoading from './loading'
import { useCallback, useEffect, useMemo, useState } from 'react'
import { BreadcrumbItem, setBreadcrumbRoute } from '@/src/store/breadcrumbs'
import { ItemType } from 'antd/es/menu/interface'
import {
  ChangePortfolioStateForm,
  DeletePortfolioForm,
  EditPortfolioForm,
} from '../_components'
import { useGetPortfolioQuery } from '@/src/store/features/ppm/portfolios-api'
import { PortfolioStateAction } from '../_components/change-portfolio-state-form'

const { Item } = Descriptions

enum MenuActions {
  Edit = 'Edit',
  Delete = 'Delete',
  Activate = 'Activate',
  Complete = 'Complete',
  Archive = 'Archive',
}

const PortfolioDetailsPage = ({ params }) => {
  useDocumentTitle('Portfolio Details')
  const [openEditPortfolioForm, setOpenEditPortfolioForm] =
    useState<boolean>(false)
  const [openActivatePortfolioForm, setOpenActivatePortfolioForm] =
    useState<boolean>(false)
  const [openCompletePortfolioForm, setOpenCompletePortfolioForm] =
    useState<boolean>(false)
  const [openArchivePortfolioForm, setOpenArchivePortfolioForm] =
    useState<boolean>(false)
  const [openDeletePortfolioForm, setOpenDeletePortfolioForm] =
    useState<boolean>(false)

  const [messageApi, contextHolder] = message.useMessage()

  const pathname = usePathname()
  const dispatch = useAppDispatch()

  const router = useRouter()

  const { hasPermissionClaim } = useAuth()
  const canUpdatePortfolio = hasPermissionClaim(
    'Permissions.ProjectPortfolios.Update',
  )
  const canDeletePortfolio = hasPermissionClaim(
    'Permissions.ProjectPortfolios.Delete',
  )

  const {
    data: portfolioData,
    isLoading,
    error,
    refetch: refetchPortfolio,
  } = useGetPortfolioQuery(params.key)

  useEffect(() => {
    if (!portfolioData) return

    const breadcrumbRoute: BreadcrumbItem[] = [
      {
        title: 'PPM',
      },
      {
        href: `/ppm/portfolios`,
        title: 'Portfolios',
      },
    ]

    breadcrumbRoute.push({
      title: 'Details',
    })

    dispatch(setBreadcrumbRoute({ route: breadcrumbRoute, pathname }))
  }, [dispatch, pathname, portfolioData])

  useEffect(() => {
    error && console.error(error)
  }, [error])

  const actionsMenuItems: MenuProps['items'] = useMemo(() => {
    const currentStatus = portfolioData?.status.name
    const availableActions =
      currentStatus === 'Proposed'
        ? [MenuActions.Delete, MenuActions.Activate]
        : currentStatus === 'Active'
          ? [MenuActions.Complete]
          : currentStatus === 'Completed'
            ? [MenuActions.Archive]
            : []

    // TODO: Implement On Hold status

    const items: ItemType[] = []
    if (canUpdatePortfolio) {
      items.push({
        key: 'edit',
        label: MenuActions.Edit,
        onClick: () => setOpenEditPortfolioForm(true),
      })
    }
    if (canDeletePortfolio && availableActions.includes(MenuActions.Delete)) {
      items.push({
        key: 'delete',
        label: MenuActions.Delete,
        onClick: () => setOpenDeletePortfolioForm(true),
      })
    }

    if (
      (canUpdatePortfolio && availableActions.includes(MenuActions.Activate)) ||
      availableActions.includes(MenuActions.Archive)
    ) {
      items.push({
        key: 'manage-divider',
        type: 'divider',
      })
    }

    if (canUpdatePortfolio && availableActions.includes(MenuActions.Activate)) {
      items.push({
        key: 'activate',
        label: MenuActions.Activate,
        onClick: () => setOpenActivatePortfolioForm(true),
      })
    }

    if (canUpdatePortfolio && availableActions.includes(MenuActions.Complete)) {
      items.push({
        key: 'complete',
        label: MenuActions.Complete,
        onClick: () => setOpenCompletePortfolioForm(true),
      })
    }

    if (canUpdatePortfolio && availableActions.includes(MenuActions.Archive)) {
      items.push({
        key: 'archive',
        label: MenuActions.Archive,
        onClick: () => setOpenArchivePortfolioForm(true),
      })
    }

    return items
  }, [canDeletePortfolio, canUpdatePortfolio, portfolioData?.status.name])

  const onEditPortfolioFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenEditPortfolioForm(false)
      if (wasSaved) {
        refetchPortfolio()
      }
    },
    [refetchPortfolio],
  )

  const onActivatePortfolioFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenActivatePortfolioForm(false)
      if (wasSaved) {
        refetchPortfolio()
      }
    },
    [refetchPortfolio],
  )

  const onCompletePortfolioFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenCompletePortfolioForm(false)
      if (wasSaved) {
        refetchPortfolio()
      }
    },
    [refetchPortfolio],
  )

  const onArchivePortfolioFormClosed = useCallback(
    (wasSaved: boolean) => {
      setOpenArchivePortfolioForm(false)
      if (wasSaved) {
        refetchPortfolio()
      }
    },
    [refetchPortfolio],
  )

  const onDeletePortfolioFormClosed = useCallback(
    (wasDeleted: boolean) => {
      setOpenDeletePortfolioForm(false)
      if (wasDeleted) {
        router.push('/ppm/portfolios')
      }
    },
    [router],
  )

  if (isLoading) {
    return <PortfolioDetailsLoading />
  }

  if (!portfolioData) {
    notFound()
  }

  return (
    <>
      {contextHolder}
      <PageTitle
        title={`${portfolioData?.key} - ${portfolioData?.name}`}
        subtitle="Portfolio Details"
        actions={<PageActions actionItems={actionsMenuItems} />}
      />
      <Space direction="vertical" size="small">
        <Descriptions>
          <Item label="State">{portfolioData.status.name}</Item>
        </Descriptions>
        <Descriptions layout="vertical" size="small">
          <Item label="Description">
            <MarkdownRenderer markdown={portfolioData.description} />
          </Item>
        </Descriptions>
      </Space>

      {openEditPortfolioForm && (
        <EditPortfolioForm
          portfolioKey={portfolioData?.key}
          showForm={openEditPortfolioForm}
          onFormComplete={() => onEditPortfolioFormClosed(true)}
          onFormCancel={() => onEditPortfolioFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openActivatePortfolioForm && (
        <ChangePortfolioStateForm
          portfolio={portfolioData}
          stateAction={PortfolioStateAction.Activate}
          showForm={openActivatePortfolioForm}
          onFormComplete={() => onActivatePortfolioFormClosed(true)}
          onFormCancel={() => onActivatePortfolioFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openCompletePortfolioForm && (
        <ChangePortfolioStateForm
          portfolio={portfolioData}
          stateAction={PortfolioStateAction.Complete}
          showForm={openCompletePortfolioForm}
          onFormComplete={() => onCompletePortfolioFormClosed(true)}
          onFormCancel={() => onCompletePortfolioFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openArchivePortfolioForm && (
        <ChangePortfolioStateForm
          portfolio={portfolioData}
          stateAction={PortfolioStateAction.Archive}
          showForm={openArchivePortfolioForm}
          onFormComplete={() => onArchivePortfolioFormClosed(true)}
          onFormCancel={() => onArchivePortfolioFormClosed(false)}
          messageApi={messageApi}
        />
      )}
      {openDeletePortfolioForm && (
        <DeletePortfolioForm
          portfolio={portfolioData}
          showForm={openDeletePortfolioForm}
          onFormComplete={() => onDeletePortfolioFormClosed(true)}
          onFormCancel={() => onDeletePortfolioFormClosed(false)}
          messageApi={messageApi}
        />
      )}
    </>
  )
}

const PortfolioDetailsPageWithAuthorization = authorizePage(
  PortfolioDetailsPage,
  'Permission',
  'Permissions.ProjectPortfolios.View',
)

export default PortfolioDetailsPageWithAuthorization
