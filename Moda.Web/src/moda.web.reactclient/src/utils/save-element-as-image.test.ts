import html2canvas from 'html2canvas'
import { saveElementAsImage } from './save-element-as-image'

jest.mock('html2canvas', () => jest.fn())

describe('saveElementAsImage', () => {
  let element: HTMLElement

  beforeEach(() => {
    // Set up a dummy DOM element
    element = document.createElement('div')
    element.id = 'test-element'
    document.body.appendChild(element)

    // Mock html2canvas to return a dummy canvas
    const canvas = document.createElement('canvas')
    canvas.toDataURL = jest.fn(() => 'data:image/png;base64,dummy')
    ;(html2canvas as jest.Mock).mockResolvedValue(canvas)
  })

  afterEach(() => {
    // Clean up the DOM
    document.body.innerHTML = ''
    jest.clearAllMocks()
  })

  test('calls html2canvas with the provided element', async () => {
    await saveElementAsImage(element)

    expect(html2canvas).toHaveBeenCalledWith(element, expect.any(Object))
  })

  test('triggers a download with the correct filename', async () => {
    const createElementSpy = jest.spyOn(document, 'createElement')
    const appendChildSpy = jest.spyOn(document.body, 'appendChild')
    const removeChildSpy = jest.spyOn(document.body, 'removeChild')

    await saveElementAsImage(element, 'custom-filename.png')

    // Ensure an <a> tag is created
    expect(createElementSpy).toHaveBeenCalledWith('a')

    // Ensure the <a> tag is added to the DOM
    expect(appendChildSpy).toHaveBeenCalled()

    // Ensure the <a> tag is removed after the download
    expect(removeChildSpy).toHaveBeenCalled()

    // Ensure the download filename is set correctly
    const link = appendChildSpy.mock.calls[0][0] as HTMLAnchorElement
    expect(link.download).toBe('custom-filename.png')
    expect(link.href).toBe('data:image/png;base64,dummy')
  })

  test('handles missing element gracefully', async () => {
    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

    await saveElementAsImage(null as any)

    // Ensure html2canvas is not called
    expect(html2canvas).not.toHaveBeenCalled()

    // Ensure an error is logged
    expect(consoleErrorSpy).toHaveBeenCalledWith(
      'No element provided to capture.',
    )

    consoleErrorSpy.mockRestore()
  })

  test('logs an error if html2canvas fails', async () => {
    ;(html2canvas as jest.Mock).mockRejectedValue(new Error('Canvas error'))
    const consoleErrorSpy = jest.spyOn(console, 'error').mockImplementation()

    await saveElementAsImage(element)

    // Ensure the error is logged
    expect(consoleErrorSpy).toHaveBeenCalledWith(
      'Failed to save the element as an image:',
      expect.any(Error),
    )

    consoleErrorSpy.mockRestore()
  })
})
