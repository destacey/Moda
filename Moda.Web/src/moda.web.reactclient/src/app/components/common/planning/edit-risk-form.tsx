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
import { getEmployeesClient, getRisksClient } from '@/src/services/clients'
import { RiskDetailsDto, UpdateRiskRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import _ from 'lodash'
import dayjs from 'dayjs'

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

interface OptionModel<T = string> {
  value: T
  label: string
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

  const [riskNumber, setRiskNumber] = useState<number>(undefined)
  const [teamName, setTeamName] = useState<string>('')
  const [employeeOptions, setEmployeeOptions] = useState<OptionModel[]>()
  const [statusOptions, setStatusOptions] = useState<OptionModel<number>[]>()
  const [categoryOptions, setCategoryOptions] =
    useState<OptionModel<number>[]>()
  const [gradeOptions, setGradeOptions] = useState<OptionModel<number>[]>()

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
    [form]
  )

  const mapToRequestValues = useCallback((values: UpdateRiskFormValues) => {
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
  }, [])

  const getRisk = useCallback(async (riskId: string) => {
    const risksClient = await getRisksClient()
    return await risksClient.getById(riskId)
  }, [])

  const getEmployeeOptions = useCallback(async () => {
    const employeesClient = await getEmployeesClient()
    const employeeDtos = await employeesClient.getList(false)
    const employees: OptionModel[] = employeeDtos
      .filter((e) => e.isActive === true)
      .map((e) => ({ value: e.id, label: e.displayName }))

    return _.sortBy(employees, ['label'])
  }, [])

  const getRiskStatusOptions = useCallback(async () => {
    const risksClient = await getRisksClient()
    const statusDtos = await risksClient.getStatuses()
    const statuses: OptionModel<number>[] = statusDtos.map((r) => ({
      value: r.id,
      label: r.name,
    }))

    return _.sortBy(statuses, ['order'])
  }, [])

  const getRiskCategoryOptions = useCallback(async () => {
    const risksClient = await getRisksClient()
    const categoryDtos = await risksClient.getCategories()
    const categories: OptionModel<number>[] = categoryDtos.map((r) => ({
      value: r.id,
      label: r.name,
    }))

    return _.sortBy(categories, ['order'])
  }, [])

  const getRiskGradeOptions = useCallback(async () => {
    const risksClient = await getRisksClient()
    const gradeDtos = await risksClient.getGrades()
    const grades: OptionModel<number>[] = gradeDtos.map((r) => ({
      value: r.id,
      label: r.name,
    }))

    return _.sortBy(grades, ['order'])
  }, [])

  const update = useCallback(
    async (values: UpdateRiskFormValues) => {
      try {
        const request = mapToRequestValues(values)
        const risksClient = await getRisksClient()
        await risksClient.update(request.riskId, request)

        return true
      } catch (error) {
        if (error.status === 422 && error.errors) {
          const formErrors = toFormErrors(error.errors)
          form.setFields(formErrors)
          messageApi.error('Correct the validation error(s) to continue.')
        } else {
          messageApi.error(
            'An unexpected error occurred while updating the Risk.'
          )
          console.error(error)
        }
        return false
      }
    },
    [form, mapToRequestValues, messageApi]
  )

  const handleOk = useCallback(async () => {
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
  }, [form, messageApi, onFormSave, update])

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }, [form, onFormCancel])

  useEffect(() => {
    if (canUpdateRisks) {
      setIsOpen(showForm)
      if (showForm === true) {
        try {
          const loadData = async () => {
            const riskData = await getRisk(riskId)
            setRiskNumber(riskData.localId)
            setTeamName(riskData.team.name)
            mapToFormValues(riskData)
            setEmployeeOptions(await getEmployeeOptions())
            setStatusOptions(await getRiskStatusOptions())
            setCategoryOptions(await getRiskCategoryOptions())
            setGradeOptions(await getRiskGradeOptions())
          }
          loadData()
        } catch (error) {
          handleCancel()
          messageApi.error(
            'An unexpected error occurred while loading form data.'
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
    getRisk,
    getRiskCategoryOptions,
    getRiskGradeOptions,
    getRiskStatusOptions,
    handleCancel,
    mapToFormValues,
    messageApi,
    riskId,
    showForm,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false)
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
        closable={false}
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
            <Input showCount maxLength={256} />
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
              options={statusOptions}
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
              options={categoryOptions}
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
              options={gradeOptions}
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
              options={gradeOptions}
              optionType="button"
              buttonStyle="solid"
            />
          </Form.Item>
          <Form.Item name="assigneeId" label="Assignee">
            <Select
              showSearch
              placeholder="Select an assignee"
              optionFilterProp="children"
              filterOption={(input, option) =>
                (option?.label.toLowerCase() ?? '').includes(
                  input.toLowerCase()
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
