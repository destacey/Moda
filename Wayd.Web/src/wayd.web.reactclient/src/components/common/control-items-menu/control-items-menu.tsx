import { ControlOutlined } from '@ant-design/icons'
import { Button, Dropdown, MenuProps } from 'antd'
import { ItemType } from 'antd/es/menu/interface'

export interface ControlItemsMenuProps {
  items: MenuProps['items'] | ItemType[]
}

const ControlItemsMenu = ({ items }: ControlItemsMenuProps) => {
  if (!items || items.length === 0) return null
  return (
    <Dropdown menu={{ items: items }} trigger={['click']}>
      <Button type="text" shape="circle" icon={<ControlOutlined />} />
    </Dropdown>
  )
}

export default ControlItemsMenu
