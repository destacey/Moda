import { GlobalToken } from 'antd'

type ID = string
type State = string

// Define base style that matches G6/antd-graphs expectations
export interface BaseStyle {
  [key: string]: any
}

export interface OrganizationChartNodeData<T = Record<string, any>> {
  id: ID
  type?: string
  data?: T
  style?: BaseStyle
  states?: State[]
  combo?: ID | null
  children?: ID[]
  depth?: number
}

export interface OrganizationChartEdgeData<T = Record<string, any>> {
  id?: ID
  source: ID
  target: ID
  sourceNode?: ID
  targetNode?: ID
  type?: string
  data?: T
  style?: BaseStyle
  states?: State[]
}

export interface OrganizationChartComboData<T = Record<string, any>> {
  id: ID
  type?: string
  data?: T
  style?: BaseStyle
  states?: State[]
  combo?: ID | null
}

export interface OrganizationChartGraphData<
  TNode = Record<string, any>,
  TEdge = Record<string, any>,
  TCombo = Record<string, any>,
> {
  nodes?: OrganizationChartNodeData<TNode>[]
  edges?: OrganizationChartEdgeData<TEdge>[]
  combos?: OrganizationChartComboData<TCombo>[]
}

export interface ModaOrganizationChartNodeProps {
  data: any
  themeToken: GlobalToken
}
