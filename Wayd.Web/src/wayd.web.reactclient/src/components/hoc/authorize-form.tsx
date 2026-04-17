'use client'

import { ComponentType, FC, useEffect } from 'react'
import useAuth from '../contexts/auth'
import { useMessage } from '../contexts/messaging'

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
  requiredClaimType: string,
  requiredClaimValue: string,
): FC<P> => {
  // Compute the name outside the component render to avoid reassigning on each render.
  const wrappedFormName =
    WrappedForm.displayName || WrappedForm.name || 'Component'

  const AuthorizedForm: FC<P> = (props: P) => {
    const messageApi = useMessage()

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
    }, [authorized, messageApi])

    // If not authorized, do not render the form.
    if (!authorized) {
      return null
    }

    return <WrappedForm {...props} />
  }

  AuthorizedForm.displayName = `authorizedForm(${wrappedFormName})`

  return AuthorizedForm
}

/**
 * Extended props type that includes the onNotAuthorized callback
 */
export type WithAuthorizationProps<P> = P & {
  onNotAuthorized?: () => void
}

/**
 * A higher-order component that wraps a form component and checks if the user has the required claim.
 * This version accepts the onNotAuthorized callback as a prop instead of during HOC creation,
 * making it compatible with React's new compiler rules that prohibit creating components during render.
 *
 * @template P - The props of the wrapped form component.
 * @param {React.ComponentType<P>} WrappedForm - The form component to be wrapped.
 * @param {string} [requiredClaimType] - The type of claim required to access the form.
 * @param {string} [requiredClaimValue] - The value of the claim required to access the form.
 * @returns {React.FC<WithAuthorizationProps<P>>} - A new component that accepts onNotAuthorized as a prop.
 */
export const authorizeFormWithPropCallback = <P extends object>(
  WrappedForm: ComponentType<P>,
  requiredClaimType: string,
  requiredClaimValue: string,
): FC<WithAuthorizationProps<P>> => {
  const wrappedFormName =
    WrappedForm.displayName || WrappedForm.name || 'Component'

  const AuthorizedForm: FC<WithAuthorizationProps<P>> = (props) => {
    const { onNotAuthorized, ...restProps } = props
    const messageApi = useMessage()

    const { hasClaim } = useAuth()
    const authorized = hasClaim(requiredClaimType, requiredClaimValue)

    // Trigger the callback when the component mounts or when authorization changes.
    useEffect(() => {
      if (!authorized) {
        onNotAuthorized?.()
        messageApi.error(
          'You do not have the correct permissions to access this form.',
        )
      }
    }, [authorized, messageApi, onNotAuthorized])

    // If not authorized, do not render the form.
    if (!authorized) {
      return null
    }

    return <WrappedForm {...(restProps as P)} />
  }

  AuthorizedForm.displayName = `authorizedForm(${wrappedFormName})`

  return AuthorizedForm
}

export default authorizeForm
