/**
 * Form Validation Hook for Part Form
 * Matches WPF PartDialog validation rules
 */

import { useState, useCallback, useMemo } from 'react'
import type { CreatePartDto, UpdatePartDto } from '@/types'

export interface PartFormData {
  partNumber: string
  name: string
  description: string
  categoryId: number | null
  costPrice: string
  stockQuantity: string
  location: string
  brand: string
  model: string
  releaseYear: string
}

export interface FormErrors {
  partNumber?: string
  name?: string
  description?: string
  categoryId?: string
  costPrice?: string
  stockQuantity?: string
  location?: string
  brand?: string
  model?: string
  releaseYear?: string
  images?: string
}

const MAX_NAME_LENGTH = 100
const MAX_DESCRIPTION_LENGTH = 500
const MAX_LOCATION_LENGTH = 100
const MAX_BRAND_LENGTH = 100
const MAX_MODEL_LENGTH = 50
const MAX_IMAGES = 6
const CURRENT_YEAR = new Date().getFullYear() + 1

const initialFormData: PartFormData = {
  partNumber: '',
  name: '',
  description: '',
  categoryId: null,
  costPrice: '',
  stockQuantity: '',
  location: '',
  brand: '',
  model: '',
  releaseYear: '',
}

/**
 * Custom hook for part form validation and management
 */
