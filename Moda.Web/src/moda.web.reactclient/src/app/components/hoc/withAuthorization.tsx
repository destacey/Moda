import { ComponentType, FC } from "react"
import NotAuthorized from "../common/not-authorized"
import useAuth from "../contexts/auth"

/**
 * Props for the withAuthorization higher-order component.
 * @param claimType - The type of claim to check. Default is "Permission".
 * @param claimValue - The value of the claim to check.
 * @param notAuthorizedBehavior - Component will not be rendered if set to true. Default is to render "Not Authorized"
 */
export interface WithAuthorizationProps {
  claimType?: string // The type of claim to check. Default is "Permission".
  claimValue: string // The value of the claim to check.
  notAuthorizedBehavior?: "NotAuthorized" | "DoNotRender" // Component will not be rendered if set to true. Default is to render "Not Authorized"
}

/**
 * Wrapper for pages that require authorization.
 * Will redirect to the not authorized page if the user does not have the listed claim.
 * @param WrappedComponent 
 */
const withAuthorization = <P extends object>(WrappedComponent: ComponentType<P>): FC<P & WithAuthorizationProps> => {
  const WithAuthorization: ComponentType<P & WithAuthorizationProps> = ({
    claimType, 
    claimValue, 
    notAuthorizedBehavior: notAuthroizedBehavior,
    ...props
  }) => {

    const { hasClaim } = useAuth()
    const wrappedComponentName = WrappedComponent.displayName 
      || WrappedComponent.name 
      || 'Component'
    
    WithAuthorization.displayName = `withAuthorization(${wrappedComponentName})`
    
    return (
      hasClaim(claimType ?? "Permission", claimValue)
      ? <WrappedComponent {...props as P} />
      : notAuthroizedBehavior === "DoNotRender"
        ? <></> 
        : <NotAuthorized/>
    )
  }
  return WithAuthorization
}

export default withAuthorization