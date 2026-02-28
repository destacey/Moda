/**
 * Generic function to save a DOM element as an image.
 * @param {HTMLElement} element - The DOM element to capture.
 * @param {string} [fileName='image.png'] - The name of the saved image file.
 * @param {Object} [options={}] - Additional options for html2canvas.
 */
export const saveElementAsImage = async (
  element: HTMLElement,
  fileName: string = 'image.png',
  options: Record<string, unknown> = {},
) => {
  if (!element) {
    console.error('No element provided to capture.')
    return
  }

  try {
    const { default: html2canvas } = await import('html2canvas')

    // Capture the element using html2canvas
    const canvas = await html2canvas(element, {
      backgroundColor: null, // Default: transparent background
      scale: Math.max(window.devicePixelRatio || 1, 2), // Always capture at minimum 2x for crisp images
      ...options, // Allow custom options to override defaults
    })

    // Convert the canvas to an image
    const image = canvas.toDataURL('image/png')

    // Create a temporary link element to trigger download
    const link = document.createElement('a')
    link.href = image
    link.download = fileName

    // Append to the document to trigger download
    document.body.appendChild(link)
    link.click()

    // Clean up: Remove the link from the DOM
    document.body.removeChild(link)
  } catch (error) {
    console.error('Failed to save the element as an image:', error)
  }
}
