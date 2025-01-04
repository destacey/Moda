import { GlobalToken, Typography } from 'antd'
import { useMemo } from 'react'

const { Paragraph } = Typography

export interface MarkdownBlockquoteProps
  extends React.DetailedHTMLProps<
    React.BlockquoteHTMLAttributes<HTMLQuoteElement>,
    HTMLQuoteElement
  > {
  node?: any
  token: GlobalToken
}

const MarkdownBlockquote = (props: MarkdownBlockquoteProps) => {
  const blockquoteStyles = useMemo(
    () => ({
      paddingTop: '14px',
      paddingBottom: '2px',
      paddingLeft: props.token.padding,
      paddingRight: props.token.padding,
      borderLeft: `${props.token.lineWidthBold}px solid ${props.token.colorPrimary}`,
      background: props.token.colorFillTertiary, // TODO: get this closer to the actual code block color <Text code {...props} />,
    }),
    [props.token],
  )

  if (
    !props.children ||
    (typeof props.children === 'string' && props.children.trim() === '')
  ) {
    return null
  }

  return (
    <Paragraph
      {...props}
      style={{
        ...props.style,
        ...blockquoteStyles,
      }}
    >
      {props.children}
    </Paragraph>
  )
}

export default MarkdownBlockquote
