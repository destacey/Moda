'use client'

import { DatePicker, Form, Input, Modal, Radio, Select, message } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../contexts/auth'
import {
  getEmployeesClient,
  getRisksClient,
  getTeamsClient,
  getTeamsOfTeamsClient,
} from '@/src/services/clients'
import { CreateRiskRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import { TeamListItem } from '@/src/app/organizations/types'
import _ from 'lodash'

export interface CreateRiskFormProps {
  showForm: boolean
  createForTeamId?: string | undefined
  onFormCreate: () => void
  onFormCancel: () => void
}

interface CreateRiskFormValues {
  teamId: string
  summary: string
  description?: string | undefined
  categoryId: number
  impactId: number
  likelihoodId: number
  assigneeId?: string | undefined
  followUpDate?: Date | undefined
  response?: string | undefined
}

interface OptionModel {
  value: string
  label: string
}

const CreateRiskForm = ({
  showForm,
  createForTeamId,
  onFormCreate,
  onFormCancel,
}: CreateRiskFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<CreateRiskFormValues>()
  const formValues = Form.useWatch([], form)
  const [messageApi, contextHolder] = message.useMessage()

  const [teamOptions, setTeamOptions] = useState<OptionModel[]>()
  const [employeeOptions, setEmployeeOptions] = useState<OptionModel[]>()
  const [categoryOptions, setCategoryOptions] = useState<OptionModel[]>()
  const [gradeOptions, setGradeOptions] = useState<OptionModel[]>()

  const [newRiskLocalId, setNewRiskLocalId] = useState<number>(null)

  const { hasClaim } = useAuth()
  const canCreateRisks = hasClaim('Permission', 'Permissions.Risks.Create')

  const mapToFormValues = useCallback(
    (teamId: string | undefined) => {
      form.setFieldsValue({
        teamId: teamId,
      })
    },
    [form]
  )

  const mapToRequestValues = useCallback((values: CreateRiskFormValues) => {
    return {
      teamId: values.teamId,
      summary: values.summary,
      description: values.description,
      categoryId: values.categoryId,
      impactId: values.impactId,
      likelihoodId: values.likelihoodId,
      assigneeId: values.assigneeId,
      followUpDate: (values.followUpDate as any)?.format('YYYY-MM-DD'),
      response: values.response,
    } as CreateRiskRequest
  }, [])

  const getTeamOptions = useCallback(async () => {
    const teamsClient = await getTeamsClient()
    const teamsDtos = await teamsClient.getList(false)
    const teamOfTeamsClient = await getTeamsOfTeamsClient()
    const teamOfTeamsDtos = await teamOfTeamsClient.getList(false)
    const teams: OptionModel[] = [
      ...(teamsDtos as TeamListItem[]),
      ...(teamOfTeamsDtos as TeamListItem[]),
    ].map((t) => ({ value: t.id, label: t.name }))

    return _.sortBy(teams, ['label'])
  }, [])

  const getEmployeeOptions = useCallback(async () => {
    const employeesClient = await getEmployeesClient()
    const employeeDtos = await employeesClient.getList(false)
    const employees: OptionModel[] = employeeDtos
      .filter((e) => e.isActive === true)
      .map((e) => ({ value: e.id, label: e.displayName }))

    return _.sortBy(employees, ['label'])
  }, [])

  const getRiskCategorOptions = useCallback(async () => {
    const risksClient = await getRisksClient()
    const categoryDtos = await risksClient.getCategories()
    const categories: OptionModel[] = categoryDtos.map((r) => ({
      value: r.id.toString(),
      label: r.name,
    }))

    return _.sortBy(categories, ['order'])
  }, [])

  const getRiskGradeOptions = useCallback(async () => {
    const risksClient = await getRisksClient()
    const gradeDtos = await risksClient.getGrades()
    const grades: OptionModel[] = gradeDtos.map((r) => ({
      value: r.id.toString(),
      label: r.name,
    }))

    return _.sortBy(grades, ['order'])
  }, [])

  const create = useCallback(
    async (values: CreateRiskFormValues) => {
      try {
        const request = mapToRequestValues(values)
        const risksClient = await getRisksClient()
        const localId = await risksClient.createRisk(request)
        setNewRiskLocalId(localId)

        return true
      } catch (error) {
        if (error.status === 422 && error.errors) {
          const formErrors = toFormErrors(error.errors)
          form.setFields(formErrors)
          messageApi.error('Correct the validation error(s) to continue.')
        } else {
          messageApi.error(
            'An unexpected error occurred while creating the Risk.'
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
      await create(values)
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }, [form, create])

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    onFormCancel()
    form.resetFields()
  }, [form, onFormCancel])

  useEffect(() => {
    if (canCreateRisks) {
      setIsOpen(showForm)
      if (showForm === true) {
        try {
          const loadData = async () => {
            setTeamOptions(await getTeamOptions())
            setEmployeeOptions(await getEmployeeOptions())
            setCategoryOptions(await getRiskCategorOptions())
            setGradeOptions(await getRiskGradeOptions())
          }
          loadData()
          mapToFormValues(createForTeamId)
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
      messageApi.error('You do not have permission to create Risks.')
    }
  }, [
    canCreateRisks,
    createForTeamId,
    getEmployeeOptions,
    getRiskCategorOptions,
    getRiskGradeOptions,
    getTeamOptions,
    handleCancel,
    mapToFormValues,
    messageApi,
    showForm,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false)
    )
  }, [form, formValues])

  useEffect(() => {
    if (newRiskLocalId) {
      setIsOpen(false)
      form.resetFields()
      onFormCreate()
      messageApi.success('Successfully created Risk.')
    }
    // we don't want a trigger on the other dependencies
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [newRiskLocalId])

  return (
    <>
      {contextHolder}
      <Modal
        title="Create Team Risk"
        open={isOpen}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Create"
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
          name="create-risk-form"
        >
          <Form.Item name="teamId" label="Team" rules={[{ required: true }]}>
            <Select
              showSearch
              disabled={createForTeamId !== undefined}
              placeholder="Select a team"
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
              options={teamOptions}
            />
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
              allowClear
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

export default CreateRiskForm
