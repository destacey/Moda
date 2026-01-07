import { Descriptions, Flex, Typography } from 'antd'

const { Title, Text } = Typography
const { Item: DescriptionItem } = Descriptions

export const ProjectTasksHelp = () => (
  <Flex vertical gap="large" style={{ width: 400 }}>
    <Title level={4}>Keyboard Shortcuts</Title>
    <Descriptions
      size="small"
      column={1}
      styles={{ header: { marginBottom: 4 } }}
      title="Inline Editing"
    >
      <DescriptionItem label="Click row or cell">
        <Text>Enter edit mode</Text>
      </DescriptionItem>
      <DescriptionItem label="Enter / ↓">
        <Text>Save and move to next row</Text>
      </DescriptionItem>
      <DescriptionItem label="↑">
        <Text>Save and move to previous row</Text>
      </DescriptionItem>
      <DescriptionItem label="Tab">
        <Text>Move to next field (wraps to next row)</Text>
      </DescriptionItem>
      <DescriptionItem label="Shift + Tab">
        <Text>Move to previous field (wraps to previous row)</Text>
      </DescriptionItem>
      <DescriptionItem label="Esc">
        <Text>Cancel changes and exit edit mode</Text>
      </DescriptionItem>
      <DescriptionItem label="Click outside table">
        <Text>Save changes and exit edit mode</Text>
      </DescriptionItem>
    </Descriptions>

    <Descriptions
      size="small"
      column={1}
      title="Dropdown Navigation"
      styles={{ header: { marginBottom: 4 } }}
    >
      <DescriptionItem label="Space">
        <Text>Open dropdown</Text>
      </DescriptionItem>
      <DescriptionItem label="↑ / ↓">
        <Text>Navigate options in open dropdown</Text>
      </DescriptionItem>
      <DescriptionItem label="Enter">
        <Text>Select highlighted option</Text>
      </DescriptionItem>
    </Descriptions>

    <Title level={4}>Drag and Drop</Title>
    <Descriptions
      size="small"
      column={1}
      styles={{ header: { marginBottom: 4 } }}
    >
      <DescriptionItem label="Drag handle">
        <Text>Click and drag the grip icon to move a task</Text>
      </DescriptionItem>
      <DescriptionItem label="Vertical drag">
        <Text>Move task up or down in the list</Text>
      </DescriptionItem>
      <DescriptionItem label="Drag right">
        <Text>Nest task under the item above it</Text>
      </DescriptionItem>
      <DescriptionItem label="Drag left">
        <Text>Move task to a shallower level</Text>
      </DescriptionItem>
      <DescriptionItem label="Esc">
        <Text>Cancel drag operation</Text>
      </DescriptionItem>
    </Descriptions>
  </Flex>
)
