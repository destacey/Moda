import toFormErrors from './problem-details'

describe('toFormErrors', () => {
  it('should return correct form errors', () => {
    const problemDetails = {
      Name: ['Name is required'],
      Email: ['Email is required', 'Email is not valid'],
    }

    const result = toFormErrors(problemDetails)

    expect(result).toEqual(
      expect.arrayContaining([
        { name: 'Name', errors: ['Name is required'] },
        { name: 'name', errors: ['Name is required'] },
        {
          name: 'Email',
          errors: ['Email is required', 'Email is not valid'],
        },
        {
          name: 'email',
          errors: ['Email is required', 'Email is not valid'],
        },
      ]),
    )
  })

  it('should map dotted and jsonpath keys', () => {
    const problemDetails = {
      'request.Key': ['Key must be unique'],
      '$.key': ['Key must be 2-20 uppercase alphanumeric characters'],
    }

    const result = toFormErrors(problemDetails)

    expect(result).toEqual(
      expect.arrayContaining([
        { name: 'request.Key', errors: ['Key must be unique'] },
        { name: 'request.key', errors: ['Key must be unique'] },
        { name: 'Key', errors: ['Key must be unique'] },
        {
          name: 'key',
          errors: [
            'Key must be unique',
            'Key must be 2-20 uppercase alphanumeric characters',
          ],
        },
        {
          name: '$.key',
          errors: ['Key must be 2-20 uppercase alphanumeric characters'],
        },
      ]),
    )
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
