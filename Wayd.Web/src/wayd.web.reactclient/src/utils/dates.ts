/**
 * Calculates the number of days remaining from a reference date (defaults to today)
 * to the given end date.
 *
 * The function normalizes both dates to midnight in UTC to avoid issues related to
 * Daylight Saving Time (DST) and time differences.
 *
 * @param {Date} endDate - The end date to calculate the remaining days for.
 * @param {Date} [referenceDate] - Optional reference date used instead of the current date.
 * @returns {number} - The number of days remaining. A positive number indicates a future date,
 *                     zero indicates the same day, and a negative number indicates a past date.
 */
export function daysRemaining(endDate: Date, referenceDate?: Date): number {
  const end = new Date(endDate)
  const now = referenceDate ? new Date(referenceDate) : new Date()

  // Normalize both dates to midnight in UTC
  end.setUTCHours(0, 0, 0, 0)
  now.setUTCHours(0, 0, 0, 0)

  const timeDiff = end.getTime() - now.getTime()

  return Math.ceil(timeDiff / (1000 * 3600 * 24))
}

/**
 * Calculates the percentage of days elapsed between a start date and end date.
 *
 * This function normalizes dates to midnight UTC (like daysRemaining) to calculate
 * the percentage based on full days rather than precise milliseconds.
 *
 * @param {Date} startDate - The start date of the period.
 * @param {Date} endDate - The end date of the period.
 * @param {Date} [referenceDate] - Optional reference date used instead of the current date.
 * @returns {number} - The percentage elapsed (0-100). Returns 0 if the period hasn't started,
 *                     capped at 100 if past the end date.
 */
export function percentageElapsed(
  startDate: Date,
  endDate: Date,
  referenceDate?: Date,
): number {
  const start = new Date(startDate)
  const end = new Date(endDate)
  const now = referenceDate ? new Date(referenceDate) : new Date()

  // Normalize all dates to midnight UTC (consistent with daysRemaining)
  start.setUTCHours(0, 0, 0, 0)
  end.setUTCHours(0, 0, 0, 0)
  now.setUTCHours(0, 0, 0, 0)

  const totalDays = Math.ceil((end.getTime() - start.getTime()) / (1000 * 3600 * 24))
  const elapsedDays = Math.max(
    0,
    Math.ceil((now.getTime() - start.getTime()) / (1000 * 3600 * 24)),
  )

  if (totalDays <= 0) return 0

  const percentage = (elapsedDays / totalDays) * 100
  return Math.min(100, percentage)
}
