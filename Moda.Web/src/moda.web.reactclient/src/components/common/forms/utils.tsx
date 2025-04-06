import { Form, FormItemProps } from 'antd'

const { Item } = Form

// Constrain T to a record so that keyof T is a string (or number) and compatible.
export type TypedFormItemProps<T extends Record<string, any>> = Omit<
  FormItemProps<T>,
  'name'
> & {
  name: keyof T
}

export function createTypedFormItem<T extends Record<string, any>>() {
  const TypedFormItem: React.FC<TypedFormItemProps<T>> = (props) => {
    // Cast props to FormItemProps<T> to ensure the generic is forwarded correctly.
    return <Item {...(props as FormItemProps<T>)} />
  }
  return TypedFormItem
}
