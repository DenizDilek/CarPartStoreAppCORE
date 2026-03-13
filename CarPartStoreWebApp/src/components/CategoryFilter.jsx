/**
 * Category Filter Component
 * Dropdown for filtering parts by category
 * @param {Array} categories - List of available categories
 * @param {number|undefined} selectedCategoryId - Currently selected category ID
 * @param {Function} onCategoryChange - Callback when category changes
 */
function CategoryFilter({ categories, selectedCategoryId, onCategoryChange }) {
  return (
    <div className="category-filter">
      <label htmlFor="category-select" className="filter-label">
        Category:
      </label>
      <select
        id="category-select"
        value={selectedCategoryId || ''}
        onChange={(e) => onCategoryChange(e.target.value ? parseInt(e.target.value) : undefined)}
        className="category-select"
      >
        <option value="">All Categories</option>
        {categories.map((category) => (
          <option key={category.id} value={category.id}>
            {category.name}
          </option>
        ))}
      </select>
    </div>
  );
}

export default CategoryFilter;
