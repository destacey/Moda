import toFormErrors from './problem-details'

describe('toFormErrors', () => {
  it('should return correct form errors', () => {
    const problemDetails = {
      Name: ['Name is required'],
      Email: ['Email is required', 'Email is not valid'],
    }

    const result = toFormErrors(problemDetails)

    expect(result).toEqual([
      { name: 'name', errors: ['Name is required'] },
      { name: 'email', errors: ['Email is required', 'Email is not valid'] },
    ])
  })

  it('should handle empty problem details', () => {
    const problemDetails = {}

    const result = toFormErrors(problemDetails)

    expect(result).toEqual([])
  })

  it('should handle null problem details', () => {
    const problemDetails = null

    const result = toFormErrors(problemDetails)

    expect(result).toEqual([])
  })

  it('should handle undefined problem details', () => {
    const problemDetails = undefined

    const result = toFormErrors(problemDetails)

    expect(result).toEqual([])
  })
})
