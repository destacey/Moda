import { getSortedNames } from './get-sorted-names'

describe('getSortedNames', () => {
  it('should return an empty string for an empty array', () => {
    const result = getSortedNames([])
    expect(result).toBe('')
  })

  it('should return the name for an array with one object', () => {
    const result = getSortedNames([{ name: 'Alice' }])
    expect(result).toBe('Alice')
  })

  it('should return sorted names for an array with multiple objects', () => {
    const result = getSortedNames([
      { name: 'Charlie' },
      { name: 'Alice' },
      { name: 'Bob' },
    ])
    expect(result).toBe('Alice, Bob, Charlie')
  })

  it('should handle objects with the same name', () => {
    const result = getSortedNames([
      { name: 'Alice' },
      { name: 'Bob' },
      { name: 'Alice' },
    ])
    expect(result).toBe('Alice, Alice, Bob')
  })

  it('should handle case sensitivity correctly', () => {
    const result = getSortedNames([
      { name: 'Alice' },
      { name: 'bob' },
      { name: 'alice' },
    ])
    expect(result).toBe('alice, Alice, bob')
  })
})
