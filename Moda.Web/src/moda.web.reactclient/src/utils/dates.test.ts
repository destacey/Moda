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
    expect(daysRemaining(tomorrow)).toEqual(1)
  })
})
