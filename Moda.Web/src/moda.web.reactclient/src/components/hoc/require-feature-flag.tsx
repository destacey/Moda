'use client'

import { ComponentType, FC } from 'react'
import { notFound } from 'next/navigation'
import { useFeatureFlag } from '../../hooks'

/**
 * A higher-order component that wraps a page component and checks if the required feature flag is enabled.
 * When the flag is disabled, triggers a 404 (the feature does not exist).
 * @template P - The props of the wrapped page component.
 * @param {React.ComponentType<P>} WrappedPage - The page component to be wrapped.
 * @param {string} featureFlagName - The name of the feature flag that must be enabled.
 * @returns {React.FC<P>} - A new component that either renders the wrapped page component or triggers a 404.
 */
const requireFeatureFlag = <P extends object>(
  WrappedPage: ComponentType<P>,
  featureFlagName: string,
): FC<P> => {
  const wrappedPageName =
    WrappedPage.displayName || WrappedPage.name || 'Component'

  const RequireFeatureFlag: ComponentType<P> = ({ ...props }) => {
    const { isEnabled, isLoading } = useFeatureFlag(featureFlagName)

    if (isLoading) {
      return null
    }

    if (!isEnabled) {
      notFound()
    }

    return <WrappedPage {...(props as P)} />
  }

  RequireFeatureFlag.displayName = `requireFeatureFlag(${wrappedPageName})`

  return RequireFeatureFlag
}

export default requireFeatureFlag
