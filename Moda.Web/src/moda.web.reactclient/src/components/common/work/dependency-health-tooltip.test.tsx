import { render, screen } from '@testing-library/react'
import userEvent from '@testing-library/user-event'
import DependencyHealthTooltip from './dependency-health-tooltip'
import { DependencyHealth } from '../../types'

describe('DependencyHealthTooltip', () => {
  describe('tooltip content', () => {
    it('should show Healthy description on hover', async () => {
      const user = userEvent.setup()
      render(
        <DependencyHealthTooltip health={DependencyHealth.Healthy}>
          <span>Test Content</span>
        </DependencyHealthTooltip>,
      )

      const content = screen.getByText('Test Content')
      await user.hover(content)

      const tooltip = await screen.findByText(
        'Either the predecessor is done, or is planned to complete on or before the successor.',
      )
      expect(tooltip).toBeInTheDocument()
    })

    it('should show AtRisk description on hover', async () => {
      const user = userEvent.setup()
      render(
        <DependencyHealthTooltip health={DependencyHealth.AtRisk}>
          <span>Test Content</span>
        </DependencyHealthTooltip>,
      )

      const content = screen.getByText('Test Content')
      await user.hover(content)

      const tooltip = await screen.findByText(
        'Neither the predecessor nor successor have future planned dates.',
      )
      expect(tooltip).toBeInTheDocument()
    })

    it('should show Unhealthy description on hover', async () => {
      const user = userEvent.setup()
      render(
        <DependencyHealthTooltip health={DependencyHealth.Unhealthy}>
          <span>Test Content</span>
        </DependencyHealthTooltip>,
      )

      const content = screen.getByText('Test Content')
      await user.hover(content)

      const tooltip = await screen.findByText(
        'Either the predecessor was removed, or is planned to complete after the successor needs it.',
      )
      expect(tooltip).toBeInTheDocument()
    })

    it('should show Unknown description on hover', async () => {
      const user = userEvent.setup()
      render(
        <DependencyHealthTooltip health={DependencyHealth.Unknown}>
          <span>Test Content</span>
        </DependencyHealthTooltip>,
      )

      const content = screen.getByText('Test Content')
      await user.hover(content)

      const tooltip = await screen.findByText(
        'The health status of this dependency has not been determined or reported.',
      )
      expect(tooltip).toBeInTheDocument()
    })

    it('should show default description for invalid health value', async () => {
      const user = userEvent.setup()
      render(
        <DependencyHealthTooltip health={999 as DependencyHealth}>
          <span>Test Content</span>
        </DependencyHealthTooltip>,
      )

      const content = screen.getByText('Test Content')
      await user.hover(content)

      const tooltip = await screen.findByText(
        'The health status of this dependency has not been determined or reported.',
      )
      expect(tooltip).toBeInTheDocument()
    })
  })

  describe('children rendering', () => {
    it('should render children correctly', () => {
      render(
        <DependencyHealthTooltip health={DependencyHealth.Healthy}>
          <div data-testid="child-element">Child Content</div>
        </DependencyHealthTooltip>,
      )

      const child = screen.getByTestId('child-element')
      expect(child).toBeInTheDocument()
      expect(child).toHaveTextContent('Child Content')
    })

    it('should render multiple children', () => {
      render(
        <DependencyHealthTooltip health={DependencyHealth.Healthy}>
          <span>First</span>
          <span>Second</span>
        </DependencyHealthTooltip>,
      )

      expect(screen.getByText('First')).toBeInTheDocument()
      expect(screen.getByText('Second')).toBeInTheDocument()
    })
  })
})
