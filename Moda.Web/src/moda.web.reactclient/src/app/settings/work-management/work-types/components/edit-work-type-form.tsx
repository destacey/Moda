'use client'

import { UpdateWorkTypeRequest, WorkTypeDto } from '@/src/services/moda-api'
import {
  useGetWorkTypeQuery,
  useUpdateWorkTypeMutation,
} from '@/src/store/features/work-management/work-type-api'
import { useGetWorkTypeLevelOptionsQuery } from '@/src/store/features/work-management/work-type-level-api'
import { Descriptions, Form, Input, Modal, Select, message } from 'antd'
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
  const [messageApi, contextHolder] = message.useMessage()

  const { data: workTypeData } = useGetWorkTypeQuery(props.workTypeId)
  // TODO: why do I have to pass null here?
  const { data: workTypeLevelOptions } = useGetWorkTypeLevelOptionsQuery(null)
  const [updateWorkTypeMutation] = useUpdateWorkTypeMutation()

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
      await updateWorkTypeMutation(request)
      setIsSaving(false)
      setIsOpen(false)
      form.resetFields()
      props.onFormSave()
      messageApi.success('Successfully updated work type.')
    } catch (error) {
      setIsSaving(false)
      messageApi.error('Error updating work type')
      console.error('handleSave error', error)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    props.onFormCancel()
    form.resetFields()
  }, [props, form])

  useEffect(() => {
    if (!workTypeData) return
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
  }, [
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
    <>
      {contextHolder}
      <Modal
        title="Edit Work Type"
        open={isOpen}
        onOk={handleSave}
        okButtonProps={{ disabled: !isValid }}
        onCancel={handleCancel}
        confirmLoading={isSaving}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnClose={true}
      >
        <Form form={form} layout="vertical">
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
    </>
  )
}

export default EditWorkTypeForm
