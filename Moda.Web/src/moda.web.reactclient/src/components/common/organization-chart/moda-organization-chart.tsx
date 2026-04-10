import { FC, ComponentType, useRef } from 'react'
import useTheme from '../../contexts/theme'
import { OrganizationChart, OrganizationChartOptions } from '@ant-design/graphs'
import {
  ModaOrganizationChartNodeProps,
  OrganizationChartGraphData,
} from './types'
import ModaEmpty from '../moda-empty'

interface CustomTransformOption {
  type: string
  key?: string
  enable?: boolean
  iconPlacement?: 'bottom' | 'right'
  refreshLayout?: boolean
  [key: string]: any
}
type TransformOption = CustomTransformOption | string

export interface ModaOrganizationChartProps<T = any> {
  data: OrganizationChartGraphData<T>
  NodeComponent: ComponentType<ModaOrganizationChartNodeProps>
  nodeSize?: [number, number]
}

const ModaOrganizationChart: FC<ModaOrganizationChartProps> = ({
  data,
  NodeComponent,
  nodeSize = [250, 80],
}) => {
  const chartInstanceRef = useRef<any>(null)

  const { antvisG6ChartsTheme, token } = useTheme()

  const options: OrganizationChartOptions = {
    padding: [40, 0, 0, 120],
    autoFit: 'view',
    data: (data as Record<string, unknown>) || { nodes: [], edges: [] },
    node: {
      style: {
        component: (nodeData) => (
          <NodeComponent data={nodeData.data} themeToken={token} />
        ),
        size: nodeSize,
        ports: [
          { key: '0', placement: [0.5, 0] },
          { key: '1', placement: [0.5, 1] },
        ],
      },
    },
    onReady: (graph) => {
      chartInstanceRef.current = graph
    },
    onDestroy: () => {
      chartInstanceRef.current = null
    },
    edge: {
      type: 'cubic-vertical',
      style: {
        radius: 0,
        lineWidth: 1.5,
        endArrow: true,
        startArrow: false,
        stroke: '#91d5ff',
        sourcePort: '1',
        targetPort: '0',
      },
    },
    layout: {
      type: 'dagre',
      nodesep: 24,
      ranksep: 48,
      rankdir: 'TB',
      controlPoints: false,
    },
    transforms: (transforms: TransformOption[]) =>
      transforms.filter(
        (transform) =>
          typeof transform === 'string' ||
          transform.type !== 'collapse-expand-react-node',
      ),
  }

  if (!data || !data.nodes || data.nodes.length === 0) {
    return <ModaEmpty message="No org chart data to display" />
  }

  return (
    <>
      <div style={{ width: '100%', height: '80vh' }}>
        <OrganizationChart {...options} theme={antvisG6ChartsTheme} />
      </div>
    </>
  )
}

export default ModaOrganizationChart
