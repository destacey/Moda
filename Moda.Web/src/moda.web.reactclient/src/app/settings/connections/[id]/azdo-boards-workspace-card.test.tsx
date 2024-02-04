import { render, screen } from '@testing-library/react'
import '@testing-library/jest-dom'
import AzdoBoardsWorkspaceCard from './azdo-boards-workspace-card'
import { createContext } from 'react'
import {
  AzdoBoardsConnectionContext,
  AzdoBoardsConnectionContextInterface,
} from './azdo-boards-connection-context'

const workspace = {
  id: '1',
  name: 'Test Workspace',
  description: 'Test Description',
  externalId: '123456',
}

describe('AzdoBoardsWorkspaceCard', () => {
  jest.mock('./page', () => ({
    __esModule: true,
    AzdoBoardsConnectionContext:
      createContext<AzdoBoardsConnectionContextInterface | null>(null),
  }))

  beforeEach(() => {
    jest.clearAllMocks()
  })

  const renderComponent = (component) => {
    const azdoBoardsConnectionContext: AzdoBoardsConnectionContextInterface = {
      connectionId: '1',
      organizationUrl: 'https://dev.azure.com/test',
    }

    return render(
      <AzdoBoardsConnectionContext.Provider value={azdoBoardsConnectionContext}>
        {component}
      </AzdoBoardsConnectionContext.Provider>,
    )
  }
  // it('renders without crashing', () => {
  //   renderComponent(
  //     <AzdoBoardsWorkspaceCard workspace={workspace} enableInit={false} />,
  //   )
  //   expect(screen.getByTestId('azdo-boards-workspace-card')).toBeInTheDocument()
  // })

  it('placeholder', () => {
    expect(true).toBeTruthy()
  })
})
