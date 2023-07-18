import { FileTextOutlined, GroupOutlined } from '@ant-design/icons'
import { Segmented } from 'antd'

export interface ProgramIncrementViewSelectorProps {
  startingView: 'details' | 'plan-review'
  programIncrementLocalId: number
}

const ProgramIncrementViewSelector = ({
  startingView,
  programIncrementLocalId,
}: ProgramIncrementViewSelectorProps) => {
  const changeViews = (value: string | number) => {
    if (value === 'details') {
      window.location.href = `/planning/program-increments/${programIncrementLocalId}`
    } else if (value === 'plan-review') {
      window.location.href = `/planning/program-increments/${programIncrementLocalId}/plan-review`
    }
  }

  if (!programIncrementLocalId) return null

  return (
    <>
      <Segmented
        defaultValue={startingView}
        options={[
          {
            value: 'details',
            icon: <FileTextOutlined title="PI Details" />,
          },
          {
            value: 'plan-review',
            icon: <GroupOutlined title="PI Plan Review" />,
          },
        ]}
        onChange={(value) => changeViews(value)}
      />
    </>
  )
}

export default ProgramIncrementViewSelector
