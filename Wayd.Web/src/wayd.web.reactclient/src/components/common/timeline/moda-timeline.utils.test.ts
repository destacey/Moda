import { DefaultTimeLineColors, getDefaultTemplate } from './moda-timeline.utils';

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
  };

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
