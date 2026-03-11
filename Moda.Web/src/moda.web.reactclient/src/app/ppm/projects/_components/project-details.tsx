'use client'

import { ModaDateRange, ResponsiveFlex } from '@/src/components/common'
import LinksCard from '@/src/components/common/links/links-card'
import { MarkdownRenderer } from '@/src/components/common/markdown'
import { ProjectDetailsDto } from '@/src/services/moda-api'
import { getSortedNames } from '@/src/utils'
import { Descriptions, Flex } from 'antd'
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
      : 'No sponsor assigned'
  const ownerNames =
    project?.projectOwners.length > 0
      ? getSortedNames(project.projectOwners)
      : 'No owner assigned'
  const managerNames =
    project?.projectManagers.length > 0
      ? getSortedNames(project.projectManagers)
      : 'No manager assigned'
  const memberNames =
    project?.projectMembers.length > 0
      ? getSortedNames(project.projectMembers)
      : 'No members assigned'

  const strategicThemes =
    project?.strategicThemes.length > 0
      ? getSortedNames(project.strategicThemes)
      : null

  return (
    <Flex vertical gap="middle">
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
          <Item label="Members">{memberNames}</Item>
          <Item label="Strategic Themes">{strategicThemes}</Item>
        </Descriptions>
        <Descriptions layout="vertical" size="small">
          <Item label="Description">
            <MarkdownRenderer markdown={project.description} />
          </Item>
        </Descriptions>
      </ResponsiveFlex>
      <LinksCard objectId={project.id} />
    </Flex>
  )
}

export default ProjectDetails
