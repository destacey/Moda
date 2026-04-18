import { Empty } from 'antd'

export interface ModaEmptyProps {
  message?: string
}

const WaydEmpty = ({ message }: ModaEmptyProps) => {
  return <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description={message} />
}

export default WaydEmpty
