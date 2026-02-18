'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import { UpdateWorkTypeRequest, WorkTypeDto } from '@/src/services/moda-api'
import {
  useGetWorkTypeQuery,
  useUpdateWorkTypeMutation,
} from '@/src/store/features/work-management/work-type-api'
import { useGetWorkTypeLevelOptionsQuery } from '@/src/store/features/work-management/work-type-level-api'
import { toFormErrors } from '@/src/utils'
import { Descriptions, Form, Input, Modal, Select } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { TextArea } = Input
const { Item: DescriptionItem } = Descriptions

export interface EditWorkTypeFormProps {
  showForm: boolean
  workTypeId: number
  onFormSave: () => void
  onFormCancel: () => void
}

interface EditWorkTypeFormValues {
  description?: string
  levelId: number
}

const mapToRequestValues = (
  workTypeId: number,
  values: EditWorkTypeFormValues,
): UpdateWorkTypeRequest => {
  return {
    id: workTypeId,
    description: values.description,
    levelId: values.levelId,
  }
}

const EditWorkTypeForm = (props: EditWorkTypeFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<EditWorkTypeFormValues>()
  const formValues = Form.useWatch([], form)
  const messageApi = useMessage()

  const { data: workTypeData } = useGetWorkTypeQuery(props.workTypeId)
  // TODO: why do I have to pass null here?
  const { data: workTypeLevelOptions } = useGetWorkTypeLevelOptionsQuery(null)
  const [updateWorkTypeMutation] = useUpdateWorkTypeMutation()

  const { hasClaim } = useAuth()
  const canUpdateWorkType = hasClaim(
    'Permission',
    'Permissions.WorkTypes.Update',
  )

  const mapToFormValues = useCallback(
    (workType: WorkTypeDto) => {
      form.setFieldsValue({
        description: workType.description,
        levelId: workType.level.id,
      })
    },
    [form],
  )

  const handleSave = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      const request = mapToRequestValues(props.workTypeId, values)
      const response = await updateWorkTypeMutation(request)
      if (response.error) {
        throw response.error
      }

      setIsSaving(false)
      setIsOpen(false)
      form.resetFields()
      props.onFormSave()
      messageApi.success('Successfully updated work type.')
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          'An unexpected error occurred while creating the work type.',
        )
        console.error('handleSave error', error)
      }
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    props.onFormCancel()
    form.resetFields()
  }, [props, form])

  useEffect(() => {
    if (!workTypeData) return

    if (canUpdateWorkType) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        try {
          mapToFormValues(workTypeData)
          setIsValid(true)
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
      messageApi.error(`You do not have permission to update work types.`)
    }
  }, [
    canUpdateWorkType,
    form,
    handleCancel,
    mapToFormValues,
    messageApi,
    props.showForm,
    workTypeData,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <Modal
      title="Edit Work Type"
      open={isOpen}
      onOk={handleSave}
      okButtonProps={{ disabled: !isValid }}
      okText="Save"
      onCancel={handleCancel}
      confirmLoading={isSaving}
      keyboard={false} // disable esc key to close modal
      destroyOnHidden={true}
    >
      <Form
        form={form}
        size="small"
        layout="vertical"
        name="edit-work-type-form"
      >
        <Descriptions size="small" column={1}>
          <DescriptionItem label="Name">{workTypeData?.name}</DescriptionItem>
        </Descriptions>
        <Item label="Description" name="description">
          <TextArea
            autoSize={{ minRows: 4, maxRows: 10 }}
            showCount
            maxLength={1024}
          />
        </Item>
        <Item label="Level" name="levelId" rules={[{ required: true }]}>
          <Select options={workTypeLevelOptions} />
        </Item>
      </Form>
    </Modal>
  )
}

export default EditWorkTypeForm
