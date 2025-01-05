import html2canvas, { Options } from 'html2canvas'

/**
 * Generic function to save a DOM element as an image.
 * @param {HTMLElement} element - The DOM element to capture.
 * @param {string} [fileName='image.png'] - The name of the saved image file.
 * @param {Object} [options={}] - Additional options for html2canvas.
 */
export const saveElementAsImage = async (
  element: HTMLElement,
  fileName: string = 'image.png',
  options: Partial<Options> = {},
) => {
  if (!element) {
    console.error('No element provided to capture.')
    return
  }

  try {
    // Capture the element using html2canvas
    const canvas = await html2canvas(element, {
      backgroundColor: null, // Default: transparent background
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
