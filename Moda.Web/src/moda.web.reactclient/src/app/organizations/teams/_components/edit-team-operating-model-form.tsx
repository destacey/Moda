'use client'

import { Form, Modal, Radio, Spin } from 'antd'
import { useCallback, useEffect, useState } from 'react'
import useAuth from '../../../../components/contexts/auth'
import {
  Methodology,
  SizingMethod,
  UpdateTeamOperatingModelRequest,
} from '@/src/services/moda-api'
import { toFormErrors } from '@/src/utils'
import {
  useGetTeamOperatingModelQuery,
  useUpdateTeamOperatingModelMutation,
} from '@/src/store/features/organizations/team-api'
import { useMessage } from '@/src/components/contexts/messaging'

const { Item: FormItem } = Form
const { Group: RadioGroup } = Radio

export interface EditTeamOperatingModelFormProps {
  showForm: boolean
  teamId: string
  operatingModelId: string
  onFormComplete: () => void
  onFormCancel: () => void
}

interface EditTeamOperatingModelFormValues {
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
  values: EditTeamOperatingModelFormValues,
): UpdateTeamOperatingModelRequest => {
  return {
    methodology: values.methodology,
    sizingMethod: values.sizingMethod,
  } as UpdateTeamOperatingModelRequest
}

const EditTeamOperatingModelForm = (props: EditTeamOperatingModelFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditTeamOperatingModelFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const {
    data: operatingModel,
    isLoading,
    isFetching,
  } = useGetTeamOperatingModelQuery(
    { teamId: props.teamId, operatingModelId: props.operatingModelId },
    { skip: !props.showForm },
  )

  const [updateOperatingModel] = useUpdateTeamOperatingModelMutation()

  const { hasClaim } = useAuth()
  const canUpdateTeam = hasClaim('Permission', 'Permissions.Teams.Update')

  const update = async (
    values: EditTeamOperatingModelFormValues,
  ): Promise<boolean> => {
    try {
      const request = mapToRequestValues(values)
      await updateOperatingModel({
        teamId: props.teamId,
        operatingModelId: props.operatingModelId,
        request,
      }).unwrap()
      return true
    } catch (error: any) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An unexpected error occurred while updating the operating model.',
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
        form.resetFields()
        props.onFormComplete()
        messageApi.success('Successfully updated operating model.')
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
    if (!canUpdateTeam) {
      handleCancel()
      messageApi.error('You do not have permission to update teams.')
      return
    }

    setIsOpen(props.showForm)
  }, [canUpdateTeam, handleCancel, messageApi, props.showForm])

  // Set form values when operating model data is loaded
  useEffect(() => {
    if (operatingModel && !isLoading && !isFetching) {
      form.setFieldsValue({
        methodology: operatingModel.methodology,
        sizingMethod: operatingModel.sizingMethod,
      })
    }
  }, [operatingModel, isLoading, isFetching, form])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true),
      () => setIsValid(false),
    )
  }, [form, formValues])

  const isLoadingData = isLoading || isFetching

  return (
    <Modal
      title="Edit Operating Model"
      open={isOpen}
      onOk={handleOk}
      okButtonProps={{ disabled: !isValid || isLoadingData }}
      okText="Save"
      confirmLoading={isSaving}
      onCancel={handleCancel}
      mask={{ blur: false }}
      maskClosable={false}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden={true}
    >
      <Spin spinning={isLoadingData} tip="Loading operating model...">
        <Form
          form={form}
          size="small"
          layout="vertical"
          name="edit-team-operating-model-form"
        >
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
      </Spin>
    </Modal>
  )
}

export default EditTeamOperatingModelForm
