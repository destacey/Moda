'use client'

import { EmployeeSelect } from '@/src/components/common/organizations'
import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  AnalyticsDataset,
  AnalyticsViewDetailsDto,
  UpdateAnalyticsViewRequest,
  Visibility,
} from '@/src/services/moda-api'
import { useUpdateAnalyticsViewMutation } from '@/src/store/features/analytics/analytics-views-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { useGetInternalEmployeeIdQuery } from '@/src/store/features/user-management/profile-api'
import { toFormErrors } from '@/src/utils'
import {
  Button,
  Col,
  Divider,
  Form,
  Input,
  InputNumber,
  Modal,
  Row,
  Select,
  Space,
} from 'antd'
import { useCallback, useEffect, useMemo, useState } from 'react'
import {
  buildDefinitionJson,
  fieldOptions,
  filterOperatorOptions,
  groupByOptions,
  makeDefaultDefinition,
  measureTypeOptions,
  numericFieldOptions,
  parseDefinition,
} from '../builder'
import { FormValues } from '../types'

const { TextArea } = Input

export interface EditAnalyticsViewFormProps {
  showForm: boolean
  analyticsView: AnalyticsViewDetailsDto
  onFormUpdate: () => void
  onFormCancel: () => void
}

const EditAnalyticsViewForm = ({
  showForm,
  analyticsView,
  onFormUpdate,
  onFormCancel,
}: EditAnalyticsViewFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<FormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const { hasPermissionClaim } = useAuth()
  const canUpdate = hasPermissionClaim('Permissions.AnalyticsViews.Update')

  const { data: currentUserEmployeeId } = useGetInternalEmployeeIdQuery()
  const { data: employeeOptions } = useGetEmployeeOptionsQuery(false)
  const [updateAnalyticsView] = useUpdateAnalyticsViewMutation()

  const selectedMeasureOptions = useMemo(() => {
    const definition = formValues?.definition
    return (
      definition?.measures?.map((m) => ({
        label: m.alias || `${m.type}_${m.field ?? 'id'}`,
        value: m.alias || `${m.type}_${m.field ?? 'id'}`,
      })) ?? []
    )
  }, [formValues])

  const sortFieldOptions = useMemo(() => {
    const definition = formValues?.definition
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
  }, [selectedMeasureOptions, formValues])

  const mapToFormValues = useCallback(
    (view: AnalyticsViewDetailsDto) => {
      const definition = parseDefinition(view.definitionJson, view.dataset)
      form.setFieldsValue({
        id: view.id,
        name: view.name,
        description: view.description,
        dataset: view.dataset,
        visibility: view.visibility,
        managerIds: view.managerIds,
        isActive: view.isActive,
        includeInactive: false,
        definition,
      })
    },
    [form],
  )

  const update = async (values: FormValues): Promise<boolean> => {
    const definitionJson = buildDefinitionJson(
      values.definition,
      values.dataset,
    )

    try {
      const request: UpdateAnalyticsViewRequest = {
        id: analyticsView.id,
        name: values.name,
        description: values.description,
        dataset: values.dataset,
        visibility: values.visibility,
        managerIds: values.managerIds,
        definitionJson,
        isActive: values.isActive,
      }
      const response = await updateAnalyticsView(request)
      if ('error' in response) throw response.error
      return true
    } catch (error: any) {
      if (error?.status === 422 && error.errors) {
        form.setFields(toFormErrors(error.errors))
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error('An error occurred updating the analytics view.')
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await update(values)) {
        setIsOpen(false)
        form.resetFields()
        messageApi.success('Analytics view updated.')
        onFormUpdate()
      }
    } catch {
      messageApi.error('Correct the validation error(s) to continue.')
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = () => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }

  useEffect(() => {
    if (canUpdate) {
      setIsOpen(showForm)
      if (showForm && analyticsView) {
        mapToFormValues(analyticsView)
      }
    } else {
      onFormCancel()
      messageApi.error(
        'You do not have permission to update analytics views.',
      )
    }
  }, [
    canUpdate,
    showForm,
    analyticsView,
    mapToFormValues,
    messageApi,
    onFormCancel,
  ])

  useEffect(() => {
    form
      .validateFields({ validateOnly: true })
      .then(
        () => setIsValid(true && form.isFieldsTouched()),
        () => setIsValid(false),
      )
  }, [form, formValues])

  return (
    <Modal
      title="Edit Analytics View"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden={true}
      width={900}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="edit-analytics-view-form"
      >
        <Form.Item name="id" hidden>
          <Input />
        </Form.Item>

        <Row gutter={12}>
          <Col span={12}>
            <Form.Item
              label="Name"
              name="name"
              rules={[{ required: true, message: 'Name is required.' }]}
            >
              <Input maxLength={128} />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item label="Description" name="description">
              <Input maxLength={2048} />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={12}>
          <Col span={8}>
            <Form.Item
              label="Dataset"
              name="dataset"
              initialValue={AnalyticsDataset.WorkItems}
            >
              <Select
                options={[
                  { label: 'Work Items', value: AnalyticsDataset.WorkItems },
                  {
                    label: 'Dependencies',
                    value: AnalyticsDataset.Dependencies,
                    disabled: true,
                  },
                  {
                    label: 'PI Objectives',
                    value: AnalyticsDataset.PlanningIntervalObjectives,
                    disabled: true,
                  },
                ]}
              />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item
              label="Visibility"
              name="visibility"
              initialValue={Visibility.Private}
            >
              <Select
                options={[
                  { label: 'Private', value: Visibility.Private },
                  { label: 'Public', value: Visibility.Public },
                ]}
              />
            </Form.Item>
          </Col>
          <Col span={8}>
            <Form.Item label="Is Active" name="isActive" initialValue={true}>
              <Select
                options={[
                  { label: 'Active', value: true },
                  { label: 'Inactive', value: false },
                ]}
              />
            </Form.Item>
          </Col>
        </Row>

        <Form.Item
          label="Managers"
          name="managerIds"
          rules={[
            {
              required: true,
              message: 'Select at least one manager.',
            },
            {
              validator: async (_, value) => {
                if (
                  currentUserEmployeeId &&
                  (!value || !value.includes(currentUserEmployeeId))
                ) {
                  return Promise.reject(
                    new Error(
                      'You must be a manager of the analytics view.',
                    ),
                  )
                }
                return Promise.resolve()
              },
            },
          ]}
        >
          <EmployeeSelect
            employees={employeeOptions ?? []}
            allowMultiple={true}
            placeholder="Select one or more managers"
          />
        </Form.Item>

        <Divider>Columns</Divider>
        <Form.List name={['definition', 'columns']}>
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Space
                  key={field.key}
                  align="baseline"
                  style={{ display: 'flex', marginBottom: 8 }}
                >
                  <Form.Item
                    name={[field.name, 'field']}
                    rules={[{ required: true, message: 'Field required.' }]}
                  >
                    <Select
                      style={{ width: 240 }}
                      options={groupByOptions}
                      placeholder="Field"
                    />
                  </Form.Item>
                  <Form.Item name={[field.name, 'alias']}>
                    <Input
                      style={{ width: 200 }}
                      placeholder="Alias (optional)"
                    />
                  </Form.Item>
                  <Button danger onClick={() => remove(field.name)}>
                    Remove
                  </Button>
                </Space>
              ))}
              <Button onClick={() => add({ field: 'key', alias: '' })}>
                Add Column
              </Button>
            </>
          )}
        </Form.List>

        <Divider>Filters</Divider>
        <Form.List name={['definition', 'filters']}>
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Space
                  key={field.key}
                  align="baseline"
                  style={{ display: 'flex', marginBottom: 8 }}
                >
                  <Form.Item
                    name={[field.name, 'field']}
                    rules={[{ required: true, message: 'Field required.' }]}
                  >
                    <Select
                      style={{ width: 220 }}
                      options={groupByOptions}
                      placeholder="Field"
                    />
                  </Form.Item>
                  <Form.Item
                    name={[field.name, 'operator']}
                    rules={[
                      { required: true, message: 'Operator required.' },
                    ]}
                  >
                    <Select
                      style={{ width: 200 }}
                      options={filterOperatorOptions}
                      placeholder="Operator"
                    />
                  </Form.Item>
                  <Form.Item name={[field.name, 'valuesCsv']}>
                    <Input
                      style={{ width: 280 }}
                      placeholder="Values (comma separated)"
                    />
                  </Form.Item>
                  <Button danger onClick={() => remove(field.name)}>
                    Remove
                  </Button>
                </Space>
              ))}
              <Button
                onClick={() =>
                  add({
                    field: 'statusCategory',
                    operator: 'Equals',
                    valuesCsv: '',
                  })
                }
              >
                Add Filter
              </Button>
            </>
          )}
        </Form.List>

        <Divider>Grouping & Measures</Divider>
        <Form.Item label="Group By" name={['definition', 'groupBy']}>
          <Select
            mode="multiple"
            options={groupByOptions}
            placeholder="Select grouping fields"
          />
        </Form.Item>

        <Form.List name={['definition', 'measures']}>
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Space
                  key={field.key}
                  align="baseline"
                  style={{ display: 'flex', marginBottom: 8 }}
                >
                  <Form.Item
                    name={[field.name, 'type']}
                    rules={[{ required: true, message: 'Type required.' }]}
                  >
                    <Select
                      style={{ width: 160 }}
                      options={measureTypeOptions}
                      placeholder="Type"
                    />
                  </Form.Item>
                  <Form.Item name={[field.name, 'field']}>
                    <Select
                      style={{ width: 200 }}
                      options={numericFieldOptions}
                      placeholder="Field"
                      allowClear
                    />
                  </Form.Item>
                  <Form.Item name={[field.name, 'percentile']}>
                    <InputNumber
                      min={0}
                      max={100}
                      style={{ width: 120 }}
                      placeholder="Percentile"
                    />
                  </Form.Item>
                  <Form.Item name={[field.name, 'alias']}>
                    <Input style={{ width: 180 }} placeholder="Alias" />
                  </Form.Item>
                  <Button danger onClick={() => remove(field.name)}>
                    Remove
                  </Button>
                </Space>
              ))}
              <Button
                onClick={() =>
                  add({ type: 'Count', field: 'id', alias: 'Count' })
                }
              >
                Add Measure
              </Button>
            </>
          )}
        </Form.List>

        <Divider>Sort</Divider>
        <Form.List name={['definition', 'sort']}>
          {(fields, { add, remove }) => (
            <>
              {fields.map((field) => (
                <Space
                  key={field.key}
                  align="baseline"
                  style={{ display: 'flex', marginBottom: 8 }}
                >
                  <Form.Item
                    name={[field.name, 'field']}
                    rules={[
                      { required: true, message: 'Sort field required.' },
                    ]}
                  >
                    <Select
                      style={{ width: 260 }}
                      options={sortFieldOptions}
                      placeholder="Sort Field"
                    />
                  </Form.Item>
                  <Form.Item
                    name={[field.name, 'direction']}
                    rules={[
                      { required: true, message: 'Direction required.' },
                    ]}
                  >
                    <Select
                      style={{ width: 140 }}
                      options={[
                        { label: 'Ascending', value: 'Asc' },
                        { label: 'Descending', value: 'Desc' },
                      ]}
                    />
                  </Form.Item>
                  <Button danger onClick={() => remove(field.name)}>
                    Remove
                  </Button>
                </Space>
              ))}
              <Button onClick={() => add({ field: 'key', direction: 'Asc' })}>
                Add Sort
              </Button>
            </>
          )}
        </Form.List>

        <Divider>Definition Preview</Divider>
        <TextArea
          value={buildDefinitionJson(
            formValues?.definition ?? makeDefaultDefinition(),
            formValues?.dataset ?? AnalyticsDataset.WorkItems,
          )}
          autoSize={{ minRows: 6, maxRows: 12 }}
          readOnly
        />
      </Form>
    </Modal>
  )
}

export default EditAnalyticsViewForm
