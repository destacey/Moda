'use client'

import useAuth from '@/src/components/contexts/auth'
import { useMessage } from '@/src/components/contexts/messaging'
import {
  ProjectDetailsDto,
  ChangeProjectKeyRequest,
} from '@/src/services/moda-api'
import {
  useChangeProjectKeyMutation,
  useGetProjectQuery,
} from '@/src/store/features/ppm/projects-api'
import { toFormErrors } from '@/src/utils'
import { Flex, Form, Input, Modal, Typography } from 'antd'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form
const { Text } = Typography

export interface ChangeProjectKeyFormProps {
  projectKey: string
  onFormComplete: (newKey: string) => void
  onFormCancel: () => void
}

interface ChangeProjectKeyFormValues {
  key: string
}

const keyPattern = /^[A-Z0-9]{2,20}$/

const ChangeProjectKeyForm = (props: ChangeProjectKeyFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [isInitialized, setIsInitialized] = useState(false)
  const [form] = Form.useForm<ChangeProjectKeyFormValues>()
  const formValues = Form.useWatch([], form)

  const messageApi = useMessage()

  const { data: projectData } = useGetProjectQuery(props.projectKey)

  const [changeProjectKey] = useChangeProjectKeyMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdateProject = hasPermissionClaim('Permissions.Projects.Update')

  const mapToFormValues = useCallback(
    (project: ProjectDetailsDto) => {
      if (!project) {
        throw new Error('Project not found')
      }
      form.setFieldsValue({
        key: project.key,
      })
      setIsInitialized(true)
    },
    [form],
  )

  const changeKey = async (
    values: ChangeProjectKeyFormValues,
    project: ProjectDetailsDto,
  ) => {
    try {
      const newKey = values.key.trim().toUpperCase()
      const request: ChangeProjectKeyRequest = {
        key: newKey,
      }

      const response = await changeProjectKey({
        id: project.id,
        request,
      })

      if (response.error) {
        throw response.error
      }

      messageApi.success(`Project key changed successfully.`)
      return newKey
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      } else {
        messageApi.error(
          error.detail ??
            'An error occurred while updating the project key. Please try again.',
        )
      }
      return null
    }
  }

  const handleOk = async () => {
    setIsSaving(true)
    try {
      const values = await form.validateFields()
      const newKey = await changeKey(values, projectData)
      if (newKey) {
        setIsOpen(false)
        form.resetFields()
        props.onFormComplete(newKey)
      }
    } catch (error) {
      console.error('handleOk error', error)
      messageApi.error(
        'An error occurred while changing the project key. Please try again.',
      )
    } finally {
      setIsSaving(false)
    }
  }

  const handleCancel = useCallback(() => {
    setIsOpen(false)
    form.resetFields()
    props.onFormCancel()
  }, [form, props])

  useEffect(() => {
    if (!projectData || isInitialized) return

    if (canUpdateProject) {
      mapToFormValues(projectData)
      setIsOpen(true)
    } else {
      props.onFormCancel()
      messageApi.error('You do not have permission to update projects.')
    }
  }, [
    canUpdateProject,
    isInitialized,
    mapToFormValues,
    messageApi,
    projectData,
    props,
  ])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  return (
    <>
      <Modal
        title="Change Project Key"
        open={isOpen}
        width={'40vw'}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText="Save"
        confirmLoading={isSaving}
        onCancel={handleCancel}
        keyboard={false}
        destroyOnHidden={true}
      >
        <Flex vertical gap="small">
          <Text type="secondary">Enter a new key for this project.</Text>
          <Form
            form={form}
            size="small"
            layout="vertical"
            name="change-project-key-form"
          >
            <Item
              name="key"
              label="Key"
              extra="2-20 uppercase alphanumeric characters (A-Z, 0-9)"
              rules={[
                { required: true, message: 'Key is required.' },
                {
                  pattern: keyPattern,
                  message:
                    'Key must be 2-20 uppercase alphanumeric characters (A-Z, 0-9).',
                },
              ]}
              normalize={(value) => (value ?? '').toUpperCase()}
            >
              <Input
                placeholder="Enter new key"
                autoComplete="off"
                showCount
                maxLength={20}
              />
            </Item>
          </Form>
        </Flex>
      </Modal>
    </>
  )
}

export default ChangeProjectKeyForm

