'use client'

import { Tag } from 'antd'
import useTheme from '../contexts/theme'

interface InactiveTagProps {
  isActive: boolean
}

const InactiveTag: React.FC<InactiveTagProps> = ({ isActive }) => {
  const { token } = useTheme()

  if (isActive === undefined || isActive === null || isActive) {
    return null
  }

  return <Tag color={token.colorWarning}>Inactive</Tag>
}

export default InactiveTag
