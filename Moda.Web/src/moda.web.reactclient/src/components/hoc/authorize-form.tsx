'use client'

import { ComponentType, FC, useEffect } from 'react'
import useAuth from '../contexts/auth'
import { MessageInstance } from 'antd/es/message/interface'

/**
 * A higher-order component that wraps a form component and checks if the user has the required claim.
 * If not authorized, it calls the provided onNotAuthorized callback and does not render the form.
 *
 * @template P - The props of the wrapped form component.
 * @param {React.ComponentType<P>} WrappedForm - The form component to be wrapped.
 * @param {() => void} onNotAuthorized - Callback to be called when the user is not authorized.
 * @param {string} [requiredClaimType] - The type of claim required to access the form.
 * @param {string} [requiredClaimValue] - The value of the claim required to access the form.
 * @returns {React.FC<P>} - A new component that either renders the form or calls the callback and renders nothing.
 */
const authorizeForm = <P extends object>(
  WrappedForm: ComponentType<P>,
  onNotAuthorized: () => void,
  messageApi: MessageInstance,
  requiredClaimType: string,
  requiredClaimValue: string,
): FC<P> => {
  // Compute the name outside the component render to avoid reassigning on each render.
  const wrappedFormName =
    WrappedForm.displayName || WrappedForm.name || 'Component'

  const AuthorizedForm: FC<P> = (props: P) => {
    const { hasClaim } = useAuth()
    const authorized = hasClaim(requiredClaimType, requiredClaimValue)

    // Trigger the callback when the component mounts or when authorization changes.
    useEffect(() => {
      if (!authorized) {
        onNotAuthorized()
        messageApi.error(
          'You do not have the correct permissions to access this form.',
        )
      }
    }, [authorized])

    // If not authorized, do not render the form.
    if (!authorized) {
      return null
    }

    return <WrappedForm {...props} />
  }

  AuthorizedForm.displayName = `authorizedForm(${wrappedFormName})`

  return AuthorizedForm
}

export default authorizeForm
