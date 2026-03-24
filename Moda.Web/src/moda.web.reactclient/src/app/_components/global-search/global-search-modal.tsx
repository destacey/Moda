'use client'

import {
  FC,
  memo,
  useCallback,
  useEffect,
  useMemo,
  useRef,
  useState,
} from 'react'
import {
  Divider,
  Empty,
  Flex,
  Input,
  Modal,
  Radio,
  Spin,
  Tabs,
  Typography,
} from 'antd'
import { SearchOutlined } from '@ant-design/icons'
import { useRouter } from 'next/navigation'
import { useLazyGlobalSearchQuery } from '@/src/store/features/search/search-api'
import {
  GlobalSearchCategoryDto,
  GlobalSearchResultItemDto,
} from '@/src/services/moda-api'
import { getSearchResultUrl } from './search-result-url'
import styles from './global-search-modal.module.css'

const { Title, Text } = Typography
const { Group: RadioGroup } = Radio

interface GlobalSearchModalProps {
  open: boolean
  onClose: () => void
}

const GlobalSearchModal: FC<GlobalSearchModalProps> = memo(
  ({ open, onClose }) => {
    const router = useRouter()
    const [searchTerm, setSearchTerm] = useState('')
    const [activeTab, setActiveTab] = useState('all')
    const [activeIndex, setActiveIndex] = useState(0)
    const [maxResults, setMaxResults] = useState(5)
    const inputRef = useRef<any>(null)
    const debounceTimerRef = useRef<ReturnType<typeof setTimeout>>(undefined)
    const activeItemRef = useRef<HTMLDivElement>(null)
    const isKeyboardNavRef = useRef(false)
    // Track previous search context to reset activeIndex synchronously during render
    // instead of via a useEffect (avoids cascading state update that breaks tests)
    const prevContextRef = useRef({ searchTerm: '', activeTab: 'all' })

    const [triggerSearch, { data, isFetching }] = useLazyGlobalSearchQuery()

    // Debounced search
    useEffect(() => {
      if (!searchTerm || searchTerm.length < 2) return

      debounceTimerRef.current = setTimeout(() => {
        triggerSearch({ query: searchTerm, maxResultsPerCategory: maxResults })
      }, 300)

      return () => clearTimeout(debounceTimerRef.current)
    }, [searchTerm, maxResults, triggerSearch])

    // Reset state when opening
    useEffect(() => {
      if (open) {
        setSearchTerm('')
        setActiveTab('all')
        setActiveIndex(0)
        setTimeout(() => inputRef.current?.focus(), 100)
      }
    }, [open])

    // Compute visible items based on active tab
    const visibleItems = useMemo(() => {
      if (!data?.categories) return []

      if (activeTab === 'all') {
        return data.categories.flatMap((cat) =>
          cat.items.map((item) => ({ ...item, category: cat.name })),
        )
      }

      const category = data.categories.find((c) => c.slug === activeTab)
      return (
        category?.items.map((item) => ({
          ...item,
          category: category.name,
        })) ?? []
      )
    }, [data, activeTab])

    // Reset activeIndex synchronously during render when search context changes.
    // Using a ref comparison avoids a useEffect that would trigger a secondary
    // render (cascading state update).
    const prev = prevContextRef.current
    if (prev.searchTerm !== searchTerm || prev.activeTab !== activeTab) {
      prevContextRef.current = { searchTerm, activeTab }
      if (activeIndex !== 0) setActiveIndex(0)
    }

    // Scroll active item into view on keyboard navigation
    useEffect(() => {
      if (isKeyboardNavRef.current) {
        activeItemRef.current?.scrollIntoView({ block: 'nearest' })
        // Reset after a frame so the scroll event doesn't trigger mouse hover
        requestAnimationFrame(() => {
          isKeyboardNavRef.current = false
        })
      }
    }, [activeIndex])

    const handleNavigate = useCallback(
      (item: GlobalSearchResultItemDto) => {
        router.push(getSearchResultUrl(item))
        onClose()
      },
      [router, onClose],
    )

    // Keyboard navigation
    const handleKeyDown = useCallback(
      (e: React.KeyboardEvent) => {
        if (e.key === 'ArrowDown') {
          e.preventDefault()
          isKeyboardNavRef.current = true
          setActiveIndex((prev) =>
            prev < visibleItems.length - 1 ? prev + 1 : 0,
          )
        } else if (e.key === 'ArrowUp') {
          e.preventDefault()
          isKeyboardNavRef.current = true
          setActiveIndex((prev) =>
            prev > 0 ? prev - 1 : visibleItems.length - 1,
          )
        } else if (e.key === 'Enter' && visibleItems.length > 0) {
          e.preventDefault()
          handleNavigate(visibleItems[activeIndex])
        } else if (e.key === 'Escape') {
          e.preventDefault()
          onClose()
        }
      },
      [visibleItems, activeIndex, handleNavigate, onClose],
    )

    // Build tab items
    const tabItems = useMemo(() => {
      const categories = data?.categories ?? []
      const tabs = [
        {
          key: 'all',
          label: `All${categories.length > 0 ? ` (${categories.reduce((sum, c) => sum + c.totalCount, 0)})` : ''}`,
        },
        ...categories
          .filter((c) => c.items.length > 0)
          .map((c) => ({
            key: c.slug,
            label: `${c.name} (${c.totalCount})`,
          })),
      ]
      return tabs
    }, [data])

    // Group items by category for "All" tab
    const groupedResults = useMemo(() => {
      if (!data?.categories || activeTab !== 'all') return null
      return data.categories.filter((c) => c.items.length > 0)
    }, [data, activeTab])

    const renderResultItem = (
      item: GlobalSearchResultItemDto & { category?: string },
      index: number,
    ) => (
      <Flex
        key={`${item.entityType}-${item.key}-${index}`}
        ref={index === activeIndex ? activeItemRef : undefined}
        align="center"
        gap={12}
        className={`${styles.resultItem} ${index === activeIndex ? styles.resultItemActive : ''}`}
        onClick={() => handleNavigate(item)}
        onMouseEnter={() => {
          if (!isKeyboardNavRef.current) setActiveIndex(index)
        }}
      >
        <Text code className={styles.resultKey}>
          {item.key}
        </Text>
        <Flex vertical className={styles.resultItemContent}>
          <Text ellipsis>{item.title}</Text>
          {item.subtitle && (
            <Text type="secondary" ellipsis className={styles.resultSubtitle}>
              {item.subtitle}
            </Text>
          )}
        </Flex>
      </Flex>
    )

    const renderAllResults = (categories: GlobalSearchCategoryDto[]) => {
      let globalIndex = 0
      return categories.map((category) => (
        <div key={category.slug}>
          <Title level={5} type="secondary" className={styles.categoryHeader}>
            {category.name}
          </Title>
          {category.items.map((item) => {
            const idx = globalIndex++
            return renderResultItem({ ...item, category: category.name }, idx)
          })}
        </div>
      ))
    }

    const renderTabResults = () => {
      const category = data?.categories?.find((c) => c.slug === activeTab)
      if (!category) return null
      return category.items.map((item, index) => renderResultItem(item, index))
    }

    return (
      <Modal
        open={open}
        onCancel={onClose}
        footer={null}
        closable
        mask={{ closable: true }}
        keyboard={false}
        destroyOnHidden
        width={600}
        className={styles.searchModal}
        styles={{
          body: { padding: 0 },
        }}
      >
        <div onKeyDown={handleKeyDown}>
          <Input
            ref={inputRef}
            prefix={<SearchOutlined />}
            placeholder="Search Moda..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className={styles.searchInput}
            variant="borderless"
            size="large"
            allowClear
          />

          {isFetching && !data && (
            <Flex justify="center" style={{ padding: '32px 16px' }}>
              <Spin size="small" />
            </Flex>
          )}

          {!isFetching && searchTerm.length >= 2 && data && (
            <div className={styles.tabsContainer}>
              <Tabs
                activeKey={activeTab}
                onChange={setActiveTab}
                size="small"
                items={tabItems}
              />
              <div className={styles.resultsList}>
                {visibleItems.length === 0 ? (
                  <Empty
                    description={`No results found for "${searchTerm}"`}
                    image={Empty.PRESENTED_IMAGE_SIMPLE}
                  />
                ) : activeTab === 'all' && groupedResults ? (
                  renderAllResults(groupedResults)
                ) : (
                  renderTabResults()
                )}
              </div>
            </div>
          )}

          {!isFetching && searchTerm.length < 2 && (
            <Empty
              description="Type at least 2 characters to search"
              image={Empty.PRESENTED_IMAGE_SIMPLE}
            />
          )}

          <Divider style={{ margin: 0 }} />
          <Flex
            justify="space-between"
            align="center"
            gap={16}
            style={{ padding: '6px 16px' }}
            className={styles.shortcutHint}
          >
            <Flex gap={16}>
              <Text type="secondary">
                <kbd className={styles.kbd}>↑</kbd>
                <kbd className={styles.kbd}>↓</kbd> navigate
              </Text>
              <Text type="secondary">
                <kbd className={styles.kbd}>↵</kbd> select
              </Text>
              <Text type="secondary">
                <kbd className={styles.kbd}>esc</kbd> close
              </Text>
            </Flex>
            <Flex align="center" gap={6}>
              <Text type="secondary">Per Category:</Text>
              <RadioGroup
                size="small"
                value={maxResults}
                onChange={(e) => setMaxResults(e.target.value)}
                optionType="button"
                buttonStyle="outline"
                options={[
                  { value: 5, label: '5' },
                  { value: 10, label: '10' },
                  { value: 20, label: '20' },
                ]}
                className={styles.resultsRadioGroup}
              />
            </Flex>
          </Flex>
        </div>
      </Modal>
    )
  },
)

GlobalSearchModal.displayName = 'GlobalSearchModal'

export default GlobalSearchModal

