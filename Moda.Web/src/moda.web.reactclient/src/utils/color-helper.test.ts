import { getLuminance, getLuminanceTheme } from './color-helper'

describe('getLuminance', () => {
  test('should return 1 for a light color', () => {
    expect(getLuminance('#FFFFFF')).toBeCloseTo(1, 5)
  })

  test('should return 0 for a dark color', () => {
    expect(getLuminance('#000000')).toBeCloseTo(0, 5)
  })

  test('should return approximately 0.5 for a mid-tone color', () => {
    expect(getLuminance('#808080')).toBeCloseTo(0.5)
  })

  test('should handle invalid color format gracefully', () => {
    expect(() => getLuminance('invalid')).toThrow('Invalid hex color format')
  })
})

describe('getLuminanceTheme', () => {
  test('should return "light" for a light color', () => {
    expect(getLuminanceTheme('#FFFFFF')).toBe('light')
  })

  test('should return "dark" for a dark color', () => {
    expect(getLuminanceTheme('#000000')).toBe('dark')
  })

  test('should return "light" for a mid-tone color', () => {
    expect(getLuminanceTheme('#808080')).toBe('light')
  })

  test('should handle invalid color format gracefully', () => {
    expect(() => getLuminanceTheme('invalid')).toThrow(
      'Invalid hex color format',
    )
  })
})
