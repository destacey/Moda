import { getInitials } from './get-initials'

describe('getInitials', () => {
  it('returns first and last initials for two-word name', () => {
    expect(getInitials('John Doe')).toBe('JD')
  })

  it('returns first and last initials for multi-word name', () => {
    expect(getInitials('Mary Jane Watson')).toBe('MW')
  })

  it('returns first two chars for single-word name', () => {
    expect(getInitials('Alice')).toBe('AL')
  })

  it('handles extra whitespace', () => {
    expect(getInitials('  John   Doe  ')).toBe('JD')
  })

  it('returns uppercase', () => {
    expect(getInitials('john doe')).toBe('JD')
  })

  it('handles single character name', () => {
    expect(getInitials('A')).toBe('A')
  })
})
