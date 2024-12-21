'use client'

import { Divider, Image, Typography } from 'antd'
import useTheme from '../../contexts/theme'
import React from 'react'

const { Title, Paragraph, Text, Link: AntDLink } = Typography

export type MarkdownComponentType = Record<string, React.FC<any>>

export const markdownComponents: MarkdownComponentType = {
  h1: (props) => <Title level={1} {...props} />,
  h2: (props) => <Title level={2} {...props} />,
  h3: (props) => <Title level={3} {...props} />,
  h4: (props) => <Title level={4} {...props} />,
  h5: (props) => <Title level={5} {...props} />,
  p: (props) => <Paragraph {...props} />,
  strong: (props) => <Text strong {...props} />,
  em: (props) => <Text italic {...props} />,
  u: (props) => <Text underline {...props} />, // TODO: add to toolbar
  del: (props) => <Text delete {...props} />,
  code: (props) => <Text code {...props} />,
  pre: (props) => <CodeBlock {...props} />, // TODO: styling is still off
  blockquote: (props) => <Blockquote {...props} />,
  a: (props) => (
    <AntDLink target="_blank" rel="noopener noreferrer" {...props} />
  ),
  hr: (props) => <Divider {...props} />,
  img: (props) => <Image alt={props.alt} {...props} />, // TODO: needs improvement, especially for background
}

const Blockquote = (props) => {
  const { token } = useTheme()

  return (
    <div>
      <Paragraph
        {...props}
        style={{
          ...props.style,
          paddingTop: '14px',
          paddingBottom: '2px',
          paddingLeft: token.padding,
          paddingRight: token.padding,
          borderLeft: `${token.lineWidthBold}px solid ${token.colorPrimary}`,
          background: token.colorBgContainer,
        }}
      >
        {props.children}
      </Paragraph>
    </div>
  )
}

const CodeBlock = (props) => {
  const { token } = useTheme()

  return (
    <div
      style={{
        display: 'block',
        padding: token.padding,
        background: token.colorBgContainer,
        borderRadius: token.borderRadius,
        overflowX: 'auto',
      }}
    >
      <Paragraph
        style={{
          fontFamily: 'monospace',
          whiteSpace: 'pre',
        }}
      >
        {props.children}
      </Paragraph>
    </div>
  )
}
