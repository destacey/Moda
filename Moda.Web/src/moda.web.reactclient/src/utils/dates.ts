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
