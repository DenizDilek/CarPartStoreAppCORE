/**
 * Part Form Component
 * Form for adding and editing car parts
 */

import { useState, useEffect } from 'react';
import { useCreatePart, useUpdatePart } from '../hooks/useParts';

/**
 * PartForm Component
 * @param {CarPartDto | null} part - Part to edit (null for new part)
 * @param {CategoryDto[]} categories - Available categories
 * @param {() => void} onSubmit - Callback when form is submitted
 * @param {() => void} onCancel - Callback when form is cancelled
 */
function PartForm({ part, categories, onSubmit, onCancel }) {
  const isEditMode = part !== null;

  // Form state
  const [formData, setFormData] = useState({
    partNumber: '',
    name: '',
    description: '',
    categoryId: categories.length > 0 ? categories[0].id : 0,
    costPrice: 0,
    retailPrice: 0,
    stockQuantity: 0,
    location: '',
    supplier: '',
    imagePath: '',
  });

  // Validation errors
  const [errors, setErrors] = useState({});

  // Mutations
  const createPartMutation = useCreatePart();
  const updatePartMutation = useUpdatePart();

  // Populate form when editing
  useEffect(() => {
    if (part) {
      setFormData({
        partNumber: part.partNumber || '',
        name: part.name || '',
        description: part.description || '',
        categoryId: part.categoryId || (categories.length > 0 ? categories[0].id : 0),
        costPrice: part.costPrice || 0,
        retailPrice: part.retailPrice || 0,
        stockQuantity: part.stockQuantity || 0,
        location: part.location || '',
        supplier: part.supplier || '',
        imagePath: part.imagePath || '',
      });
    }
  }, [part, categories]);

  // Validate form
  const validateForm = () => {
    const newErrors = {};

    if (!formData.partNumber.trim()) {
      newErrors.partNumber = 'Part number is required';
    } else if (formData.partNumber.length > 50) {
      newErrors.partNumber = 'Part number must be 50 characters or less';
    }

    if (!formData.name.trim()) {
      newErrors.name = 'Name is required';
    } else if (formData.name.length > 100) {
      newErrors.name = 'Name must be 100 characters or less';
    }

    if (formData.description && formData.description.length > 500) {
      newErrors.description = 'Description must be 500 characters or less';
    }

    if (formData.costPrice < 0) {
      newErrors.costPrice = 'Cost price must be 0 or greater';
    }

    if (formData.retailPrice < 0) {
      newErrors.retailPrice = 'Retail price must be 0 or greater';
    }

    if (formData.stockQuantity < 0) {
      newErrors.stockQuantity = 'Stock quantity must be 0 or greater';
    }

    if (formData.location && formData.location.length > 100) {
      newErrors.location = 'Location must be 100 characters or less';
    }

    if (formData.supplier && formData.supplier.length > 255) {
      newErrors.supplier = 'Supplier must be 255 characters or less';
    }

    if (formData.imagePath && formData.imagePath.length > 255) {
      newErrors.imagePath = 'Image path must be 255 characters or less';
    }

    setErrors(newErrors);
    return Object.keys(newErrors).length === 0;
  };

  // Handle form submission
  const handleSubmit = async (e) => {
    e.preventDefault();

    if (!validateForm()) {
      return;
    }

    try {
      if (isEditMode) {
        await updatePartMutation.mutateAsync({
          id: part.id,
          data: formData,
        });
      } else {
        await createPartMutation.mutateAsync(formData);
      }
      onSubmit();
    } catch (error) {
      console.error('Failed to save part:', error);
      alert(`Failed to ${isEditMode ? 'update' : 'create'} part. Please try again.`);
    }
  };

  // Handle input change
  const handleChange = (e) => {
    const { name, value, type } = e.target;
    setFormData((prev) => ({
      ...prev,
      [name]: type === 'number' ? parseFloat(value) || 0 : value,
    }));

    // Clear error for this field
    if (errors[name]) {
      setErrors((prev) => {
        const newErrors = { ...prev };
        delete newErrors[name];
        return newErrors;
      });
    }
  };

  // Check if form is submitting
  const isSubmitting =
    createPartMutation.isPending || updatePartMutation.isPending;

  return (
    <form onSubmit={handleSubmit} className="part-form">
      <div className="form-grid">
        {/* Part Number */}
        <div className="form-group">
          <label htmlFor="partNumber">
            Part Number <span className="required">*</span>
          </label>
          <input
            type="text"
            id="partNumber"
            name="partNumber"
            value={formData.partNumber}
            onChange={handleChange}
            className={`form-input ${errors.partNumber ? 'input-error' : ''}`}
            placeholder="e.g., BRAKE-001"
            maxLength={50}
            disabled={isSubmitting}
          />
          {errors.partNumber && <span className="error-message">{errors.partNumber}</span>}
        </div>

        {/* Name */}
        <div className="form-group">
          <label htmlFor="name">
            Name <span className="required">*</span>
          </label>
          <input
            type="text"
            id="name"
            name="name"
            value={formData.name}
            onChange={handleChange}
            className={`form-input ${errors.name ? 'input-error' : ''}`}
            placeholder="e.g., Front Brake Pads"
            maxLength={100}
            disabled={isSubmitting}
          />
          {errors.name && <span className="error-message">{errors.name}</span>}
        </div>

        {/* Description */}
        <div className="form-group">
          <label htmlFor="description">Description</label>
          <textarea
            id="description"
            name="description"
            value={formData.description}
            onChange={handleChange}
            className={`form-textarea ${errors.description ? 'input-error' : ''}`}
            placeholder="Part description or notes..."
            rows={3}
            maxLength={500}
            disabled={isSubmitting}
          />
          {errors.description && (
            <span className="error-message">{errors.description}</span>
          )}
        </div>

        {/* Category */}
        <div className="form-group">
          <label htmlFor="categoryId">
            Category <span className="required">*</span>
          </label>
          <select
            id="categoryId"
            name="categoryId"
            value={formData.categoryId}
            onChange={handleChange}
            className="form-select"
            disabled={isSubmitting}
          >
            {categories.map((category) => (
              <option key={category.id} value={category.id}>
                {category.name}
              </option>
            ))}
          </select>
        </div>

        {/* Cost Price */}
        <div className="form-group">
          <label htmlFor="costPrice">
            Cost Price ($) <span className="required">*</span>
          </label>
          <input
            type="number"
            id="costPrice"
            name="costPrice"
            value={formData.costPrice}
            onChange={handleChange}
            step="0.01"
            min="0"
            className={`form-input ${errors.costPrice ? 'input-error' : ''}`}
            placeholder="0.00"
            disabled={isSubmitting}
          />
          {errors.costPrice && (
            <span className="error-message">{errors.costPrice}</span>
          )}
        </div>

        {/* Retail Price */}
        <div className="form-group">
          <label htmlFor="retailPrice">
            Retail Price ($) <span className="required">*</span>
          </label>
          <input
            type="number"
            id="retailPrice"
            name="retailPrice"
            value={formData.retailPrice}
            onChange={handleChange}
            step="0.01"
            min="0"
            className={`form-input ${errors.retailPrice ? 'input-error' : ''}`}
            placeholder="0.00"
            disabled={isSubmitting}
          />
          {errors.retailPrice && (
            <span className="error-message">{errors.retailPrice}</span>
          )}
        </div>

        {/* Stock Quantity */}
        <div className="form-group">
          <label htmlFor="stockQuantity">
            Stock Quantity <span className="required">*</span>
          </label>
          <input
            type="number"
            id="stockQuantity"
            name="stockQuantity"
            value={formData.stockQuantity}
            onChange={handleChange}
            min="0"
            className={`form-input ${errors.stockQuantity ? 'input-error' : ''}`}
            placeholder="0"
            disabled={isSubmitting}
          />
          {errors.stockQuantity && (
            <span className="error-message">{errors.stockQuantity}</span>
          )}
        </div>

        {/* Location */}
        <div className="form-group">
          <label htmlFor="location">Location</label>
          <input
            type="text"
            id="location"
            name="location"
            value={formData.location}
            onChange={handleChange}
            className={`form-input ${errors.location ? 'input-error' : ''}`}
            placeholder="e.g., Shelf A-1"
            maxLength={100}
            disabled={isSubmitting}
          />
          {errors.location && (
            <span className="error-message">{errors.location}</span>
          )}
        </div>

        {/* Supplier */}
        <div className="form-group">
          <label htmlFor="supplier">Supplier</label>
          <input
            type="text"
            id="supplier"
            name="supplier"
            value={formData.supplier}
            onChange={handleChange}
            className={`form-input ${errors.supplier ? 'input-error' : ''}`}
            placeholder="e.g., AutoParts Inc."
            maxLength={255}
            disabled={isSubmitting}
          />
          {errors.supplier && (
            <span className="error-message">{errors.supplier}</span>
          )}
        </div>

        {/* Image Path */}
        <div className="form-group">
          <label htmlFor="imagePath">Image Path</label>
          <input
            type="text"
            id="imagePath"
            name="imagePath"
            value={formData.imagePath}
            onChange={handleChange}
            className={`form-input ${errors.imagePath ? 'input-error' : ''}`}
            placeholder="/path/to/image.jpg"
            maxLength={255}
            disabled={isSubmitting}
          />
          {errors.imagePath && (
            <span className="error-message">{errors.imagePath}</span>
          )}
        </div>
      </div>

      {/* Form Actions */}
      <div className="form-actions">
        <button
          type="button"
          onClick={onCancel}
          className="btn btn-secondary"
          disabled={isSubmitting}
        >
          Cancel
        </button>
        <button
          type="submit"
          className="btn btn-primary"
          disabled={isSubmitting}
        >
          {isSubmitting ? 'Saving...' : isEditMode ? 'Update Part' : 'Add Part'}
        </button>
      </div>

      {/* Required fields notice */}
      <p className="required-notice">
        <span className="required">*</span> Required fields
      </p>
    </form>
  );
}

export default PartForm;