export function usePartForm(initialData?: Partial<PartFormData>) {
  const [formData, setFormData] = useState<PartFormData>({
    ...initialFormData,
    ...initialData,
  })
  const [errors, setErrors] = useState<FormErrors>({})
  const [touched, setTouched] = useState<Set<keyof FormErrors>>(new Set())

  /**
   * Validate a single field
   */
  const validateField = useCallback(
    (field: keyof PartFormData, value: string | number | null): string | undefined => {
      switch (field) {
        case 'name':
          if (!value || (typeof value === 'string' && value.trim() === '')) {
            return 'Name is required'
          }
          if (value && typeof value === 'string' && value.length > MAX_NAME_LENGTH) {
            return `Name must not exceed ${MAX_NAME_LENGTH} characters`
          }
          return undefined

        case 'description':
          if (value && typeof value === 'string' && value.length > MAX_DESCRIPTION_LENGTH) {
            return `Description must not exceed ${MAX_DESCRIPTION_LENGTH} characters`
          }
          return undefined

        case 'categoryId':
          if (!value) {
            return 'Category is required'
          }
          return undefined

        case 'costPrice':
          if (!value || (typeof value === 'string' && value.trim() === '')) {
            return 'Cost price is required'
          }
          const price = typeof value === 'string' ? parseFloat(value) : value
          if (isNaN(price) || price < 0) {
            return 'Cost price must be a valid number >= 0'
          }
          return undefined

        case 'stockQuantity':
          if (!value || (typeof value === 'string' && value.trim() === '')) {
            return 'Stock quantity is required'
          }
          const stock = typeof value === 'string' ? parseInt(value, 10) : value
          if (isNaN(stock) || stock < 0) {
            return 'Stock quantity must be a valid number >= 0'
          }
          return undefined

        case 'location':
          if (value && typeof value === 'string' && value.length > MAX_LOCATION_LENGTH) {
            return `Location must not exceed ${MAX_LOCATION_LENGTH} characters`
          }
          return undefined

        case 'brand':
          if (value && typeof value === 'string' && value.length > MAX_BRAND_LENGTH) {
            return `Brand must not exceed ${MAX_BRAND_LENGTH} characters`
          }
          return undefined

        case 'model':
          if (value && typeof value === 'string' && value.length > MAX_MODEL_LENGTH) {
            return `Model must not exceed ${MAX_MODEL_LENGTH} characters`
          }
          return undefined

        case 'releaseYear':
          if (value && typeof value === 'string' && value.trim() !== '') {
            const year = parseInt(value, 10)
            if (isNaN(year) || year < 1900 || year > CURRENT_YEAR) {
              return `Release year must be between 1900 and ${CURRENT_YEAR}`
            }
          }
          return undefined

        case 'partNumber':
          // Part number is optional
          return undefined

        default:
          return undefined
      }
  },
    []
  )

  /**
   * Validate all form data
   */
  const validate = useCallback((): boolean => {
    const newErrors: FormErrors = {}

    // Validate all fields
    Object.entries(formData).forEach(([key, value]) => {
      const error = validateField(key as keyof PartFormData, value)
      if (error) {
        newErrors[key as keyof FormErrors] = error
      }
    })

    setErrors(newErrors)
    return Object.keys(newErrors).length === 0
  }, [formData, validateField])

  /**
   * Validate images
   */
  const validateImages = useCallback((tempImages: File[] | { file: File }[], existingImages: string[]): string | undefined => {
    const totalImages = tempImages.length + existingImages.length
    if (totalImages > MAX_IMAGES) {
      return `Maximum ${MAX_IMAGES} images allowed`
    }
    return undefined
  }, [])

  /**
   * Update a single field
   */
  const updateField = useCallback(
    (field: keyof PartFormData, value: string | number | null) => {
      setFormData((prev) => ({
        ...prev,
        [field]: value,
      }))

      // Clear error for this field when user starts typing
      if (errors[field]) {
        setErrors((prev) => {
          const newErrors = { ...prev }
          delete newErrors[field]
          return newErrors
        })
      }

      // Mark field as touched
      setTouched((prev) => new Set(prev).add(field))
    },
    [errors]
  )

  /**
   * Handle input change
   */
  const handleChange = useCallback(
    (field: keyof PartFormData) => (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
      updateField(field, e.target.value)
    },
    [updateField]
  )

  /**
   * Handle select change
   */
  const handleSelectChange = useCallback(
    (field: keyof PartFormData) => (value: string) => {
      if (field === 'categoryId') {
        updateField(field, value ? parseInt(value, 10) : null)
      } else {
        updateField(field, value)
      }
    },
    [updateField]
  )

  /**
   * Handle blur (validate field)
   */
  const handleBlur = useCallback(
    (field: keyof PartFormData) => () => {
      setTouched((prev) => new Set(prev).add(field))
      const error = validateField(field, formData[field])
      setErrors((prev) => ({
        ...prev,
        [field]: error,
      }))
    },
    [formData, validateField]
  )

  /**
   * Reset form
   */
  const reset = useCallback(() => {
    setFormData({ ...initialFormData, ...initialData })
    setErrors({})
    setTouched(new Set())
  }, [initialData])

  /**
   * Check if form is valid
   */
  const isValid = useMemo(() => {
    return Object.keys(errors).length === 0
  }, [errors])

  /**
   * Get form data for submission
   */
  const getSubmitData = useCallback((): CreatePartDto | UpdatePartDto => {
    return {
      partNumber: formData.partNumber.trim(),
      name: formData.name.trim(),
      description: formData.description.trim() || undefined,
      categoryId: formData.categoryId!,
      costPrice: parseFloat(formData.costPrice),
      stockQuantity: parseInt(formData.stockQuantity, 10),
      location: formData.location.trim() || undefined,
      brand: formData.brand.trim() || undefined,
      model: formData.model.trim() || undefined,
      releaseYear: formData.releaseYear ? parseInt(formData.releaseYear, 10) : undefined,
    }
  }, [formData])

  /**
   * Set form data from existing part (for edit mode)
   */
  const setPartData = useCallback((part: Partial<CreatePartDto | UpdatePartDto>) => {
    setFormData({
      partNumber: part.partNumber || '',
      name: part.name || '',
      description: part.description || '',
      categoryId: part.categoryId || null,
      costPrice: part.costPrice?.toString() || '',
      stockQuantity: part.stockQuantity?.toString() || '',
      location: part.location || '',
      brand: part.brand || '',
      model: part.model || '',
      releaseYear: part.releaseYear?.toString() || '',
    })
    setErrors({})
    setTouched(new Set())
  }, [])

  return {
    formData,
    errors,
    touched,
    isValid,
    handleChange,
    handleSelectChange,
    handleBlur,
    updateField,
    validate,
    validateImages,
    reset,
    getSubmitData,
    setPartData,
    setErrors,
  }
}

/**
 * Character count limits for validation
 */
export const FIELD_LIMITS = {
  NAME: MAX_NAME_LENGTH,
  DESCRIPTION: MAX_DESCRIPTION_LENGTH,
  LOCATION: MAX_LOCATION_LENGTH,
  BRAND: MAX_BRAND_LENGTH,
  MODEL: MAX_MODEL_LENGTH,
  MAX_IMAGES,
  MIN_YEAR: 1900,
  MAX_YEAR: CURRENT_YEAR,
}
