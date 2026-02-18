'use client'

import { DatePicker, Form, Modal, Radio } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import {
  Methodology,
  SetTeamOperatingModelRequest,
  SizingMethod,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import { useSetTeamOperatingModelMutation } from '@/src/store/features/organizations/team-api'
import { useMessage } from '@/src/components/contexts/messaging'
import useAuth from '@/src/components/contexts/auth'

const { Item: FormItem } = Form
const { Group: RadioGroup } = Radio

export interface SetTeamOperatingModelFormProps {
  showForm: boolean
  teamId: string
  onFormComplete: () => void
  onFormCancel: () => void
}

interface SetTeamOperatingModelFormValues {
  startDate: Date
  methodology: Methodology
  sizingMethod: SizingMethod
}

const methodologyOptions = [
  { value: Methodology.Scrum, label: 'Scrum' },
  { value: Methodology.Kanban, label: 'Kanban' },
]

const sizingMethodOptions = [
  { value: SizingMethod.StoryPoints, label: 'Story Points' },
  { value: SizingMethod.Count, label: 'Count' },
]

const mapToRequestValues = (
  values: SetTeamOperatingModelFormValues,
): SetTeamOperatingModelRequest => {
  return {
    startDate: (values.startDate as any)?.format('YYYY-MM-DD'),
    methodology: values.methodology,
    sizingMethod: values.sizingMethod,
  } as SetTeamOperatingModelRequest
}

const SetTeamOperatingModelForm = (props: SetTeamOperatingModelFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<SetTeamOperatingModelFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const [setOperatingModel] = useSetTeamOperatingModelMutation()

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')

  const create = async (
    values: SetTeamOperatingModelFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      await setOperatingModel({ teamId: props.teamId, request }).unwrap()
      return true
    } catch (error: any) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An unexpected error occurred while setting the operating model.',
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
      if (await create(values)) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete()
        messageApi.success('Successfully set operating model.')
      }
    } catch (errorInfo) {
      console.log('handleOk error', errorInfo)
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    props.onFormCancel()
    form.resetFields()
  }, [form, props])

  useEffect(() => {
    if (canUpdateTeam) {
      setIsOpen(props.showForm)
    } else {
      handleCancel()
      messageApi.error('You do not have permission to update teams.')
    }
  }, [canUpdateTeam, handleCancel, messageApi, props.showForm])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <Modal
      title="Set Operating Model"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden={true}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="set-team-operating-model-form"
      >
        <FormItem
          name="startDate"
          label="Start Date"
          rules={[{ required: true, message: 'Start date is required' }]}
        >
          <DatePicker style={{ width: '100%' }} />
        </FormItem>
        <FormItem
          name="methodology"
          label="Methodology"
          rules={[{ required: true, message: 'Methodology is required' }]}
        >
          <RadioGroup
            options={methodologyOptions}
            optionType="button"
            buttonStyle="solid"
          />
        </FormItem>
        <FormItem
          name="sizingMethod"
          label="Sizing Method"
          rules={[{ required: true, message: 'Sizing method is required' }]}
        >
          <RadioGroup
            options={sizingMethodOptions}
            optionType="button"
            buttonStyle="solid"
          />
        </FormItem>
      </Form>
    </Modal>
  )
}

export default SetTeamOperatingModelForm

