import { ComponentType, useMemo, useRef } from 'react'
import useTheme from '../../contexts/theme'
import { OrganizationChart, OrganizationChartOptions } from '@ant-design/graphs'
import {
  ModaOrganizationChartNodeProps,
  OrganizationChartGraphData,
} from './types'
import ModaEmpty from '../moda-empty'

interface CustomTransformOption {
  type: string
  enable?: boolean
  iconOffsetY?: number
  [key: string]: any
}

export interface ModaOrganizationChartProps<T = any> {
  data: OrganizationChartGraphData<T>
  NodeComponent: ComponentType<ModaOrganizationChartNodeProps>
  nodeSize?: [number, number]
}

const ModaOrganizationChart: React.FC<ModaOrganizationChartProps> = ({
  data,
  NodeComponent,
  nodeSize = [250, 80],
}) => {
  const chartInstanceRef = useRef<any>(null)

  const { antvisG6ChartsTheme, token } = useTheme()

  const options: OrganizationChartOptions = useMemo(
    () => ({
      padding: [40, 0, 0, 120],
      autoFit: 'view',
      data: (data as Record<string, unknown>) || { nodes: [], edges: [] },
      node: {
        style: {
          component: (nodeData) => (
            <NodeComponent data={nodeData.data} themeToken={token} />
          ),
          size: nodeSize,
        },
      },
      onReady: (graph) => {
        chartInstanceRef.current = graph
      },
      onDestroy: () => {
        chartInstanceRef.current = null
      },
      edge: {
        style: {
          radius: 0,
          lineWidth: 2,
          endArrow: true,
          startArrow: false,
          stroke: '#91d5ff',
        },
      },
      layout: {
        type: 'antv-dagre',
        nodesep: 24,
        ranksep: 0,
        rankdir: 'TB',
        controlPoints: true,
      },
      transforms: (transforms: CustomTransformOption[]) => {
        const filteredTransforms = transforms.filter(
          (transform) => transform.type !== 'collapse-expand-react-node',
        )

        const collapseExpandTransform = transforms.find(
          (transform) => transform.type === 'collapse-expand-react-node',
        )

        return [
          ...filteredTransforms,
          {
            ...(collapseExpandTransform || {}),
            type: 'collapse-expand-react-node',
            enable: true,
            iconOffsetY: 24,
          },
        ]
      },
    }),
    [NodeComponent, data, nodeSize, token],
  )

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
