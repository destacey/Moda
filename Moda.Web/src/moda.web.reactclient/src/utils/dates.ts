export function programIncrementDaysRemaining(endDate: Date): number {
  const end = new Date(endDate)
  const now = new Date()
  return Math.ceil((end.getTime() - now.getTime()) / (1000 * 3600 * 24))
}
