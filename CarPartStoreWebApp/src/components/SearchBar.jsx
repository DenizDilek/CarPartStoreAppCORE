/**
 * Search Bar Component
 * Input field for searching parts
 */

import { useState } from 'react';

/**
 * SearchBar Component
 * @param {string} value - Current search term
 * @param {(value: string) => void} onChange - Callback when search term changes
 * @param {string} placeholder - Placeholder text for input
 */
function SearchBar({ value, onChange, placeholder = 'Search...' }) {
  const [localValue, setLocalValue] = useState(value);

  // Handle input change
  const handleChange = (e) => {
    const newValue = e.target.value;
    setLocalValue(newValue);
    onChange(newValue);
  };

  // Handle clear button click
  const handleClear = () => {
    setLocalValue('');
    onChange('');
  };

  return (
    <div className="search-bar">
      <div className="search-input-wrapper">
        <span className="search-icon">🔍</span>
        <input
          type="text"
          value={localValue}
          onChange={handleChange}
          placeholder={placeholder}
          className="search-input"
          aria-label="Search parts"
        />
        {localValue && (
          <button onClick={handleClear} className="clear-btn" aria-label="Clear search">
            ✕
          </button>
        )}
      </div>
    </div>
  );
}

export default SearchBar;
