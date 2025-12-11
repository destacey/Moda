'use client'

import { ExpenditureCategoryDetailsDto } from '@/src/services/moda-api'
import { Descriptions, Space } from 'antd'

const { Item } = Descriptions

interface ExpenditureCategoryDetailsProps {
  expenditureCategory: ExpenditureCategoryDetailsDto
}

const ExpenditureCategoryDetails: React.FC<ExpenditureCategoryDetailsProps> = ({
  expenditureCategory,
}: ExpenditureCategoryDetailsProps) => {
  if (!expenditureCategory) return null

  return (
    <Space vertical>
      <Descriptions size="small">
        <Item label="State">{expenditureCategory.state.name}</Item>
        <Item label="Capitalizable">
          {expenditureCategory.isCapitalizable?.toString()}
        </Item>
        <Item label="Requires Depreciation">
          {expenditureCategory.requiresDepreciation?.toString()}
        </Item>
        <Item label="Accounting Code">
          {expenditureCategory.accountingCode}
        </Item>
      </Descriptions>
      <Descriptions size="small">
        <Item label="Description">{expenditureCategory.description}</Item>
      </Descriptions>
    </Space>
  )
}

export default ExpenditureCategoryDetails
