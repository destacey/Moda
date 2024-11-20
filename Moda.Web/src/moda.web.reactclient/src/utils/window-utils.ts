// a method to determine the drawer width based on the window size
// the drawer width is calculated based on the window size
export const getDrawerWidthPercentage = (): string => {
  const width = window.innerWidth
  return width >= 1500
    ? '30%'
    : width >= 1300
      ? '35%'
      : width >= 1100
        ? '40%'
        : width >= 900
          ? '50%'
          : '80%'
}
