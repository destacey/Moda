'use client'

import { DatePicker, Form, Input, Modal, Radio, Select } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../contexts/auth'
import { getTeamsClient, getTeamsOfTeamsClient } from '@/src/services/clients'
import { CreateRiskRequest } from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import { TeamListItem } from '@/src/app/organizations/types'
import _ from 'lodash'
import { OptionModel } from '../../types'
import { MarkdownEditor } from '../markdown'
import { useMessage } from '../../contexts/messaging'
import {
  useCreateRiskMutation,
  useGetRiskCategoryOptionsQuery,
  useGetRiskGradeOptionsQuery,
} from '@/src/store/features/planning/risks-api'
import { useGetEmployeeOptionsQuery } from '@/src/store/features/organizations/employee-api'

const { Item } = Form
const { TextArea } = Input
const { Group: RadioGroup } = Radio

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

const mapToRequestValues = (values: CreateRiskFormValues) => {
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
  const messageApi = useMessage()

  const [teamOptions, setTeamOptions] = useState<OptionModel[]>()

  const [newRiskKey, setNewRiskKey] = useState<number>(null)

  const { hasClaim } = useAuth()
  const canCreateRisks = hasClaim('Permission', 'Permissions.Risks.Create')

  const [createRisk, { error: mutationError }] = useCreateRiskMutation()
  const { data: riskCategoryOptions } = useGetRiskCategoryOptionsQuery()
  const { data: riskGradeOptions } = useGetRiskGradeOptionsQuery()
  const { data: employeeOptions } = useGetEmployeeOptionsQuery(false)

  const mapToFormValues = useCallback(
    (teamId: string | undefined) => {
      form.setFieldsValue({
        teamId: teamId,
        description: '',
        response: '',
      })
    },
    [form],
  )

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

  const formAction = async (values: CreateRiskFormValues) => {
    try {
      const request = mapToRequestValues(values)
      const response = await createRisk(request)
      if (response.error) {
        throw response.error
      }

      messageApi.success(
        `Successfully created Risk. Risk key: ${response.data.key}`,
      )

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while creating the strategic initiative. Please try again.',
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
        form.resetFields()
        onFormCreate()
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while creating the risk. Please try again.',
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
    if (canCreateRisks) {
      setIsOpen(showForm)
      if (showForm === true) {
        try {
          const loadData = async () => {
            setTeamOptions(await getTeamOptions())
          }
          loadData()
          mapToFormValues(createForTeamId)
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
      messageApi.error('You do not have permission to create Risks.')
    }
  }, [
    canCreateRisks,
    createForTeamId,
    getTeamOptions,
    handleCancel,
    mapToFormValues,
    messageApi,
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
      title="Create Team Risk"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Create"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      maskClosable={false}
      keyboard={false} // disable esc key to close modal
      destroyOnClose={true}
    >
      <Form form={form} size="small" layout="vertical" name="create-risk-form">
        <Item name="teamId" label="Team" rules={[{ required: true }]}>
          <Select
            showSearch
            disabled={createForTeamId !== undefined}
            placeholder="Select a team"
            optionFilterProp="children"
            filterOption={(input, option) =>
              (option?.label.toLowerCase() ?? '').includes(input.toLowerCase())
            }
            filterSort={(optionA, optionB) =>
              (optionA?.label ?? '')
                .toLowerCase()
                .localeCompare((optionB?.label ?? '').toLowerCase())
            }
            options={teamOptions}
          />
        </Item>
        <Item label="Summary" name="summary" rules={[{ required: true }]}>
          <TextArea
            autoSize={{ minRows: 2, maxRows: 4 }}
            showCount
            maxLength={256}
          />
        </Item>
        <Item name="description" label="Description" rules={[{ max: 1024 }]}>
          <MarkdownEditor
            value={form.getFieldValue('description')}
            onChange={(value) => form.setFieldValue('description', value || '')}
            maxLength={1024}
          />
        </Item>
        <Item name="categoryId" label="Category" rules={[{ required: true }]}>
          <RadioGroup
            options={riskCategoryOptions}
            optionType="button"
            buttonStyle="solid"
          />
        </Item>
        <Item name="impactId" label="Impact" rules={[{ required: true }]}>
          <RadioGroup
            options={riskGradeOptions}
            optionType="button"
            buttonStyle="solid"
          />
        </Item>
        <Item
          name="likelihoodId"
          label="Likelihood"
          rules={[{ required: true }]}
        >
          <RadioGroup
            options={riskGradeOptions}
            optionType="button"
            buttonStyle="solid"
          />
        </Item>
        <Item name="assigneeId" label="Assignee">
          <Select
            allowClear
            showSearch
            placeholder="Select an assignee"
            optionFilterProp="children"
            filterOption={(input, option) =>
              (option?.label.toLowerCase() ?? '').includes(input.toLowerCase())
            }
            filterSort={(optionA, optionB) =>
              (optionA?.label ?? '')
                .toLowerCase()
                .localeCompare((optionB?.label ?? '').toLowerCase())
            }
            options={employeeOptions}
          />
        </Item>
        <Item label="Follow Up" name="followUpDate">
          <DatePicker />
        </Item>
        <Item name="response" label="Response" rules={[{ max: 1024 }]}>
          <MarkdownEditor
            value={form.getFieldValue('response')}
            onChange={(value) => form.setFieldValue('response', value || '')}
            maxLength={1024}
          />
        </Item>
      </Form>
    </Modal>
  )
}

export default CreateRiskForm
