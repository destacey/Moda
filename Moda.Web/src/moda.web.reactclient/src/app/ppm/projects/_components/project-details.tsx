'use client'

import { ModaDateRange, ResponsiveFlex } from '@/src/components/common'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import { ProjectDetailsDto } from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { Descriptions } from 'antd'
import Link from 'next/link'
import { FC } from 'react'

const { Item } = Descriptions

export interface ProjectDetailsProps {
  project: ProjectDetailsDto
}

const ProjectDetails: FC<ProjectDetailsProps> = ({
  project,
}: ProjectDetailsProps) => {
  if (!project) return null

  const sponsorNames =
    project?.projectSponsors.length > 0
      ? getSortedNames(project.projectSponsors)
      : 'No sponsors assigned'
  const ownerNames =
    project?.projectOwners.length > 0
      ? getSortedNames(project.projectOwners)
      : 'No owners assigned'
  const managerNames =
    project?.projectManagers.length > 0
      ? getSortedNames(project.projectManagers)
      : 'No managers assigned'

  const strategicThemes =
    project?.strategicThemes.length > 0
      ? getSortedNames(project.strategicThemes)
      : null

  return (
    <ResponsiveFlex gap="middle" align="start">
      <Descriptions column={1} size="small">
        <Item label="Portfolio">
          <Link href={`/ppm/portfolios/${project.portfolio.key}`}>
            {project.portfolio.name}
          </Link>
        </Item>
        <Item label="Program">
          {project.program && (
            <Link href={`/ppm/programs/${project.program.key}`}>
              {project.program.name}
            </Link>
          )}
        </Item>
        <Item label="Dates">
          <ModaDateRange
            dateRange={{ start: project.start, end: project.end }}
          />
        </Item>
        <Item label="Expenditure Category">
          {project.expenditureCategory.name}
        </Item>
        <Item label="Sponsors">{sponsorNames}</Item>
        <Item label="Owners">{ownerNames}</Item>
        <Item label="Managers">{managerNames}</Item>
        <Item label="Strategic Themes">{strategicThemes}</Item>
      </Descriptions>
      <Descriptions layout="vertical" size="small">
        <Item label="Description">
          <MarkdownRenderer markdown={project.description} />
        </Item>
      </Descriptions>
    </ResponsiveFlex>
  )
}

export default ProjectDetails
