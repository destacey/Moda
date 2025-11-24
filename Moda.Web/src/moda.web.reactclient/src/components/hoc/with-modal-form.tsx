import { Form, FormInstance, Modal } from 'antd'
import { ComponentType, FC, useEffect, useState } from 'react'
import { FieldData } from 'rc-field-form/lib/interface'
import { useAppDispatch } from '../../hooks'
import { Action, ThunkAction } from '@reduxjs/toolkit'
import { RootState } from '@/src/store'
import { useMessage } from '../contexts/messaging'

export interface ModalFormProps<TFormValues> {
  title: string
  okText?: string
  useFormState: () => FormState
  onOk: (
    values: TFormValues,
  ) => Action | ThunkAction<void, RootState, unknown, Action<string>>
  onCancel: Action
}

export interface FormProps<TFormValues> {
  form: FormInstance<TFormValues>
  onClose?: (success: boolean) => void
}

export interface FormState {
  isInEditMode: boolean
  isSaving: boolean
  error: any
  validationErrors: FieldData[]
}

// withModalForm is a higher-order component that accepts a generic type parameter of the typeof form values and wraps a form in a modal dialog.
const withModalForm = <P extends FormProps<TFormValues>, TFormValues>(
  WrappedForm: ComponentType<P>,
  modalFormProps: ModalFormProps<TFormValues>,
): FC<Omit<P, 'form'>> => {
  const wrappedComponentName =
    WrappedForm.displayName || WrappedForm.name || 'Component'

  const WithModalForm: ComponentType<Omit<P, 'form'>> = ({ ...props }) => {
    const [isValid, setIsValid] = useState(false)
    const [form] = Form.useForm<TFormValues>()
    const formValues = Form.useWatch([], form)
    const messageApi = useMessage()

    const dispatch = useAppDispatch()
    const { isInEditMode, isSaving, error, validationErrors } =
      modalFormProps.useFormState()

    const handleOk = async () => {
      try {
        const values = await form.validateFields()
        // dispacth and await the thunk action, it may be an async thunk action or a regular thunk action
        await dispatch(modalFormProps.onOk(values))
        props.onClose?.(true)
      } catch (errorInfo) {
        console.error(errorInfo)
      }
    }

    const handleCancel = () => {
      dispatch(modalFormProps.onCancel)
      form.resetFields()
      props.onClose?.(false)
    }

    useEffect(() => {
      form.validateFields({ validateOnly: true }).then(
        () => setIsValid(true && form.isFieldsTouched()),
        () => setIsValid(false),
      )
    }, [form, formValues])

    useEffect(() => {
      if (error) {
        console.error(error)
        messageApi.error('An unexpected error occurred while saving.')
      }
    }, [error, messageApi])

    useEffect(() => {
      if (form && validationErrors?.length > 0) {
        form.setFields(validationErrors)
        messageApi.error('Correct the validation error(s) to continue.')
      }
    }, [validationErrors, messageApi, form])

    return (
      <Modal
        title={modalFormProps.title}
        open={isInEditMode}
        onOk={handleOk}
        okButtonProps={{ disabled: !isValid }}
        okText={modalFormProps.okText ?? 'Save'}
        confirmLoading={isSaving}
        onCancel={handleCancel}
        maskClosable={false}
        keyboard={false} // disable esc key to close modal
        destroyOnHidden={true}
      >
        <WrappedForm form={form} {...(props as P)} />
      </Modal>
    )
  }

  WithModalForm.displayName = `withModalForm(${wrappedComponentName})`

  return WithModalForm
}

export default withModalForm
