/**
 * Callback function invoked when a node in the tree has been moved via drag-and-drop.
 *
 * @param changedKey - The unique key of the node that was moved.
 * @param parentId - The unique key of the parent node. If the node is moved to the root level, this will be `null`.
 * @param index - The new zero-based index position of the node within its new parent's children array.
 */
export type NodeChangedCallback = (
  changedKey: React.Key,
  parentId: React.Key | null,
  index: number,
) => void
