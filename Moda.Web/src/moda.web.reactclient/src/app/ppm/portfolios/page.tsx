'use client'

import { authorizePage } from '@/src/components/hoc'
import { useDocumentTitle } from '@/src/hooks'
import { message } from 'antd'

const PortfoliosPage: React.FC = () => {
  useDocumentTitle('Portfolios')

  const [messageApi, contextHolder] = message.useMessage()

  return <div>Portfolios</div>
}

const PortfoliosPageWithAuthorization = authorizePage(
  PortfoliosPage,
  'Permission',
  'Permissions.ProjectPortfolios.View',
)

export default PortfoliosPageWithAuthorization
