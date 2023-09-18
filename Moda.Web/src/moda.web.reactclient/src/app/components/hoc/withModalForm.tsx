import { Form, FormInstance, Modal, message } from 'antd'
import { ComponentType, FC, useEffect, useState } from 'react'
import { FieldData } from 'rc-field-form/lib/interface'
import { useAppDispatch } from '../../hooks'
import { AppDispatch } from '@/src/store'

export interface ModalFormProps<T> {
    title: string
    okText?: string
    useFormState: () => FormState
    onOk: (values: T, dispatch: AppDispatch) => Promise<void> | void
    onCancel: (dispatch: AppDispatch) => void
}

export interface FormProps<T> {
    form: FormInstance<T>
  }

export interface FormState {
    isInEditMode: boolean
    isSaving: boolean
    error: any
    validationErrors: FieldData[]
}

// withModalForm is a higher-order component that accepts a generic type parameter of the typeof form values and wraps a form in a modal dialog.
const withModalForm = <P extends FormProps<T>, T>(
    WrappedForm: ComponentType<P>,
    modalFormProps: ModalFormProps<T>
): FC<Omit<P, "form">> => {
    const WithModalForm: ComponentType<Omit<P, "form">> = ({
        ...props
    }) => {
        const wrappedComponentName =
        WrappedForm.displayName || WrappedForm.name || 'Component'

        WithModalForm.displayName = `withModalForm(${wrappedComponentName})`

        const [isValid, setIsValid] = useState(false)
        const [form] = Form.useForm<T>()
        const formValues = Form.useWatch([], form)
        const [messageApi, contextHolder] = message.useMessage()

        const dispatch = useAppDispatch()
        const { isInEditMode, isSaving, error, validationErrors } = modalFormProps.useFormState()

        const handleOk = async () => {
            try {
                const values = await form.validateFields()
                await modalFormProps.onOk(values, dispatch)
            } catch (errorInfo) {
                console.error(errorInfo)
            }
        }

        const handleCancel = () => {
            modalFormProps.onCancel(dispatch)
            form.resetFields()
        }

        useEffect(() => {
            form.validateFields({ validateOnly: true }).then(
              () => setIsValid(true && form.isFieldsTouched()),
              () => setIsValid(false)
            )
          }, [form, formValues])

          useEffect(() => {
            if(error) {
              console.error(error)
              messageApi.error('An unexpected error occurred while saving.')
            }
          }, [error, messageApi])
        
          useEffect(() => {
            if(form && validationErrors?.length > 0) {
              form.setFields(validationErrors)
              messageApi.error('Correct the validation error(s) to continue.')
            }
          }, [validationErrors, messageApi, form])

        return (
            <>
                {contextHolder}
                <Modal
                    title={modalFormProps.title}
                    open={isInEditMode}
                    onOk={handleOk}
                    okButtonProps={{ disabled: !isValid }}
                    okText={modalFormProps.okText ?? "Save"}
                    confirmLoading={isSaving}
                    onCancel={handleCancel}
                    maskClosable={false}
                    keyboard={false} // disable esc key to close modal
                    destroyOnClose={true}
                >
                    <WrappedForm form={form} {...(props as P)} />
                </Modal>
            </>

        )
    }
    return WithModalForm
}

export default withModalForm