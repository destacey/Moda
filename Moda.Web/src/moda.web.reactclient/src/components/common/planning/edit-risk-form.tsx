'use client'

import {
  DatePicker,
  Descriptions,
  Form,
  Input,
  Modal,
  Radio,
  Select,
  message,
} from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../contexts/auth'
import { RiskDetailsDto, UpdateRiskRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import dayjs from 'dayjs'
import {
  useGetRisk,
  useGetRiskCategoryOptions,
  useGetRiskGradeOptions,
  useGetRiskStatusOptions,
  useUpdateRiskMutation,
} from '@/src/services/queries/planning-queries'
import { useGetEmployeeOptions } from '@/src/services/queries/organization-queries'
import { MarkdownEditor } from '../markdown'

const { Item } = Descriptions
const { Item: FormItem } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio

export interface EditRiskFormProps {
  showForm: boolean
  riskId: string
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
  riskId,
  onFormSave,
  onFormCancel,
}: EditRiskFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditRiskFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { data: riskData } = useGetRisk(riskId)
  const [riskNumber, setRiskNumber] = useState<number>(undefined)
  const [teamName, setTeamName] = useState<string>('')
  const updateRisk = useUpdateRiskMutation()
  const { data: riskStatusOptions } = useGetRiskStatusOptions()
  const { data: riskCategoryOptions } = useGetRiskCategoryOptions()
  const { data: riskGradeOptions } = useGetRiskGradeOptions()
  const { data: employeeOptions } = useGetEmployeeOptions()

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

  const update = async (values: EditRiskFormValues) => {
    try {
      const request = mapToRequestValues(values)
      await updateRisk.mutateAsync(request)

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An unexpected error occurred while updating the Risk.',
        )
        console.error(error)
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
        onFormSave()
        form.resetFields()
        messageApi.success('Successfully updated risk.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
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
      if (showForm === true) {
        try {
          const loadData = async () => {
            setRiskNumber(riskData?.key)
            setTeamName(riskData?.team.name)
            mapToFormValues(riskData)
          }
          loadData()
        } catch (error) {
          handleCancel()
          messageApi.error(
            'An unexpected error occurred while loading form data.',
          )
          console.error(error)
        }
      }
    } else {
      handleCancel()
      messageApi.error('You do not have permission to update Risks.')
    }
  }, [
    canUpdateRisks,
    handleCancel,
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
    <>
      {contextHolder}
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
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="update-risk-form"
        >
          <Descriptions size="small" column={1}>
            <Item label="Number">{riskNumber}</Item>
            <Item label="Team">{teamName}</Item>
          </Descriptions>
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
      </Modal>
    </>
  )
}

export default EditRiskForm
