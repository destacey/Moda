'use client'

import { MarkdownEditor } from '@/src/components/common/markdown'
import useAuth from '@/src/components/contexts/auth'
import {
  ProjectPortfolioDetailsDto,
  UpdatePortfolioRequest,
} from '@/src/services/moda-api'
import {
  useGetPortfolioQuery,
  useUpdatePortfolioMutation,
} from '@/src/store/features/ppm/portfolios-api'
import { toFormErrors } from '@/src/utils'
import { Form, Input, Modal } from 'antd'
import TextArea from 'antd/es/input/TextArea'
import { MessageInstance } from 'antd/es/message/interface'
import { useCallback, useEffect, useState } from 'react'

const { Item } = Form

export interface EditPortfolioFormProps {
  portfolioKey: number
  showForm: boolean
  onFormComplete: () => void
  onFormCancel: () => void
  messageApi: MessageInstance
}

interface UpdatePortfolioFormValues {
  name: string
  description: string
}

const mapToRequestValues = (
  values: UpdatePortfolioFormValues,
  strategicThemeId: string,
): UpdatePortfolioRequest => {
  return {
    id: strategicThemeId,
    name: values.name,
    description: values.description,
  } as UpdatePortfolioRequest
}

const EditPortfolioForm = (props: EditPortfolioFormProps) => {
  const [isOpen, setIsOpen] = useState(false)
  const [isSaving, setIsSaving] = useState(false)
  const [isValid, setIsValid] = useState(false)
  const [form] = Form.useForm<UpdatePortfolioFormValues>()
  const formValues = Form.useWatch([], form)

  const {
    data: portfolioData,
    isLoading,
    error,
  } = useGetPortfolioQuery(props.portfolioKey)

  const [updatePortfolio, { error: mutationError }] =
    useUpdatePortfolioMutation()

  const { hasPermissionClaim } = useAuth()
  const canUpdatePortfolio = hasPermissionClaim(
    'Permissions.ProjectPortfolios.Update',
  )

  const mapToFormValues = useCallback(
    (strategicTheme: ProjectPortfolioDetailsDto) => {
      if (!strategicTheme) {
        throw new Error('Portfolio not found')
      }
      form.setFieldsValue({
        name: strategicTheme.name,
        description: strategicTheme.description,
      })
    },
    [form],
  )

  const update = async (values: UpdatePortfolioFormValues) => {
    try {
      const request = mapToRequestValues(values, portfolioData.id)
      const response = await updatePortfolio({
        request,
        cacheKey: portfolioData.key,
      })
      if (response.error) {
        throw response.error
      }
      props.messageApi.success('Portfolio updated successfully.')

      return true
    } catch (error) {
      if (error.status === 422 && error.errors) {
        const formErrors = toFormErrors(error.errors)
        form.setFields(formErrors)
        props.messageApi.error('Correct the validation error(s) to continue.')
      } else {
        props.messageApi.error(
          error.detail ??
            'An error occurred while updating the Portfolio. Please try again.',
        )
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
      }
    } catch (error) {
      console.error('update error', error)
      props.messageApi.error(
        'An error occurred while updating the Portfolio. Please try again.',
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
    if (!portfolioData) return

    if (canUpdatePortfolio) {
      setIsOpen(props.showForm)
      if (props.showForm) {
        mapToFormValues(portfolioData)
      }
    } else {
      props.onFormCancel()
      props.messageApi.error('You do not have permission to update Portfolios.')
    }
  }, [canUpdatePortfolio, mapToFormValues, props, portfolioData])

  useEffect(() => {
    form.validateFields({ validateOnly: true }).then(
      () => setIsValid(true && form.isFieldsTouched()),
      () => setIsValid(false),
    )
  }, [form, formValues])

  useEffect(() => {
    if (error) {
      props.messageApi.error(
        error.detail ??
          'An error occurred while loading the Portfolio. Please try again.',
      )
    }
  }, [error, props.messageApi])

  return (
    <>
      <Modal
        title="Edit Portfolio"
        open={isOpen}
        width={'60vw'}
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
          name="update-portfolio-form"
        >
          <Item
            label="Name"
            name="name"
            rules={[{ required: true, message: 'Name is required' }]}
          >
            <Input maxLength={128} showCount />
          </Item>
          <Item name="description" label="Description" rules={[{ max: 1024 }]}>
            <MarkdownEditor
              value={form.getFieldValue('description')}
              onChange={(value) =>
                form.setFieldValue('description', value || '')
              }
              maxLength={1024}
            />
          </Item>
        </Form>
      </Modal>
    </>
  )
}

export default EditPortfolioForm
