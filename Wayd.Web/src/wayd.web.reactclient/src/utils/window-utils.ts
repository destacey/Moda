// a method to determine the drawer width based on the window size
// the drawer width is calculated based on the window size
export const getDrawerWidthPixels = (): number => {
  const width = window.innerWidth
  return width >= 1500
    ? Math.floor(width * 0.3)
    : width >= 1300
      ? Math.floor(width * 0.35)
      : width >= 1100
        ? Math.floor(width * 0.4)
        : width >= 900
          ? Math.floor(width * 0.5)
          : Math.floor(width * 0.8)
}
