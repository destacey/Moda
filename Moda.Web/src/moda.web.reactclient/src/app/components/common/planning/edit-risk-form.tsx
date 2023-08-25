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
import { getEmployeesClient } from '@/src/services/clients'
import { RiskDetailsDto, UpdateRiskRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import _ from 'lodash'
import dayjs from 'dayjs'
import { OptionModel } from '../../types'
import {
  useGetRiskById,
  useGetRiskCategoryOptions,
  useGetRiskGradeOptions,
  useGetRiskStatusOptions,
  useUpdateRiskMutation,
} from '@/src/services/queries/planning-queries'

export interface UpdateRiskFormProps {
  showForm: boolean
  riskId: string
  onFormSave: () => void
  onFormCancel: () => void
}

interface UpdateRiskFormValues {
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

const mapToRequestValues = (values: UpdateRiskFormValues) => {
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

const UpdateRiskForm = ({
  showForm,
  riskId,
  onFormSave,
  onFormCancel,
}: UpdateRiskFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<UpdateRiskFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const { data: riskData } = useGetRiskById(riskId)
  const [riskNumber, setRiskNumber] = useState<number>(undefined)
  const [teamName, setTeamName] = useState<string>('')
  const [employeeOptions, setEmployeeOptions] = useState<OptionModel[]>()
  const updateRisk = useUpdateRiskMutation()
  const { data: riskStatusOptions } = useGetRiskStatusOptions()
  const { data: riskCategoryOptions } = useGetRiskCategoryOptions()
  const { data: riskGradeOptions } = useGetRiskGradeOptions()

  const { hasClaim } = useAuth()
  const canUpdateRisks = hasClaim('Permission', 'Permissions.Risks.Update')

  const mapToFormValues = useCallback(
    (risk: RiskDetailsDto) => {
      form.setFieldsValue({
        riskId: risk.id,
        teamId: risk.team.id,
        summary: risk.summary,
        description: risk.description,
        statusId: risk.status.id,
        categoryId: risk.category.id,
        impactId: risk.impact.id,
        likelihoodId: risk.likelihood.id,
        assigneeId: risk.assignee?.id,
        followUpDate: risk.followUpDate ? dayjs(risk.followUpDate) : undefined,
        response: risk.response,
      })
    },
    [form],
  )

  const getEmployeeOptions = useCallback(async () => {
    const employeesClient = await getEmployeesClient()
    const employeeDtos = await employeesClient.getList(false)
    const employees: OptionModel[] = employeeDtos
      .filter((e) => e.isActive === true)
      .map((e) => ({ value: e.id, label: e.displayName }))

    return _.sortBy(employees, ['label'])
  }, [])

  const update = async (values: UpdateRiskFormValues) => {
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
            setEmployeeOptions(await getEmployeeOptions())
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
    getEmployeeOptions,
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
            <Descriptions.Item label="Number">{riskNumber}</Descriptions.Item>
            <Descriptions.Item label="Team">{teamName}</Descriptions.Item>
          </Descriptions>
          <Form.Item name="riskId" hidden={true}>
            <Input />
          </Form.Item>
          <Form.Item name="teamId" hidden={true}>
            <Input />
          </Form.Item>
          <Form.Item
            label="Summary"
            name="summary"
            rules={[{ required: true }]}
          >
            <Input.TextArea
              autoSize={{ minRows: 2, maxRows: 4 }}
              showCount
              maxLength={256}
            />
          </Form.Item>
          <Form.Item
            name="description"
            label="Description"
            help="Markdown enabled"
          >
            <Input.TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={1024}
            />
          </Form.Item>
          <Form.Item
            name="statusId"
            label="Status"
            rules={[{ required: true }]}
          >
            <Radio.Group
              options={riskStatusOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </Form.Item>
          <Form.Item
            name="categoryId"
            label="Category"
            rules={[{ required: true }]}
          >
            <Radio.Group
              options={riskCategoryOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </Form.Item>
          <Form.Item
            name="impactId"
            label="Impact"
            rules={[{ required: true }]}
          >
            <Radio.Group
              options={riskGradeOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </Form.Item>
          <Form.Item
            name="likelihoodId"
            label="Likelihood"
            rules={[{ required: true }]}
          >
            <Radio.Group
              options={riskGradeOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </Form.Item>
          <Form.Item name="assigneeId" label="Assignee">
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
          </Form.Item>
          <Form.Item label="Follow Up" name="followUpDate">
            <DatePicker />
          </Form.Item>
          <Form.Item name="response" label="Response" help="Markdown enabled">
            <Input.TextArea
              autoSize={{ minRows: 6, maxRows: 10 }}
              showCount
              maxLength={1024}
            />
          </Form.Item>
        </Form>
      </Modal>
    </>
  )
}

export default UpdateRiskForm
