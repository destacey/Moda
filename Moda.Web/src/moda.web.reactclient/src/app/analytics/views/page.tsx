'use client'

import { PageTitle } from '@/src/components/common'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import {
  AnalyticsViewResultDto,
  CreateAnalyticsViewRequest,
  RunAnalyticsViewRequest,
  UpdateAnalyticsViewRequest,
} from '@/src/services/moda-api'
import {
  useCreateAnalyticsViewMutation,
  useDeleteAnalyticsViewMutation,
  useGetAnalyticsViewQuery,
  useGetAnalyticsViewsQuery,
  useRunAnalyticsViewMutation,
  useUpdateAnalyticsViewMutation,
} from '@/src/store/features/analytics/analytics-views-api'
import { toFormErrors } from '@/src/utils'
import { Col, Form, Row } from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  fieldOptions,
  buildDefinitionJson,
  makeDefaultFormValues,
  parseDefinition,
} from './builder'
import AnalyticsViewEditorCard from './_components/analytics-view-editor-card'
import AnalyticsViewResultsCard from './_components/analytics-view-results-card'
import AnalyticsViewsListCard from './_components/analytics-views-list-card'
import { FormValues } from './types'

const AnalyticsViewsPage = () => {
  useDocumentTitle('Analytics Views')
  const messageApi = useMessage()
  const [form] = Form.useForm<FormValues>()
  const watchedForm = Form.useWatch([], form)

  const [selectedId, setSelectedId] = useState<string | null>(null)
  const [result, setResult] = useState<AnalyticsViewResultDto | null>(null)
  const [runPage, setRunPage] = useState(1)
  const [runPageSize, setRunPageSize] = useState(25)
  const [isSaving, setIsSaving] = useState(false)

  const includeInactive = Form.useWatch('includeInactive', form) ?? false

  const { hasPermissionClaim } = useAuth()
  const canCreate = hasPermissionClaim('Permissions.AnalyticsViews.Create')
  const canUpdate = hasPermissionClaim('Permissions.AnalyticsViews.Update')
  const canDelete = hasPermissionClaim('Permissions.AnalyticsViews.Delete')
  const canRun = hasPermissionClaim('Permissions.AnalyticsViews.Run')

  const { data: viewsData, isLoading: isViewsLoading, refetch } =
    useGetAnalyticsViewsQuery(includeInactive)

  const { data: selectedView, isFetching: isDetailLoading } =
    useGetAnalyticsViewQuery(selectedId ?? '', {
      skip: !selectedId,
    })

  const [createAnalyticsView] = useCreateAnalyticsViewMutation()
  const [updateAnalyticsView] = useUpdateAnalyticsViewMutation()
  const [deleteAnalyticsView] = useDeleteAnalyticsViewMutation()
  const [runAnalyticsView, { isLoading: isRunning }] = useRunAnalyticsViewMutation()

  useEffect(() => {
    if (!selectedView) return

    const definition = parseDefinition(
      selectedView.definitionJson,
      selectedView.dataset,
    )
    form.setFieldsValue({
      id: selectedView.id,
      name: selectedView.name,
      description: selectedView.description,
      dataset: selectedView.dataset,
      visibility: selectedView.visibility,
      isActive: selectedView.isActive,
      includeInactive,
      definition,
    })
    setResult(null)
  }, [form, includeInactive, selectedView])

  const selectedMeasureOptions = useMemo(() => {
    const definition = watchedForm?.definition
    const measureNames =
      definition?.measures?.map((m) => ({
        label: m.alias || `${m.type}_${m.field ?? 'id'}`,
        value: m.alias || `${m.type}_${m.field ?? 'id'}`,
      })) ?? []
    return measureNames
  }, [watchedForm])

  const sortFieldOptions = useMemo(() => {
    const definition = watchedForm?.definition
    const fields = new Set<string>()
    ;(definition?.columns ?? []).forEach((c) => {
      if (c.field) fields.add(c.field)
      if (c.alias) fields.add(c.alias)
    })
    ;(definition?.groupBy ?? []).forEach((g) => {
      if (g) fields.add(g)
    })
    selectedMeasureOptions.forEach((m) => fields.add(m.value))

    if (fields.size === 0) {
      fieldOptions.forEach((f) => fields.add(f.value))
    }

    return [...fields].map((f) => ({ label: f, value: f }))
  }, [selectedMeasureOptions, watchedForm])

  const resetForCreate = useCallback(() => {
    setSelectedId(null)
    setResult(null)
    form.setFieldsValue(makeDefaultFormValues())
  }, [form])

  const save = async (values: FormValues) => {
    const definitionJson = buildDefinitionJson(values.definition, values.dataset)

    try {
      setIsSaving(true)
      if (values.id) {
        if (!canUpdate) {
          messageApi.error('You do not have permission to update analytics views.')
          return
        }
        const request: UpdateAnalyticsViewRequest = {
          id: values.id,
          name: values.name,
          description: values.description,
          dataset: values.dataset,
          visibility: values.visibility,
          definitionJson,
          isActive: values.isActive,
        }
        const response = await updateAnalyticsView(request)
        if ('error' in response) throw response.error
        messageApi.success('Analytics view updated.')
      } else {
        if (!canCreate) {
          messageApi.error('You do not have permission to create analytics views.')
          return
        }
        const request: CreateAnalyticsViewRequest = {
          name: values.name,
          description: values.description,
          dataset: values.dataset,
          visibility: values.visibility,
          definitionJson,
          isActive: values.isActive,
        }
        const response = await createAnalyticsView(request)
        if ('error' in response) throw response.error
        setSelectedId(response.data)
        messageApi.success('Analytics view created.')
      }
      refetch()
    } catch (error: any) {
      if (error?.status === 422 && error.errors) {
        form.setFields(toFormErrors(error.errors))
        messageApi.error('Correct validation errors to continue.')
      } else {
        messageApi.error('Unable to save analytics view.')
      }
    } finally {
      setIsSaving(false)
    }
  }

  const run = useCallback(
    async (pageNumber = runPage, pageSize = runPageSize) => {
      const id = form.getFieldValue('id') as string | undefined
      if (!id) {
        messageApi.info('Save the analytics view before running it.')
        return
      }
      if (!canRun) {
        messageApi.error('You do not have permission to run analytics views.')
        return
      }

      try {
        const request: RunAnalyticsViewRequest = {
          id,
          pageNumber,
          pageSize,
        }
        const response = await runAnalyticsView(request)
        if ('error' in response) throw response.error
        setResult(response.data)
      } catch {
        messageApi.error('Unable to run analytics view.')
      }
    },
    [canRun, form, messageApi, runAnalyticsView, runPage, runPageSize],
  )

  const remove = useCallback(async () => {
    const id = form.getFieldValue('id') as string | undefined
    if (!id) return
    if (!canDelete) {
      messageApi.error('You do not have permission to delete analytics views.')
      return
    }
    try {
      const response = await deleteAnalyticsView(id)
      if ('error' in response) throw response.error
      messageApi.success('Analytics view deleted.')
      resetForCreate()
      refetch()
    } catch {
      messageApi.error('Unable to delete analytics view.')
    }
  }, [canDelete, deleteAnalyticsView, form, messageApi, refetch, resetForCreate])

  useEffect(() => {
    form.setFieldsValue({ includeInactive: false, ...makeDefaultFormValues() })
  }, [form])

  return (
    <>
      <PageTitle title="Analytics Views" />

      <Row gutter={16} align="top">
        <Col xs={24} xl={8}>
          <AnalyticsViewsListCard
            form={form}
            canCreate={canCreate}
            isViewsLoading={isViewsLoading}
            viewsData={viewsData}
            onResetForCreate={resetForCreate}
            onSelectView={setSelectedId}
            onIncludeInactiveChanged={() => {
              setSelectedId(null)
              setResult(null)
            }}
          />
        </Col>

        <Col xs={24} xl={16}>
          <AnalyticsViewEditorCard
            form={form}
            selectedId={selectedId}
            isDetailLoading={isDetailLoading}
            isSaving={isSaving}
            canCreate={canCreate}
            canUpdate={canUpdate}
            canDelete={canDelete}
            canRun={canRun}
            sortFieldOptions={sortFieldOptions}
            watchedDefinition={watchedForm?.definition}
            watchedDataset={watchedForm?.dataset}
            onRun={() => run()}
            onDelete={remove}
            onSubmit={() => form.submit()}
            onFinish={save}
          />
        </Col>
      </Row>

      <AnalyticsViewResultsCard
        result={result}
        runPage={runPage}
        runPageSize={runPageSize}
        isRunning={isRunning}
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
    </>
  )
}

const PageWithAuthorization = authorizePage(
  AnalyticsViewsPage,
  'Permission',
  'Permissions.AnalyticsViews.View',
)

export default PageWithAuthorization
