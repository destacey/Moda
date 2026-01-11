import { GlobalToken } from 'antd'
import { LifecyclePhase } from '../components/types'
import { LifecycleNavigationDto } from '../services/moda-api'

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
