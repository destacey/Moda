import {
  DefaultTimeLineColors,
  getDefaultTemplate,
  groupsStructurallyEqual,
} from './wayd-timeline.utils';
import { WaydDataGroup } from './types';

const group = (
  id: string | number,
  overrides: Partial<WaydDataGroup> = {},
): WaydDataGroup => ({
  id,
  content: `Group ${id}`,
  ...overrides,
});

describe('DefaultTimeLineColors', () => {
  it('should have light and dark themes with required color fields', () => {
    expect(DefaultTimeLineColors).toHaveProperty('light');
    expect(DefaultTimeLineColors).toHaveProperty('dark');
    expect(DefaultTimeLineColors.light.item).toHaveProperty('background');
    expect(DefaultTimeLineColors.light.item).toHaveProperty('foreground');
    expect(DefaultTimeLineColors.light.item).toHaveProperty('font');
    expect(DefaultTimeLineColors.dark.background).toHaveProperty('background');
  });
});

describe('getDefaultTemplate', () => {
  const props = {
    rangeItemTemplate: jest.fn(() => null),
    groupTemplate: jest.fn(() => null),
    data: [],
    groups: [],
    isLoading: false,
    options: { start: new Date(), end: new Date(), min: new Date(), max: new Date() },
  } as any;

  it('returns rangeItemTemplate for type "range"', () => {
    expect(getDefaultTemplate('range', props)).toBe(props.rangeItemTemplate);
  });

  it('returns groupTemplate for type "group"', () => {
    expect(getDefaultTemplate('group', props)).toBe(props.groupTemplate);
  });

  it('returns BackgroundItemTemplate for type "background"', () => {
    const result = getDefaultTemplate('background', props);
    expect(typeof result).toBe('function');
  });

  it('returns undefined for type "box"', () => {
    expect(getDefaultTemplate('box', props)).toBeUndefined();
  });

  it('returns undefined for type "point"', () => {
    expect(getDefaultTemplate('point', props)).toBeUndefined();
  });
});

describe('groupsStructurallyEqual', () => {
  it('returns true for two empty arrays', () => {
    expect(groupsStructurallyEqual([], [])).toBe(true);
  });

  it('returns false when lengths differ', () => {
    expect(groupsStructurallyEqual([group(1)], [group(1), group(2)])).toBe(
      false,
    );
  });

  it('returns true when ids and nestedGroups match exactly', () => {
    const a = [
      group('root', { nestedGroups: ['a', 'b'] }),
      group('a'),
      group('b'),
    ];
    const b = [
      group('root', { nestedGroups: ['a', 'b'] }),
      group('a'),
      group('b'),
    ];
    expect(groupsStructurallyEqual(a, b)).toBe(true);
  });

  it('returns true when ids match but array order differs (vis-timeline lays out via groupOrder, not array index)', () => {
    const a = [group(1), group(2), group(3)];
    const b = [group(3), group(1), group(2)];
    expect(groupsStructurallyEqual(a, b)).toBe(true);
  });

  it('returns true when only label/content fields change (those are patched in place via DataSet.update())', () => {
    const a = [group(1, { content: 'Old name' })];
    const b = [group(1, { content: 'New name' })];
    expect(groupsStructurallyEqual(a, b)).toBe(true);
  });

  it('returns true when style or className change but hierarchy is unchanged', () => {
    const a = [group(1, { className: 'a', style: 'color: red' })];
    const b = [group(1, { className: 'b', style: 'color: blue' })];
    expect(groupsStructurallyEqual(a, b)).toBe(true);
  });

  it('returns false when a group id is removed', () => {
    expect(
      groupsStructurallyEqual([group(1), group(2)], [group(1), group(3)]),
    ).toBe(false);
  });

  it('returns false when nestedGroups length changes (child added)', () => {
    const a = [group('root', { nestedGroups: ['a'] }), group('a')];
    const b = [
      group('root', { nestedGroups: ['a', 'b'] }),
      group('a'),
      group('b'),
    ];
    expect(groupsStructurallyEqual(a, b)).toBe(false);
  });

  it('returns false when nestedGroups membership changes (different child, same length)', () => {
    const a = [
      group('root', { nestedGroups: ['a', 'b'] }),
      group('a'),
      group('b'),
    ];
    const b = [
      group('root', { nestedGroups: ['a', 'c'] }),
      group('a'),
      group('c'),
    ];
    expect(groupsStructurallyEqual(a, b)).toBe(false);
  });

  it('returns false when nestedGroups order changes (vis-timeline tracks ordered membership for nesting)', () => {
    const a = [
      group('root', { nestedGroups: ['a', 'b'] }),
      group('a'),
      group('b'),
    ];
    const b = [
      group('root', { nestedGroups: ['b', 'a'] }),
      group('a'),
      group('b'),
    ];
    expect(groupsStructurallyEqual(a, b)).toBe(false);
  });

  it('treats missing nestedGroups as equivalent to an empty array', () => {
    const a = [group(1)];
    const b = [group(1, { nestedGroups: [] })];
    expect(groupsStructurallyEqual(a, b)).toBe(true);
  });

  it('returns false when a group is missing an id', () => {
    const a = [group(1)];
    const b = [{ content: 'no id' } as WaydDataGroup];
    expect(groupsStructurallyEqual(a, b)).toBe(false);
  });
});
