'use client'

import PageTitle from '@/src/components/common/page-title'
import { PageActions } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import {
  AnalyticsViewDataQueryParams,
  AnalyticsViewDataResultDto,
  useDeleteAnalyticsViewMutation,
  useGetAnalyticsViewQuery,
  useLazyGetAnalyticsViewDataQuery,
} from '@/src/store/features/analytics/analytics-views-api'
import { Button, Card, Descriptions, DescriptionsProps, Space, Tag } from 'antd'
import { ItemType } from 'antd/es/menu/interface'
import { notFound, useRouter } from 'next/navigation'
import { use, useCallback, useMemo, useState } from 'react'
import EditAnalyticsViewForm from '../_components/edit-analytics-view-form'
import AnalyticsViewResultsCard from '../_components/analytics-view-results-card'

const AnalyticsViewDetailPage = (props: {
  params: Promise<{ id: string }>
}) => {
  const { id } = use(props.params)
  const router = useRouter()
  const messageApi = useMessage()

  const [openEditForm, setOpenEditForm] = useState(false)
  const [dataResult, setDataResult] =
    useState<AnalyticsViewDataResultDto | null>(null)
  const [runPage, setRunPage] = useState(1)
  const [runPageSize, setRunPageSize] = useState(25)

  const { hasPermissionClaim } = useAuth()
  const canUpdate = hasPermissionClaim('Permissions.AnalyticsViews.Update')
  const canDelete = hasPermissionClaim('Permissions.AnalyticsViews.Delete')
  const canRun = hasPermissionClaim('Permissions.AnalyticsViews.Run')

  const {
    data: analyticsView,
    isLoading,
    isFetching,
    refetch,
  } = useGetAnalyticsViewQuery(id)

  const [deleteAnalyticsView] = useDeleteAnalyticsViewMutation()
  const [fetchData, { isFetching: isRunning }] =
    useLazyGetAnalyticsViewDataQuery()

  useDocumentTitle(analyticsView?.name ?? 'Analytics View')

  const detailsItems = useMemo<DescriptionsProps['items']>(() => {
    if (!analyticsView) return []
    return [
      {
        key: 'dataset',
        label: 'Dataset',
        children: analyticsView.dataset,
      },
      {
        key: 'visibility',
        label: 'Visibility',
        children: (
          <Tag
            color={analyticsView.visibility === 'Public' ? 'blue' : 'default'}
          >
            {analyticsView.visibility}
          </Tag>
        ),
      },
      {
        key: 'isActive',
        label: 'Active',
        children: analyticsView.isActive ? 'Yes' : 'No',
      },
    ]
  }, [analyticsView])

  const actionsMenuItems = useMemo(() => {
    const items: ItemType[] = []
    if (canUpdate) {
      items.push({
        key: 'edit-menu-item',
        label: 'Edit',
        onClick: () => setOpenEditForm(true),
      })
    }
    if (canDelete) {
      items.push({
        key: 'delete-menu-item',
        label: 'Delete',
        danger: true,
        onClick: () => remove(),
      })
    }
    return items
  }, [canUpdate, canDelete])

  const run = useCallback(
    async (pageNumber = runPage, pageSize = runPageSize) => {
      if (!canRun) {
        messageApi.error(
          'You do not have permission to run analytics views.',
        )
        return
      }

      try {
        const params: AnalyticsViewDataQueryParams = {
          id,
          pageNumber,
          pageSize,
        }
        const response = await fetchData(params).unwrap()
        setDataResult(response)
      } catch {
        messageApi.error('Unable to run analytics view.')
      }
    },
    [canRun, id, messageApi, fetchData, runPage, runPageSize],
  )

  const remove = useCallback(async () => {
    if (!canDelete) {
      messageApi.error(
        'You do not have permission to delete analytics views.',
      )
      return
    }
    try {
      const response = await deleteAnalyticsView(id)
      if ('error' in response) throw response.error
      messageApi.success('Analytics view deleted.')
      router.push('/analytics/views')
    } catch {
      messageApi.error('Unable to delete analytics view.')
    }
  }, [canDelete, deleteAnalyticsView, id, messageApi, router])

  const onEditFormClosed = (wasUpdated: boolean) => {
    setOpenEditForm(false)
    if (wasUpdated) {
      refetch()
    }
  }

  if (!isLoading && !isFetching && !analyticsView) {
    notFound()
  }

  return (
    <>
      <PageTitle
        title={analyticsView?.name ?? 'Analytics View'}
        actions={
          <Space>
            {canRun && (
              <Button type="primary" onClick={() => run()} loading={isRunning}>
                Run
              </Button>
            )}
            <PageActions actionItems={actionsMenuItems} />
          </Space>
        }
      />

      <Card loading={isLoading}>
        <Descriptions
          size="small"
          column={{ xs: 1, sm: 2, md: 3, lg: 3 }}
          items={detailsItems}
        />
        {analyticsView?.description && (
          <Descriptions layout="vertical" size="small">
            <Descriptions.Item label="Description">
              {analyticsView.description}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Card>

      <AnalyticsViewResultsCard
        data={dataResult}
        page={runPage}
        pageSize={runPageSize}
        isLoading={isRunning}
        onPrev={() => {
          const nextPage = Math.max(runPage - 1, 1)
          setRunPage(nextPage)
          run(nextPage, runPageSize)
        }}
        onNext={() => {
          const nextPage = runPage + 1
          setRunPage(nextPage)
          run(nextPage, runPageSize)
        }}
        onPageSizeChanged={setRunPageSize}
        onRefresh={() => run(runPage, runPageSize)}
      />

      {openEditForm && analyticsView && (
        <EditAnalyticsViewForm
          showForm={openEditForm}
          analyticsView={analyticsView}
          onFormUpdate={() => onEditFormClosed(true)}
          onFormCancel={() => onEditFormClosed(false)}
        />
      )}
    </>
  )
}

const PageWithAuthorization = authorizePage(
  AnalyticsViewDetailPage,
  'Permission',
  'Permissions.AnalyticsViews.View',
)

export default PageWithAuthorization
