/**
 * Represents the health status of an iteration (sprint or PI iteration).
 */
export enum IterationHealthStatus {
  OnTrack = 'On Track',
  AtRisk = 'At Risk',
  OffTrack = 'Off Track',
  Completed = 'Completed',
  NotStarted = 'Not Started',
  Unknown = 'Unknown',
}

/**
 * Result of the iteration health calculation.
 */
export interface IterationHealthResult {
  status: IterationHealthStatus
  variancePercent: number
}

/**
 * Parameters for calculating iteration health.
 */
export interface IterationHealthParams {
  /** Start date of the iteration */
  startDate: Date
  /** End date of the iteration */
  endDate: Date
  /** Total planned points/items */
  total: number
  /** Completed points/items */
  completed: number
  /** Optional reference date (defaults to now) */
  referenceDate?: Date
}

/**
 * Calculates the health status of an iteration based on burndown progress.
 *
 * The calculation compares actual progress against ideal linear burndown:
 * - On Track: Within 10% of ideal burndown
 * - At Risk: 10-25% behind ideal burndown
 * - Off Track: More than 25% behind ideal burndown
 *
 * @param params - The iteration health parameters
 * @returns The health result with status and variance percentage
 *
 * @example
 * // Sprint: 40 SP, 14 days, Day 10, 24 SP done
 * const result = calculateIterationHealth({
 *   startDate: new Date('2024-01-01'),
 *   endDate: new Date('2024-01-14'),
 *   total: 40,
 *   completed: 24,
 *   referenceDate: new Date('2024-01-10'),
 * })
 * // result.status === IterationHealthStatus.AtRisk
 * // result.variancePercent === 11.5 (behind)
 */
export function calculateIterationHealth(
  params: IterationHealthParams,
): IterationHealthResult {
  const { startDate, endDate, total, completed, referenceDate } = params

  const start = new Date(startDate)
  const end = new Date(endDate)
  const now = referenceDate ? new Date(referenceDate) : new Date()

  // Normalize dates to midnight UTC
  start.setUTCHours(0, 0, 0, 0)
  end.setUTCHours(0, 0, 0, 0)
  now.setUTCHours(0, 0, 0, 0)

  const elapsedMs = now.getTime() - start.getTime()

  // Handle iteration not yet started
  if (elapsedMs <= 0) {
    return { status: IterationHealthStatus.NotStarted, variancePercent: 0 }
  }

  // Handle completed iteration (end date has passed)
  if (now.getTime() > end.getTime()) {
    return { status: IterationHealthStatus.Completed, variancePercent: 0 }
  }

  // Handle no work planned
  if (total <= 0) {
    return { status: IterationHealthStatus.Unknown, variancePercent: 0 }
  }

  const totalMs = end.getTime() - start.getTime()

  const totalDays = totalMs / (1000 * 3600 * 24)
  const daysElapsed = Math.min(elapsedMs / (1000 * 3600 * 24), totalDays)
  const daysRemaining = Math.max(0, totalDays - daysElapsed)

  // Where should we be? (ideal linear burndown)
  // At day 0, idealRemaining = total
  // At end, idealRemaining = 0
  const idealRemaining = total * (daysRemaining / totalDays)

  // Where are we actually?
  const actualRemaining = total - completed

  // How far off are we? (positive = behind, negative = ahead)
  const variance = actualRemaining - idealRemaining
  const variancePercent = (variance / total) * 100

  // Determine health status based on thresholds
  if (variancePercent <= 10) {
    return { status: IterationHealthStatus.OnTrack, variancePercent }
  } else if (variancePercent <= 25) {
    return { status: IterationHealthStatus.AtRisk, variancePercent }
  } else {
    return { status: IterationHealthStatus.OffTrack, variancePercent }
  }
}
