import { Space, Switch } from 'antd'
import { SwitchChangeEventHandler, SwitchProps } from 'antd/es/switch'

export interface ControlItemSwitchProps extends SwitchProps {
  label: string
  checked: boolean
  onChange: SwitchChangeEventHandler
}

const ControlItemSwitch = (props: ControlItemSwitchProps) => {
  const { label, checked, onChange, ...rest } = props
  return (
    <Space>
      <Switch size="small" checked={checked} onChange={onChange} {...rest} />
      {label}
    </Space>
  )
}

export default ControlItemSwitch
