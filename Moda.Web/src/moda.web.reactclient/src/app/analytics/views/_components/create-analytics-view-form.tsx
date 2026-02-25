'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  AnalyticsDataset,
  CreateAnalyticsViewRequest,
  Visibility,
} from '@/src/services/moda-api'
import { useCreateAnalyticsViewMutation } from '@/src/store/features/analytics/analytics-views-api'
import { useGetInternalEmployeeIdQuery } from '@/src/store/features/user-management/profile-api'
import { toFormErrors } from '@/src/utils'
import { Col, Form, Input, Modal, Radio, Row, Select } from 'antd'
import { useEffect, useState } from 'react'
import { EmployeeSelect } from '@/src/components/common/organizations'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'
import { buildDefinitionJson, makeDefaultDefinition } from '../builder'

const { TextArea } = Input

interface CreateAnalyticsViewFormValues {
  name: string
  description?: string
  dataset: AnalyticsDataset
  visibility: Visibility
  managerIds: string[]
}

export interface CreateAnalyticsViewFormProps {
  showForm: boolean
  onFormCreate: (id: string) => void
  onFormCancel: () => void
}

const CreateAnalyticsViewForm = ({
  showForm,
  onFormCreate,
  onFormCancel,
}: CreateAnalyticsViewFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateAnalyticsViewFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const { hasPermissionClaim } = useAuth()
  const canCreate = hasPermissionClaim('Permissions.AnalyticsViews.Create')

  const { data: currentUserEmployeeId } = useGetInternalEmployeeIdQuery()
  const { data: employeeOptions } = useGetEmployeeOptionsQuery(false)
  const [createAnalyticsView] = useCreateAnalyticsViewMutation()

  const create = async (
    values: CreateAnalyticsViewFormValues,
  ): Promise<string | null> => {
    const defaultDefinition = makeDefaultDefinition()
    const definitionJson = buildDefinitionJson(
      defaultDefinition,
      values.dataset,
    )

    try {
      const request: CreateAnalyticsViewRequest = {
        name: values.name,
        description: values.description,
        dataset: values.dataset,
        visibility: values.visibility,
        managerIds: values.managerIds,
        definitionJson,
        isActive: true,
      }
      const response = await createAnalyticsView(request)
      if ('error' in response) throw response.error
      return response.data
    } catch (error: any) {
      if (error?.status === 422 && error.errors) {
        form.setFields(toFormErrors(error.errors))
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error('An error occurred creating the analytics view.')
      }
      return null
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      const id = await create(values)
      if (id) {
        setIsOpen(false)
        form.resetFields()
        messageApi.success('Analytics view created.')
        onFormCreate(id)
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
    if (canCreate) {
      setIsOpen(showForm)
    } else {
      onFormCancel()
      messageApi.error(
        'You do not have permission to create analytics views.',
      )
    }
  }, [canCreate, messageApi, onFormCancel, showForm])

  useEffect(() => {
    if (currentUserEmployeeId) {
      form.setFieldValue('managerIds', [currentUserEmployeeId])
    }
  }, [currentUserEmployeeId, form])

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
      title="Create Analytics View"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Create"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false}
      destroyOnHidden={true}
      width={600}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="create-analytics-view-form"
      >
        <Form.Item
          label="Name"
          name="name"
          rules={[{ required: true, message: 'Name is required.' }]}
        >
          <Input maxLength={128} placeholder="Enter name" />
        </Form.Item>

        <Form.Item label="Description" name="description">
          <TextArea
            maxLength={2048}
            rows={3}
            placeholder="Enter brief description"
          />
        </Form.Item>

        <Row gutter={12}>
          <Col span={12}>
            <Form.Item
              label="Dataset"
              name="dataset"
              initialValue={AnalyticsDataset.WorkItems}
              rules={[{ required: true, message: 'Dataset is required.' }]}
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
          <Col span={12}>
            <Form.Item
              label="Visibility"
              name="visibility"
              initialValue={Visibility.Private}
            >
              <Radio.Group>
                <Radio value={Visibility.Private}>Private</Radio>
                <Radio value={Visibility.Public}>Public</Radio>
              </Radio.Group>
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
          ]}
        >
          <EmployeeSelect
            employees={employeeOptions ?? []}
            allowMultiple={true}
            placeholder="Select one or more managers"
          />
        </Form.Item>
      </Form>
    </Modal>
  )
}

export default CreateAnalyticsViewForm
