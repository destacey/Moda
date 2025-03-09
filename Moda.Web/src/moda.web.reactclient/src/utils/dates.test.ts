import { daysRemaining } from './dates'

describe('dates', () => {
  it('should return the number of days remaining for a future date', () => {
    const futureDate = new Date()
    futureDate.setDate(futureDate.getDate() + 5) // 5 days from now
    expect(daysRemaining(futureDate)).toEqual(5)
  })

  it('should return a negative number for a past date', () => {
    const pastDate = new Date()
    pastDate.setDate(pastDate.getDate() - 3) // 3 days ago
    expect(daysRemaining(pastDate)).toEqual(-3)
  })

  it('should return 0 for today', () => {
    const today = new Date()
    expect(daysRemaining(today)).toEqual(0)
  })

  it('should return 1 for tomorrow', () => {
    const tomorrow = new Date()
    tomorrow.setDate(tomorrow.getDate() + 1) // tomorrow
    // the +0 is to make sure the result is not -0
    expect(daysRemaining(tomorrow) + 0).toEqual(1)
  })

  it('should handle end of month correctly', () => {
    const endOfMonth = new Date()
    endOfMonth.setMonth(endOfMonth.getMonth() + 1)
    endOfMonth.setDate(0) // last day of the previous month
    const today = new Date()
    const daysDiff = Math.ceil(
      (endOfMonth.getTime() - today.getTime()) / (1000 * 3600 * 24),
    )
    expect(daysRemaining(endOfMonth)).toEqual(daysDiff)
  })

  it('should handle leap years correctly', () => {
    const leapYearDate = new Date('2024-02-29T00:00:00Z')
    const today = new Date('2024-02-28T00:00:00Z')
    expect(daysRemaining(leapYearDate, today)).toEqual(1)
  })
})
