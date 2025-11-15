import { render, screen } from '@testing-library/react'
import DependencyHealthTag from './dependency-health-tag'
import { DependencyHealth } from '../../types'

describe('DependencyHealthTag', () => {
  describe('rendering', () => {
    it('should render the tag with the provided name', () => {
      render(
        <DependencyHealthTag
          name="Test Dependency"
          health={DependencyHealth.Healthy}
        />,
      )
      const tag = screen.getByText('Test Dependency')
      expect(tag).toBeInTheDocument()
    })
  })

  describe('color mapping', () => {
    it('should use success color for Healthy status', () => {
      const { container } = render(
        <DependencyHealthTag
          name="Healthy Dependency"
          health={DependencyHealth.Healthy}
        />,
      )
      const tag = container.querySelector('.ant-tag-success')
      expect(tag).toBeInTheDocument()
    })

    it('should use warning color for AtRisk status', () => {
      const { container } = render(
        <DependencyHealthTag
          name="At Risk Dependency"
          health={DependencyHealth.AtRisk}
        />,
      )
      const tag = container.querySelector('.ant-tag-warning')
      expect(tag).toBeInTheDocument()
    })

    it('should use error color for Unhealthy status', () => {
      const { container } = render(
        <DependencyHealthTag
          name="Unhealthy Dependency"
          health={DependencyHealth.Unhealthy}
        />,
      )
      const tag = container.querySelector('.ant-tag-error')
      expect(tag).toBeInTheDocument()
    })

    it('should use default color for Unknown status', () => {
      const { container } = render(
        <DependencyHealthTag
          name="Unknown Dependency"
          health={DependencyHealth.Unknown}
        />,
      )
      const tag = container.querySelector('.ant-tag-default')
      expect(tag).toBeInTheDocument()
    })
  })

  describe('edge cases', () => {
    it('should handle empty name', () => {
      const { container } = render(
        <DependencyHealthTag name="" health={DependencyHealth.Healthy} />,
      )
      const tag = container.querySelector('.ant-tag')
      expect(tag).toBeInTheDocument()
      expect(tag).toHaveTextContent('')
    })

    it('should handle long names', () => {
      const longName = 'This is a very long dependency name that might wrap'
      render(
        <DependencyHealthTag
          name={longName}
          health={DependencyHealth.Healthy}
        />,
      )
      const tag = screen.getByText(longName)
      expect(tag).toBeInTheDocument()
    })

    it('should handle special characters in name', () => {
      const specialName = 'Feature: Auth & Security (v2.0)'
      render(
        <DependencyHealthTag
          name={specialName}
          health={DependencyHealth.Healthy}
        />,
      )
      const tag = screen.getByText(specialName)
      expect(tag).toBeInTheDocument()
    })
  })
})
