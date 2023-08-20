'use client'

import { ComponentType, FC } from 'react'
import NotAuthorized from '../common/not-authorized'
import useAuth from '../contexts/auth'

/**
 * A higher-order component that wraps a page component and checks if the user has the required claim to access the page.
 * @template P - The props of the wrapped page component.
 * @param {React.ComponentType<P>} WrappedPage - The page component to be wrapped.
 * @param {string} [requiredClaimType] - The type of claim required to access the page. Defaults to "Permission".
 * @param {string} [requiredClaimValue] - The value of the claim required to access the page.
 * @returns {React.FC<P>} - A new component that either renders the wrapped page component or a "NotAuthorized" component based on the user's claim.
 */
const authorizePage = <P extends object>(
  WrappedPage: ComponentType<P>,
  requiredClaimType?: string,
  requiredClaimValue?: string,
): FC<P> => {
  const AuthorizePage: ComponentType<P> = ({ ...props }) => {
    const { hasClaim } = useAuth()
    const wrappedPageName =
      WrappedPage.displayName || WrappedPage.name || 'Component'

    AuthorizePage.displayName = `authorizedPage(${wrappedPageName})`

    return hasClaim(requiredClaimType ?? 'Permission', requiredClaimValue) ? (
      <WrappedPage {...(props as P)} />
    ) : (
      <NotAuthorized />
    )
  }
  return AuthorizePage
}

export default authorizePage
