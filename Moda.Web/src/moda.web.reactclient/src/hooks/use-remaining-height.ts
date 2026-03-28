import { RefObject, useCallback, useEffect, useState } from 'react'

/**
 * Returns the remaining viewport height below the top of the referenced element.
 * Recalculates on window resize.
 *
 * @param ref - A ref attached to the element whose top edge defines the start of the remaining space.
 * @param bottomOffset - Optional pixel padding to subtract from the bottom (e.g., for page margins). Defaults to 30.
 * @returns The remaining height in pixels.
 *
 * @example
 * ```tsx
 * const containerRef = useRef<HTMLDivElement>(null)
 * const height = useRemainingHeight(containerRef)
 *
 * return (
 *   <div ref={containerRef} style={{ height }}>
 *     <DataGrid height={height} />
 *   </div>
 * )
 * ```
 */
export function useRemainingHeight(
  ref: RefObject<HTMLElement | null>,
  bottomOffset: number = 30,
): number {
  const [height, setHeight] = useState(500)

  const calculate = useCallback(() => {
    if (!ref.current) return
    const top = ref.current.getBoundingClientRect().top
    setHeight(Math.max(300, window.innerHeight - top - bottomOffset))
  }, [ref, bottomOffset])

  useEffect(() => {
    calculate()

    window.addEventListener('resize', calculate, { passive: true })
    return () => window.removeEventListener('resize', calculate)
  }, [calculate])

  return height
}

