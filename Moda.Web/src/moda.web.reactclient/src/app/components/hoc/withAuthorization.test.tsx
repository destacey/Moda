import { render, screen } from "@testing-library/react"
import '@testing-library/jest-dom'
import auth from "@/src/services/auth";
import withAuthorization, { WithAuthorizationProps } from "./withAuthorization"

jest.mock("../../../services/auth")
const mockedAuth = auth as jest.Mocked<typeof auth>

describe("withAuthorization", () => {
  const MockComponent = () => <div>Authorized</div>;

  const mockProps: WithAuthorizationProps = {
    claimValue: "testClaim",
  };

  it("renders the wrapped component if the user has the required claim", () => {

    mockedAuth.hasClaim.mockReturnValue(true)

    const WrappedComponent = withAuthorization(MockComponent);
    const { getByText } = render(<WrappedComponent {...mockProps} />);

    expect(getByText("Authorized")).toBeInTheDocument();
  });

  it("renders the NotAuthorized component if the user does not have the required claim", () => {
    mockedAuth.hasClaim.mockReturnValue(false)  

    const WrappedComponent = withAuthorization(MockComponent);
    const { getByText } = render(<WrappedComponent {...mockProps} />);

    expect(getByText("Not Authorized")).toBeInTheDocument();
  });

  it("does not render a component if the user does not have the required claim", () => {
    mockedAuth.hasClaim.mockReturnValue(false)  

    const WrappedComponent = withAuthorization(MockComponent);
    const { queryByText } = render(<WrappedComponent {...mockProps} doNotRenderOnNotAuthorized={true} />);

    expect(queryByText("Authorized", {exact: false})).toBeNull();
  });
});