'use client'

import {
  DatePicker,
  Descriptions,
  Flex,
  Form,
  Input,
  Modal,
  Radio,
  Select,
} from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../contexts/auth'
import { RiskDetailsDto, UpdateRiskRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import { MarkdownEditor } from '../markdown'
import { useMessage } from '../../contexts/messaging'
import {
  useGetRiskCategoryOptionsQuery,
  useGetRiskGradeOptionsQuery,
  useGetRiskQuery,
  useGetRiskStatusOptionsQuery,
  useUpdateRiskMutation,
} from '@/src/store/features/planning/risks-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'

const { Item } = Descriptions
const { Item: FormItem } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio

export interface EditRiskFormProps {
  showForm: boolean
  riskKey: number
  onFormSave: () => void
  onFormCancel: () => void
}

interface EditRiskFormValues {
  riskId: string
  teamId: string
  summary: string
  description?: string | undefined
  statusId: number
  categoryId: number
  impactId: number
  likelihoodId: number
  assigneeId?: string | undefined
  followUpDate?: Date | undefined
  response?: string | undefined
}

const mapToRequestValues = (values: EditRiskFormValues) => {
  return {
    riskId: values.riskId,
    teamId: values.teamId,
    summary: values.summary,
    description: values.description,
    statusId: values.statusId,
    categoryId: values.categoryId,
    impactId: values.impactId,
    likelihoodId: values.likelihoodId,
    assigneeId: values.assigneeId,
    followUpDate: (values.followUpDate as any)?.format('YYYY-MM-DD'),
    response: values.response,
  } as UpdateRiskRequest
}

const EditRiskForm = ({
  showForm,
  riskKey,
  onFormSave,
  onFormCancel,
}: EditRiskFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditRiskFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const [teamName, setTeamName] = useState<string>('')

  const { data: riskData } = useGetRiskQuery(riskKey)

  const [updateRisk, { error: mutationError }] = useUpdateRiskMutation()

  const { data: riskStatusOptions } = useGetRiskStatusOptionsQuery()
  const { data: riskCategoryOptions } = useGetRiskCategoryOptionsQuery()
  const { data: riskGradeOptions } = useGetRiskGradeOptionsQuery()
  const { data: employeeOptions } = useGetEmployeeOptionsQuery(true)

  const { hasPermissionClaim } = useAuth()
  const canUpdateRisks = hasPermissionClaim('Permissions.Risks.Update')

  const mapToFormValues = useCallback(
    (risk: RiskDetailsDto) => {
      form.setFieldsValue({
        riskId: risk.id,
        teamId: risk.team.id,
        summary: risk.summary,
        description: risk.description || '',
        statusId: risk.status.id,
        categoryId: risk.category.id,
        impactId: risk.impact.id,
        likelihoodId: risk.likelihood.id,
        assigneeId: risk.assignee?.id,
        followUpDate: risk.followUpDate ? dayjs(risk.followUpDate) : undefined,
        response: risk.response || '',
      })
    },
    [form],
  )

  const formAction = async (values: EditRiskFormValues) => {
    try {
      const request = mapToRequestValues(values)
      const response = await updateRisk({ request, cacheKey: riskKey })

      if (response.error) {
        throw response.error
      }

      messageApi.success(`Risk updated successfully.`)

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the risk. Please try again.',
        )
      }
      return false
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      if (await formAction(values)) {
        setIsOpen(false)
        onFormSave()
        form.resetFields()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while updating the risk. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }, [form, onFormCancel])

  useEffect(() => {
    if (!riskData) return
    if (canUpdateRisks) {
      setIsOpen(showForm)
      if (showForm) {
        setTeamName(riskData?.team.name)
        mapToFormValues(riskData)
      }
    } else {
      onFormCancel()
      messageApi.error('You do not have permission to update Risks.')
    }
  }, [
    canUpdateRisks,
    onFormCancel,
    mapToFormValues,
    messageApi,
    riskData,
    showForm,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <Modal
      title="Edit Team Risk"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      maskClosable={false}
      keyboard={false} // disable esc key to close modal
      destroyOnClose={true}
    >
      <Flex vertical gap="small">
        <Descriptions size="small" column={1}>
          <Item label="Number">{riskKey}</Item>
          <Item label="Team">{teamName}</Item>
        </Descriptions>
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="update-risk-form"
        >
          <FormItem name="riskId" hidden={true}>
            <Input />
          </FormItem>
          <FormItem name="teamId" hidden={true}>
            <Input />
          </FormItem>
          <FormItem label="Summary" name="summary" rules={[{ required: true }]}>
            <TextArea
              autoSize={{ minRows: 2, maxRows: 4 }}
              showCount
              maxLength={256}
            />
          </FormItem>
          <FormItem
            name="description"
            label="Description"
            rules={[{ max: 1024 }]}
          >
            <MarkdownEditor
              value={form.getFieldValue('description')}
              onChange={(value) =>
                form.setFieldValue('description', value || '')
              }
              maxLength={1024}
            />
          </FormItem>
          <FormItem name="statusId" label="Status" rules={[{ required: true }]}>
            <RadioGroup
              options={riskStatusOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </FormItem>
          <FormItem
            name="categoryId"
            label="Category"
            rules={[{ required: true }]}
          >
            <RadioGroup
              options={riskCategoryOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </FormItem>
          <FormItem name="impactId" label="Impact" rules={[{ required: true }]}>
            <RadioGroup
              options={riskGradeOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </FormItem>
          <FormItem
            name="likelihoodId"
            label="Likelihood"
            rules={[{ required: true }]}
          >
            <RadioGroup
              options={riskGradeOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </FormItem>
          <FormItem name="assigneeId" label="Assignee">
            <Select
              allowClear
              showSearch
              placeholder="Select an assignee"
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.label.toLowerCase() ?? '').includes(
                  input.toLowerCase(),
                )
              }
              filterSort={(optionA, optionB) =>
                (optionA?.label ?? '')
                  .toLowerCase()
                  .localeCompare((optionB?.label ?? '').toLowerCase())
              }
              options={employeeOptions}
            />
          </FormItem>
          <FormItem label="Follow Up" name="followUpDate">
            <DatePicker />
          </FormItem>
          <FormItem name="response" label="Response" rules={[{ max: 1024 }]}>
            <MarkdownEditor
              value={form.getFieldValue('response')}
              onChange={(value) => form.setFieldValue('response', value || '')}
              maxLength={1024}
            />
          </FormItem>
        </Form>
      </Flex>
    </Modal>
  )
}

export default EditRiskForm
