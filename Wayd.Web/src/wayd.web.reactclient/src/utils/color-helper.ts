import { GlobalToken } from 'antd'
import { LifecyclePhase } from '../components/types'
import { LifecycleNavigationDto } from '../services/wayd-api'

/**
 * Determines the ant design status color string based on the status of a work status category.
 *
 * @param {string} statusCategory
 * @returns {string} returns an ant design status color string
 */
export const getWorkStatusCategoryColor = (statusCategory: string): string => {
  switch (statusCategory) {
    case 'Active':
      return 'processing'
    case 'Done':
      return 'success'
    case 'Removed':
      return 'error'
    case 'Proposed':
    default:
      return 'default'
  }
}

/**
 * Determines the ant design status color string based on the status of an objective.
 *
 * @param {string} status
 * @returns {string} returns an ant design status color string
 */
export const getObjectiveStatusColor = (status: string): string => {
  switch (status) {
    case 'In Progress':
      return 'processing'
    case 'Completed':
      return 'success'
    case 'Canceled':
    case 'Missed':
      return 'error'
    default:
      return 'default'
  }
}

/**
 * Determines the relative luminance of a given hex color.
 *
 * @param {string} hexColor - The hex color code in the format #RRGGBB.
 * @returns {number} - Returns the relative luminance of the color.
 * @throws {Error} - Throws an error if the hex color format is invalid.
 */
export const getLuminance = (hexColor: string): number => {
  // verify hex color value string format
  if (!/^#[0-9A-F]{6}$/i.test(hexColor)) {
    throw new Error('Invalid hex color format')
  }

  const hex = hexColor.replace('#', '')
  const r = parseInt(hex.slice(0, 2), 16)
  const g = parseInt(hex.slice(2, 4), 16)
  const b = parseInt(hex.slice(4, 6), 16)

  // return relative luminance
  return (0.299 * r + 0.587 * g + 0.114 * b) / 255
}

/**
 * Determines whether a given hex color is considered 'light' or 'dark' based on its luminance.
 *
 * @param {string} hexColor - The hex color code in the format #RRGGBB.
 * @returns {string} - Returns 'light' if the luminance is greater than or equal to 0.5, otherwise 'dark'.
 * @throws {Error} - Throws an error if the hex color format is invalid.
 */
export const getLuminanceTheme = (hexColor: string): string => {
  // returns 'dark' or 'light' based on the luminance of the color
  return getLuminance(hexColor) >= 0.5 ? 'light' : 'dark'
}

export const getLifecyclePhaseColor = (
  phase: LifecyclePhase,
  token: GlobalToken, // GlobalToken from antd theme
): string | undefined => {
  switch (phase) {
    case LifecyclePhase.Active:
      return token.colorPrimary // or token.colorPrimary
    case LifecyclePhase.Done:
      return token.colorSuccess
    default:
      return undefined
  }
}

export const getLifecyclePhaseColorFromStatus = (
  status: LifecycleNavigationDto,
  token: any,
): string | undefined => {
  // Map string to enum
  const phase =
    LifecyclePhase[status.lifecyclePhase as keyof typeof LifecyclePhase]
  return getLifecyclePhaseColor(phase, token)
}

const avatarColors = [
  '#1677ff', '#722ed1', '#13c2c2', '#eb2f96', '#fa8c16',
  '#52c41a', '#2f54eb', '#faad14', '#f5222d', '#a0d911',
]

/**
 * Returns a deterministic color from a fixed palette based on the given string.
 * Useful for assigning consistent avatar colors to users by ID or name.
 *
 * @param {string} value - A string to hash (e.g., user ID or name).
 * @returns {string} A hex color string from the palette.
 */
export const getAvatarColor = (value: string): string => {
  let hash = 0
  for (let i = 0; i < value.length; i++) {
    hash = (hash * 31 + value.charCodeAt(i)) | 0
  }
  return avatarColors[Math.abs(hash) % avatarColors.length]
}

export const getLifecyclePhaseTagColor = (
  phase: LifecyclePhase,
): string | undefined => {
  switch (phase) {
    case LifecyclePhase.Active:
      return 'processing'
    case LifecyclePhase.Done:
      return 'success'
    default:
      return 'default'
  }
}

export const getSemanticChartColor = (
  semanticColor: string,
  token: Pick<
    GlobalToken,
    | 'colorInfo'
    | 'colorSuccess'
    | 'colorError'
    | 'colorWarning'
    | 'colorTextQuaternary'
  >,
): string => {
  switch (semanticColor) {
    case 'processing':
      return token.colorInfo
    case 'success':
      return token.colorSuccess
    case 'error':
      return token.colorError
    case 'warning':
      return token.colorWarning
    case 'default':
    default:
      return token.colorTextQuaternary
  }
}

const clamp = (value: number, min: number, max: number) =>
  Math.min(Math.max(value, min), max)

const parseColor = (color: string): [number, number, number] | null => {
  const trimmed = color.trim()

  const hex = trimmed.replace('#', '')
  if (/^[0-9a-fA-F]{6}$/.test(hex)) {
    return [
      Number.parseInt(hex.slice(0, 2), 16),
      Number.parseInt(hex.slice(2, 4), 16),
      Number.parseInt(hex.slice(4, 6), 16),
    ]
  }

  if (/^[0-9a-fA-F]{3}$/.test(hex)) {
    return [
      Number.parseInt(hex[0] + hex[0], 16),
      Number.parseInt(hex[1] + hex[1], 16),
      Number.parseInt(hex[2] + hex[2], 16),
    ]
  }

  const rgbMatch = trimmed.match(
    /^rgba?\(\s*(\d{1,3})\s*[, ]\s*(\d{1,3})\s*[, ]\s*(\d{1,3})/i,
  )
  if (rgbMatch) {
    return [
      clamp(Number.parseInt(rgbMatch[1], 10), 0, 255),
      clamp(Number.parseInt(rgbMatch[2], 10), 0, 255),
      clamp(Number.parseInt(rgbMatch[3], 10), 0, 255),
    ]
  }

  return null
}

export const softenChartColor = (
  baseColor: string,
  backgroundColor: string,
  softenBy = 0.35,
): string => {
  const base = parseColor(baseColor)
  const background = parseColor(backgroundColor)

  if (!base || !background) return baseColor

  const t = clamp(softenBy, 0, 1)
  const mixed = base.map((channel, index) =>
    Math.round(channel * (1 - t) + background[index] * t),
  )

  return `rgb(${mixed[0]}, ${mixed[1]}, ${mixed[2]})`
}
