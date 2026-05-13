import {
  DefaultTimeLineColors,
  getDefaultTemplate,
  groupsStructurallyEqual,
  itemsVisuallyEqual,
} from './wayd-timeline.utils';
import { WaydDataGroup, WaydDataItem } from './types';

const group = (
  id: string | number,
  overrides: Partial<WaydDataGroup> = {},
): WaydDataGroup => ({
  id,
  content: `Group ${id}`,
  ...overrides,
});

const item = (overrides: Partial<WaydDataItem> = {}): WaydDataItem => ({
  id: 1,
  content: 'Activity',
  start: new Date('2026-01-01T00:00:00Z'),
  end: new Date('2026-01-10T00:00:00Z'),
  type: 'range',
  group: 'g1',
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

describe('itemsVisuallyEqual', () => {
  it('returns true when all relevant fields match', () => {
    expect(itemsVisuallyEqual(item(), item())).toBe(true);
  });

  it('treats Date, epoch number, and ISO string for the same instant as equal (vis-data may store dates in any of those forms)', () => {
    const iso = '2026-01-01T00:00:00.000Z';
    const epoch = new Date(iso).getTime();
    const asDate = item({ start: new Date(iso) });
    const asNumber = item({ start: epoch as unknown as Date });
    const asString = item({ start: iso as unknown as Date });
    expect(itemsVisuallyEqual(asDate, asNumber)).toBe(true);
    expect(itemsVisuallyEqual(asDate, asString)).toBe(true);
    expect(itemsVisuallyEqual(asNumber, asString)).toBe(true);
  });

  it('returns false when start changes', () => {
    expect(
      itemsVisuallyEqual(
        item(),
        item({ start: new Date('2026-01-02T00:00:00Z') }),
      ),
    ).toBe(false);
  });

  it('returns false when end changes', () => {
    expect(
      itemsVisuallyEqual(
        item(),
        item({ end: new Date('2026-01-11T00:00:00Z') }),
      ),
    ).toBe(false);
  });

  it('returns false when content (the edited activity name) changes — this is what unblocks the user-visible refresh', () => {
    expect(
      itemsVisuallyEqual(item(), item({ content: 'Renamed Activity' })),
    ).toBe(false);
  });

  it('returns false when itemColor changes', () => {
    expect(
      itemsVisuallyEqual(
        item({ itemColor: '#ff0000' }),
        item({ itemColor: '#00ff00' }),
      ),
    ).toBe(false);
  });

  it('returns false when style changes', () => {
    expect(
      itemsVisuallyEqual(
        item({ style: 'background: red' }),
        item({ style: 'background: blue' }),
      ),
    ).toBe(false);
  });

  it('returns false when className changes', () => {
    expect(
      itemsVisuallyEqual(item({ className: 'a' }), item({ className: 'b' })),
    ).toBe(false);
  });

  it('returns false when title (tooltip) changes', () => {
    expect(
      itemsVisuallyEqual(
        item({ title: 'Old tooltip' }),
        item({ title: 'New tooltip' }),
      ),
    ).toBe(false);
  });

  it('returns false when type changes', () => {
    expect(itemsVisuallyEqual(item({ type: 'range' }), item({ type: 'box' }))).toBe(
      false,
    );
  });

  it('returns false when group (row assignment) changes', () => {
    expect(itemsVisuallyEqual(item({ group: 'g1' }), item({ group: 'g2' }))).toBe(
      false,
    );
  });

  it('returns false when order changes', () => {
    expect(
      itemsVisuallyEqual(
        item({ order: 1 } as Partial<WaydDataItem>),
        item({ order: 2 } as Partial<WaydDataItem>),
      ),
    ).toBe(false);
  });

  it('ignores objectData identity — consumers should pass payload through objectData and not rely on template re-renders for unrelated reference changes (this is the core fix: RTK Query returns new object refs each refetch)', () => {
    expect(
      itemsVisuallyEqual(
        item({ objectData: { v: 1 } }),
        item({ objectData: { v: 1 } }),
      ),
    ).toBe(true);
  });

  it('treats undefined start as equal to undefined start, but not equal to a real date', () => {
    expect(
      itemsVisuallyEqual(
        item({ start: undefined as unknown as Date }),
        item({ start: undefined as unknown as Date }),
      ),
    ).toBe(true);
    expect(
      itemsVisuallyEqual(item({ start: undefined as unknown as Date }), item()),
    ).toBe(false);
  });
});
