import { getDrawerWidthPercentage } from './window-utils'

describe('getDrawerWidthPercentage', () => {
  beforeEach(() => {
    // Reset the window.innerWidth before each test
    Object.defineProperty(window, 'innerWidth', {
      writable: true,
      configurable: true,
      value: 1024,
    })
  })

  it('should return 30% for window width >= 1500', () => {
    window.innerWidth = 1500
    expect(getDrawerWidthPercentage()).toBe('30%')
  })

  it('should return 35% for window width >= 1300 and < 1500', () => {
    window.innerWidth = 1300
    expect(getDrawerWidthPercentage()).toBe('35%')
  })

  it('should return 40% for window width >= 1100 and < 1300', () => {
    window.innerWidth = 1100
    expect(getDrawerWidthPercentage()).toBe('40%')
  })

  it('should return 50% for window width >= 900 and < 1100', () => {
    window.innerWidth = 900
    expect(getDrawerWidthPercentage()).toBe('50%')
  })

  it('should return 80% for window width < 900', () => {
    window.innerWidth = 800
    expect(getDrawerWidthPercentage()).toBe('80%')
  })
})
