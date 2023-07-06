import { FieldData } from 'rc-field-form/lib/interface'

const toFormErrors = (problemDetails: {[property: string]: string[]}): FieldData[] => {
  const formErrors: FieldData[] = Object.keys(problemDetails).map((key) => {
    // Make the first letter of the key lowercase
    const name = key.charAt(0).toLowerCase() + key.slice(1)
    return { name, errors: [...problemDetails[key]] }
  })

  return formErrors
}

export default toFormErrors