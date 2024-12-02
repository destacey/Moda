import { Card, GlobalToken, Typography } from 'antd'
import Link from 'next/link'
import { useMemo } from 'react'
import { ModaOrganizationChartNodeProps } from '../../components/common/organization-chart'

const { Text, Title } = Typography

const OrganizationalChartTeamNode: React.FC<ModaOrganizationChartNodeProps> = ({
  data,
  themeToken,
}) => {
  const { key, name, type } = data

  const colorMap = useMemo(
    () => ({
      'Team of Teams': themeToken.colorPrimary,
      Team: '#87c9a7',
    }),
    [themeToken],
  )

  const teamLink =
    type.name === 'Team'
      ? `/organizations/teams/${key}`
      : `/organizations/team-of-teams/${key}`

  return (
    <Card
      style={{
        height: 'inherit',
        width: 'inherit',
        borderRadius: 4,
        padding: 0,
        boxShadow: `0 2px 5px ${themeToken.colorBgElevated === '#ffffff' ? 'rgba(0, 0, 0, 0.1)' : 'rgba(0, 0, 0, 0.3)'}`,
        border: 'none',
        position: 'relative',
        backgroundColor: themeToken.colorBgContainer,
      }}
      styles={{
        body: {
          height: '100%',
          padding: 0,
        },
      }}
    >
      <div
        style={{
          width: '100%',
          height: 4,
          backgroundColor: colorMap[type.name] || themeToken.colorBgContainer,
          borderRadius: '4px 4px 0 0',
        }}
      />
      <div
        style={{
          height: 'calc(100% - 4px)',
          padding: '4px 16px',
          display: 'flex',
          flexDirection: 'column',
          justifyContent: 'center',
        }}
      >
        <Title
          level={5}
          ellipsis
          style={{
            margin: 0,
            marginBottom: 8,
            color: themeToken.colorText,
          }}
          title={name}
        >
          {name}
        </Title>
        <Text
          ellipsis
          style={{
            display: 'block',
            fontSize: 13,
            color: themeToken.colorTextSecondary,
          }}
        >
          {`${type.name} - `}
          <Link href={teamLink}>{key}</Link>
        </Text>
      </div>
    </Card>
  )
}

export default OrganizationalChartTeamNode
